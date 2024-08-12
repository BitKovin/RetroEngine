using System.Runtime.InteropServices;

namespace Assassin;

internal interface INativeResource<out T>
    where T : SafeHandle
{

    T SafeHandle { get; }

}
