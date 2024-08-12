using System;
using Assassin.Native;

namespace Assassin;

public sealed class FileAssSource : IAssSource
{

    public FileAssSource(string fileName, string? codePage = null)
    {
        _fileName = fileName;
        _codePage = codePage;
    }

    public AssTrack CreateTrack(AssLibrary library)
    {
        var trackPtr = NativeMethods.ass_read_file(library.SafeHandle.DangerousGetHandle(), _fileName, _codePage);

        if (trackPtr == IntPtr.Zero)
        {
            throw new AssException("Cannot create track from file");
        }

        var safeHandle = new AssTrackHandle(trackPtr);

        return new AssTrack(safeHandle);
    }

    private readonly string _fileName;

    private readonly string? _codePage;

}
