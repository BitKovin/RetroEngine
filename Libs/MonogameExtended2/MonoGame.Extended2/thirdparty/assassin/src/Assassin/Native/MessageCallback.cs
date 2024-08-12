using System;
using System.Runtime.InteropServices;

namespace Assassin.Native;

/// <summary>
/// ASS message callback.
/// </summary>
/// <param name="level">Log level, from 0 (fatal) to 7 (debug).</param>
/// <param name="format">Format string.</param>
/// <param name="args">va_list pointer.</param>
/// <param name="userData">User data provided in <see cref="NativeMethods.ass_set_message_cb(IntPtr, MessageCallback, IntPtr)"/>.</param>
public delegate void MessageCallback(int level, [MarshalAs(UnmanagedType.LPStr)] string format, IntPtr args, IntPtr userData);
