using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using Assassin.Native;
using JetBrains.Annotations;

namespace Assassin;

public sealed class RgbaImage
{

    public RgbaImage(int width, int height)
    {
        Width = width;
        Height = height;

        _buffer = new Color32Rgba[width * height];
    }

    public int Width
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    public int Height
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
    }

    public Color32Rgba[] Buffer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        get => _buffer;
    }

    public Color32Rgba ReadPixel(int x, int y)
    {
        if (x < 0 || x >= Width)
        {
            throw new ArgumentOutOfRangeException(nameof(x), x, null);
        }

        if (y < 0 || y >= Height)
        {
            throw new ArgumentOutOfRangeException(nameof(y), y, null);
        }

        var index = Width * y + x;

        return _buffer[index];
    }

    private readonly Color32Rgba[] _buffer;

}
