using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Assassin.Native;

namespace Assassin;

public class AssLibrary : IDisposableEx, INativeResource<AssLibraryHandle>
{

    private AssLibrary(AssLibraryHandle safeHandle)
    {
        SafeHandle = safeHandle;
    }

    ~AssLibrary()
    {
        Dispose(false);
    }

    public static AssLibrary Create()
    {
        var handle = NativeMethods.ass_library_init();

        if (handle == IntPtr.Zero)
        {
            throw new AssException("Cannot initialize library");
        }

        var safeHandle = new AssLibraryHandle(handle);

        return new AssLibrary(safeHandle);
    }

    public AssRenderer CreateRenderer()
    {
        this.EnsureNotDisposed();

        var rendererHandle = NativeMethods.ass_renderer_init(SafeHandle.DangerousGetHandle());

        if (rendererHandle == IntPtr.Zero)
        {
            throw new AssException("Cannot create renderer");
        }

        var safeHandle = new AssRendererHandle(rendererHandle);

        return new AssRenderer(safeHandle);
    }

    public AssTrack CreateTrack(IAssSource source)
    {
        this.EnsureNotDisposed();

        var track = source.CreateTrack(this);

        if (track == null)
        {
            throw new AssException("Failed to create track");
        }

        return track;
    }

    [Obsolete("This method may cause memory leak.")]
    public DefaultFontProvider[] GetAvailableFontProviders()
    {
        this.EnsureNotDisposed();

        DefaultFontProvider[] result;

        unsafe
        {
            NativeMethods.ass_get_available_font_providers(SafeHandle.DangerousGetHandle(), out var providers, out var size);

            var s = size.ToUInt32();
            var typedProviders = (DefaultFontProvider*)providers;

            Trace.Assert(typedProviders != null);

            result = new DefaultFontProvider[s];

            for (var i = 0; i < s; i += 1)
            {
                result[i] = typedProviders[i];
            }

            // AllocHGlobal/FreeHGlobal is just a wrapper for GlobalAlloc/GlobalFree (LocalAlloc/LocalFree),
            // which calls HeapAlloc/HeapFree in the end. malloc/free may not work like this; different CRTs
            // (e.g. Cygwin's and VC++'s) may be implemented in different ways. In short, this call does not
            // guarantee correct memory freeing.
            Marshal.FreeHGlobal(providers);
        }

        return result;
    }

    public void Dispose()
    {
        Dispose(true);
    }

    public bool IsDisposed { get; private set; }

    public AssLibraryHandle SafeHandle { get; }

    private void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        if (disposing)
        {
            SafeHandle.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}
