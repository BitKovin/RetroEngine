using System;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace Assassin;

internal static class Utilities
{

    static Utilities()
    {
        Utf8 = new UTF8Encoding(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureNotDisposed<T>(this T obj)
        where T : class, IDisposableEx
    {
        if (obj.IsDisposed)
        {
            throw new ObjectDisposedException($"Instance of '{obj.GetType().Name}'");
        }
    }

    public static readonly Encoding Utf8;

}
