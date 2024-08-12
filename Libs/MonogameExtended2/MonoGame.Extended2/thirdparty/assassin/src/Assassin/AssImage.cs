using System;
using System.Diagnostics.CodeAnalysis;
using Assassin.Native;

namespace Assassin;

/// <summary>
/// An image rendered by libass. This struct must be consumed immediately using <see cref="Blend()"/> or <see cref="Blend(RgbaImage)"/>.
/// </summary>
public readonly ref struct AssImage
{

    internal AssImage(int rendererWidth, int rendererHeight, IntPtr nativePointer)
    {
        _rendererWidth = rendererWidth;
        _rendererHeight = rendererHeight;
        _nativePointer = nativePointer;
    }

    public RgbaImage Blend()
    {
        var imageBuffer = new RgbaImage(_rendererWidth, _rendererHeight);

        if (_nativePointer != IntPtr.Zero)
        {
            Blend(imageBuffer);
        }

        return imageBuffer;
    }

    public void Blend(RgbaImage rgbaImage)
    {
        if (rgbaImage is null)
        {
            throw new ArgumentNullException(nameof(rgbaImage));
        }

        if (rgbaImage.Width < _rendererWidth || rgbaImage.Height < _rendererHeight)
        {
            throw new ArgumentException("Cannot blend into a buffer smaller than renderer size.", nameof(rgbaImage));
        }

        if (_nativePointer == IntPtr.Zero)
        {
            // The subtitle image is empty, so skip blending with video image.
            return;
        }

        unsafe
        {
            var image = (ASS_Image*)_nativePointer;

            fixed (Color32Rgba* buffer = rgbaImage.Buffer)
            {
                Blend(buffer, rgbaImage.Width, rgbaImage.Height, image);
            }
        }
    }

    public unsafe void Blend(Color32Rgba* buffer, int bufferWidth, int bufferHeight)
    {
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (bufferWidth < _rendererWidth || bufferHeight < _rendererHeight)
        {
            throw new ArgumentException("Cannot blend into a buffer smaller than renderer size.", nameof(buffer));
        }

        if (_nativePointer == IntPtr.Zero)
        {
            // The subtitle image is empty, so skip blending with video image.
            return;
        }

        var image = (ASS_Image*)_nativePointer;

        Blend(buffer, bufferWidth, bufferHeight, image);
    }

    private static unsafe void Blend(Color32Rgba* buffer, int bufferWidth, int bufferHeight, [MaybeNull] ASS_Image* image)
    {
        while (image is not null)
        {
            BlendSingle(buffer, bufferWidth, bufferHeight, image);

            var next = (ASS_Image*)image->Next;

            image = next;
        }
    }

    private static unsafe void BlendSingle(Color32Rgba* buffer, int bufferWidth, int bufferHeight, [NotNull] ASS_Image* image)
    {
        var w = image->Width;
        var h = image->Height;
        var s = image->Stride;

        if (w == 0 || h == 0)
        {
            // "w/h can be zero, in this case the bitmap should not be rendered at all."
            return;
        }

        if (bufferWidth < w || bufferHeight < h)
        {
            throw new AssException("Cannot render an ASS_Image to a buffer smaller than it");
        }

        var assLayerColor = stackalloc Color32Abgr[1];

        assLayerColor->LoadUInt32(image->Color);

        assLayerColor->A = (byte)(byte.MaxValue - assLayerColor->A);

        var opacity = assLayerColor->A / (float)byte.MaxValue;

        var src = (byte*)image->Bitmap;

        for (var y = 0; y < h; y += 1)
        {
            // "The last bitmap row is not guaranteed to be padded up to stride size, e.g. in the worst case a bitmap has the size stride * (h - 1) + w."
            var width = y < h - 1 ? w : s;

            for (var x = 0; x < width; x += 1)
            {
                var srcIndex = y * image->Stride + x;
                var dstIndex = (y + image->DestY) * bufferWidth + (x + image->DestX);

                var srcAlpha = src[srcIndex] * opacity / byte.MaxValue;
                var dstAlpha = 1.0f - srcAlpha;

                Color32ImageBlending.BlendPixel(assLayerColor, srcAlpha, buffer + dstIndex, dstAlpha, buffer + dstIndex);
            }
        }
    }

    private readonly int _rendererWidth;
    private readonly int _rendererHeight;
    private readonly IntPtr _nativePointer;

}
