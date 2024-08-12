using System;
using System.Runtime.InteropServices;

namespace Assassin.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct AssEvent
{

    public long Start;

    public long Duration;

    public int ReadOrder;

    public int Layer;

    public int Style;

    public IntPtr Name;

    public int MarginL;

    public int MarginR;

    public int MarginV;

    public IntPtr Effect;

    public IntPtr Text;

    public IntPtr RenderPriv;

}
