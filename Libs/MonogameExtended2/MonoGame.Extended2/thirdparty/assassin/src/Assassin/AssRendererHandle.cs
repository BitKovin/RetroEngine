using System;
using System.Runtime.InteropServices;
using Assassin.Native;

namespace Assassin;

public sealed class AssRendererHandle : SafeHandle, ITypedHandle<ASS_Renderer>
{

    public AssRendererHandle(IntPtr handle)
        : base(IntPtr.Zero, true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.ass_renderer_done(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    internal unsafe ASS_Renderer* GetTypedPointer()
    {
        return (ASS_Renderer*)handle;
    }

    unsafe ASS_Renderer* ITypedHandle<ASS_Renderer>.GetTypedPointer()
    {
        return GetTypedPointer();
    }

}
