using System;

namespace Assassin
{
    public sealed class AssTrack : IDisposableEx, INativeResource<AssTrackHandle>
    {

        internal AssTrack(AssTrackHandle safeHandle)
        {
            SafeHandle = safeHandle;
        }

        ~AssTrack()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool IsDisposed { get; private set; }

        public AssTrackHandle SafeHandle { get; }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            if (disposing)
            {
                SafeHandle.Dispose();
                GC.SuppressFinalize(this);
            }
        }

    }
}
