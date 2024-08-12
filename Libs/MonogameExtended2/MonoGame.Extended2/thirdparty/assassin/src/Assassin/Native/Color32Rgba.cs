using System.Runtime.InteropServices;

namespace Assassin.Native;

[StructLayout(LayoutKind.Explicit)]
public struct Color32Rgba
{

    [FieldOffset(0)]
    public byte R;

    [FieldOffset(1)]
    public byte G;

    [FieldOffset(2)]
    public byte B;

    [FieldOffset(3)]
    public byte A;

    [FieldOffset(0)]
    public uint Packed;

    internal void LoadUInt32(uint color)
    {
        R = (byte)(color & 0xff);
        G = (byte)((color >> 8) & 0xff);
        B = (byte)((color >> 16) & 0xff);
        A = (byte)((color >> 24) & 0xff);
    }

    internal static Color32Rgba FromUInt32(uint color)
    {
        var r = new Color32Rgba();
        r.LoadUInt32(color);
        return r;
    }

    public override string ToString()
    {
        return $"{{R={R}, G={G}, B={B}, A={A}}}";
    }

}
