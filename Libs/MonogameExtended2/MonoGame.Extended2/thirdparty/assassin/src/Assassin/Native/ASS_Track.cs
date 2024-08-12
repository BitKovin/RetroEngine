using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Assassin.Native;

[StructLayout(LayoutKind.Sequential)]
// ReSharper disable once InconsistentNaming
internal unsafe struct ASS_Track
{

    public int StyleCount;

    public int MaxStyleCount;

    public int EventCount;

    public int MaxEventCount;

    [NotNull]
    public AssStyle* Styles;

    [NotNull]
    public AssEvent* Events;

    public IntPtr StyleFormat;

    public IntPtr EventFormat;

    public TrackType TrackType;

    public int PlayerResolutionX;

    public int PlayerResolutionY;

    public double Timer;

    public int WrapStyle;

    public int ScaledBorderAndShadow;

    public int Kerning;

    public IntPtr Language;

    public YCbCrMatrix YCbCrMatrix;

    public int DefaultStyle;

    public IntPtr Name;

    public IntPtr Library;

    public IntPtr ParserPriv;

}
