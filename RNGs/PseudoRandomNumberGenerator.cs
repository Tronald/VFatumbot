using System;

namespace VFatumbot
{
    // Useful for local developing when ANU is being slow and it's not the randomness we're working on...
    public class PseudoRandomNumberGenerator : BaseRandomProvider, IDisposable
    {
        ~PseudoRandomNumberGenerator()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private bool _disposed;
    }
}
