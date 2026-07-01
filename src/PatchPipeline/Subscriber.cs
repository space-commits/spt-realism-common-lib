using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.PatchPipeline
{
    public sealed class Subscription : IDisposable
    {
        private Action _dispose;
        private bool _isDisposed;

        public Subscription(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _dispose?.Invoke();
            _dispose = null;
        }
    }
}
