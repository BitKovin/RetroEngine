using System;
using System.Runtime.InteropServices;

namespace Assassin.Native;

// DO NOT PACK!
[StructLayout(LayoutKind.Sequential)]
// ReSharper disable once InconsistentNaming
internal struct ASS_Image
{

    public int Width;

    public int Height;

    public int Stride;

    public IntPtr Bitmap;

    public uint Color;

    public int DestX;

    public int DestY;

    public IntPtr Next;

    public ImageType Type;

}
