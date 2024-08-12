using System;
using System.Runtime.InteropServices;

namespace Assassin;

internal sealed class NullTerminatedStringList : IDisposableEx
{

    static NullTerminatedStringList()
    {
        PointerSize = Marshal.SizeOf(typeof(IntPtr));
    }

    public NullTerminatedStringList()
    {
        _ptr = Marshal.AllocHGlobal(PointerSize);
        _strings = null;

        unsafe
        {
            *(int*)_ptr = 0;
        }
    }

    public NullTerminatedStringList(params string[] list)
    {
        var listLength = list.Length;
        var strings = new IntPtr[listLength];

        for (var i = 0; i < listLength; i += 1)
        {
            var bytes = Utilities.Utf8.GetBytes(list[i]);
            var bufferSize = bytes.Length + 1;

            var strPtr = Marshal.AllocHGlobal(bufferSize);

            strings[i] = strPtr;

            Marshal.Copy(bytes, 0, strPtr, bytes.Length);

            unsafe
            {
                *((byte*)strPtr + bytes.Length) = 0;
            }
        }

        _strings = strings;

        var pptr = Marshal.AllocHGlobal(PointerSize * (listLength + 1));

        for (var i = 0; i < listLength; i += 1)
        {
            unsafe
            {
                *((IntPtr*)pptr + i) = strings[i];
            }
        }

        unsafe
        {
            *((IntPtr*)pptr + listLength) = IntPtr.Zero;
        }

        _ptr = pptr;
    }

    ~NullTerminatedStringList()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
    }

    public IntPtr GetPointer()
    {
        this.EnsureNotDisposed();

        return _ptr;
    }

    public static implicit operator IntPtr(NullTerminatedStringList? list)
    {
        if (ReferenceEquals(list, null))
        {
            return IntPtr.Zero;
        }
        else
        {
            return list.GetPointer();
        }
    }

    public bool IsDisposed { get; private set; }

    private void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        Marshal.FreeHGlobal(_ptr);

        _ptr = IntPtr.Zero;

        if (_strings != null)
        {
            foreach (var ptr in _strings)
            {
                Marshal.FreeHGlobal(ptr);
            }

            Array.Clear(_strings, 0, _strings.Length);
        }

        if (disposing)
        {
            GC.SuppressFinalize(this);
        }

        IsDisposed = true;
    }

    private static readonly int PointerSize;

    private IntPtr _ptr;

    private readonly IntPtr[]? _strings;

}
