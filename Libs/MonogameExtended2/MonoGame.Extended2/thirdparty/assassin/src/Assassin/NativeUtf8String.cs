using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Assassin;

internal sealed class NativeUtf8String : IDisposableEx
{

    public NativeUtf8String(string? str)
    {
        _value = str;
        _shouldFreePointer = true;

        if (str == null)
        {
            _ptr = IntPtr.Zero;
        }
        else
        {
            var bytes = Utilities.Utf8.GetBytes(str);
            var ptr = Marshal.AllocHGlobal(bytes.Length + 1);

            if (bytes.Length > 0)
            {
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
            }

            unsafe
            {
                *((byte*)ptr + bytes.Length) = 0;
            }
        }
    }

    public NativeUtf8String(IntPtr ptr)
    {
        _ptr = ptr;
        _shouldFreePointer = false;

        if (ptr == IntPtr.Zero)
        {
            _value = null;
        }
        else
        {
            var byteList = new List<byte>(InitByteListSize);

            unsafe
            {
                var p = (byte*)ptr;

                while (*p != 0)
                {
                    byteList.Add(*p);
                    p += 1;
                }
            }

            var bytes = byteList.ToArray();

            _value = Utilities.Utf8.GetString(bytes);
        }
    }

    ~NativeUtf8String()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
    }

    public bool IsDisposed { get; private set; }

    public IntPtr Pointer
    {
        [DebuggerStepThrough]
        get => _ptr;
    }

    public string? Value
    {
        [DebuggerStepThrough]
        get => _value;
    }

    private void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (_shouldFreePointer && _ptr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_ptr);
            _ptr = IntPtr.Zero;
        }

        IsDisposed = true;

        if (disposing)
        {
            GC.SuppressFinalize(this);
        }
    }

    private const int InitByteListSize = 1024;

    private readonly bool _shouldFreePointer;
    private IntPtr _ptr;

    private readonly string? _value;

}
