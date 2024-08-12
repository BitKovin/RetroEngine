using System.Diagnostics.Contracts;

namespace Assassin;

internal unsafe interface ITypedHandle<T>
    where T : unmanaged
{

    [Pure]
    T* GetTypedPointer();

}
