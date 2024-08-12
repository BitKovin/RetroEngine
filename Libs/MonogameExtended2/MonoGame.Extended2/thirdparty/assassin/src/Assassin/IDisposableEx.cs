using System;

namespace Assassin;

internal interface IDisposableEx : IDisposable
{

    bool IsDisposed { get; }

}
