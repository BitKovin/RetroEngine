using System;
using System.IO;
using System.Runtime.InteropServices;
using Assassin.Native;

namespace Assassin;

public sealed class MemoryAssSource : IAssSource
{

    public MemoryAssSource(byte[] data, string? codePage = null)
    {
        _data = (byte[])data.Clone();
        _codePage = codePage;
    }

    public MemoryAssSource(MemoryStream stream, string? codePage = null)
    {
        _data = stream.ToArray();
        _codePage = codePage;
    }

    public MemoryAssSource(Stream stream, string? codePage = null)
    {
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);

            _data = memoryStream.ToArray();
        }

        _codePage = codePage;
    }

    public AssTrack CreateTrack(AssLibrary library)
    {
        var buffer = Marshal.AllocHGlobal(_data.Length);

        Marshal.Copy(_data, 0, buffer, _data.Length);

        var trackPtr = NativeMethods.ass_read_memory(library.SafeHandle.DangerousGetHandle(), buffer, _data.Length, _codePage);

        Marshal.FreeHGlobal(buffer);

        if (trackPtr == IntPtr.Zero)
        {
            throw new AssException("Cannot create track from memory");
        }

        var handle = new AssTrackHandle(trackPtr);

        return new AssTrack(handle);
    }

    private readonly byte[] _data;

    private readonly string? _codePage;

}
