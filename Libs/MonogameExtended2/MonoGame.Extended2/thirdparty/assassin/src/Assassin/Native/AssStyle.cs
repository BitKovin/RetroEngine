using System;
using System.Runtime.InteropServices;

namespace Assassin.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct AssStyle
{

    public IntPtr Name;

    public IntPtr FontName;

    public double FontSize;

    public Color32Abgr PrimaryColor;

    public Color32Abgr SecondaryColor;

    public Color32Abgr OutlineColor;

    public Color32Abgr BackColor;

    [MarshalAs(UnmanagedType.Bool)]
    public bool Bold;

    [MarshalAs(UnmanagedType.Bool)]
    public bool Italic;

    [MarshalAs(UnmanagedType.Bool)]
    public bool Underline;

    [MarshalAs(UnmanagedType.Bool)]
    public bool StrikeOut;

    public double ScaleX;

    public double ScaleY;

    public double Spacing;

    public double Angle;

    public int BorderStyle;

    public double Outline;

    public double Shadow;

    public int Alignment;

    public int MarginL;

    public int MarginR;

    public int MarginV;

    public int Encoding;

    [MarshalAs(UnmanagedType.Bool)]
    public bool TreatFontNameAsPattern;

    public double Blur;

    public int Justify;

}
