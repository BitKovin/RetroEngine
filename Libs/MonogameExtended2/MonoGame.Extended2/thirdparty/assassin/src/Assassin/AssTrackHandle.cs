using System;
using System.Runtime.InteropServices;
using Assassin.Native;

namespace Assassin;

public sealed class AssTrackHandle : SafeHandle, ITypedHandle<ASS_Track>
{

    public AssTrackHandle(IntPtr handle)
        : base(IntPtr.Zero, true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.ass_free_track(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    internal unsafe ASS_Track* GetTypedPointer()
    {
        return (ASS_Track*)handle;
    }

    unsafe ASS_Track* ITypedHandle<ASS_Track>.GetTypedPointer()
    {
        return GetTypedPointer();
    }

}
