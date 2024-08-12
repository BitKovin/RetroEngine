using System;
using System.Text;
using Assassin.Native;

namespace Assassin;

public sealed class StringAssSource : IAssSource
{

    public StringAssSource(string str, string? codePage = null)
    {
        Encoding encoding;

        if (codePage == null)
        {
            encoding = Utilities.Utf8;
        }
        else
        {
            encoding = Encoding.GetEncoding(codePage);
        }

        _codePage = codePage;
        _str = str;
        _encoding = encoding;
    }

    public AssTrack CreateTrack(AssLibrary library)
    {
        byte[] data;

        if (_data == null)
        {
            data = _encoding.GetBytes(_str);
            _data = data;
        }
        else
        {
            data = _data;
        }

        IntPtr trackPtr;

        unsafe
        {
            fixed (byte* bufferPtr = data)
            {
                var buffer = new IntPtr(bufferPtr);

                trackPtr = NativeMethods.ass_read_memory(library.SafeHandle.DangerousGetHandle(), buffer, data.Length, _codePage);
            }
        }

        if (trackPtr == IntPtr.Zero)
        {
            throw new AssException("Cannot create track from memory");
        }

        var safeHandle = new AssTrackHandle(trackPtr);

        return new AssTrack(safeHandle);
    }

    private readonly string? _codePage;

    private readonly string _str;

    private readonly Encoding _encoding;

    private byte[]? _data;

}
