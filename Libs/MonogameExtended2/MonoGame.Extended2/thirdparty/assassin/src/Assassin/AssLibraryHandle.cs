using System;
using System.Runtime.InteropServices;
using Assassin.Native;

namespace Assassin;

public sealed class AssLibraryHandle : SafeHandle, ITypedHandle<ASS_Library>
{

    internal AssLibraryHandle(IntPtr handle)
        : base(IntPtr.Zero, true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.ass_library_done(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    internal unsafe ASS_Library* GetTypedPointer()
    {
        return (ASS_Library*)handle;
    }

    unsafe ASS_Library* ITypedHandle<ASS_Library>.GetTypedPointer()
    {
        return GetTypedPointer();
    }

}
