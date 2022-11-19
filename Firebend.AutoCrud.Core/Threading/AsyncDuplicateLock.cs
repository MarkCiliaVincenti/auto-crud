using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;

namespace Firebend.AutoCrud.Core.Threading
{
    public static class AsyncDuplicateLock
    {
        private static readonly AsyncKeyedLocker _asyncKeyedLocker = new();

        public static IDisposable Lock(object key) => _asyncKeyedLocker.Lock(key);

        public static async Task<IDisposable> LockAsync(object key, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            if (timeout.HasValue)
            {
                return await _asyncKeyedLocker.LockAsync(key, timeout.Value, cancellationToken).ConfigureAwait(false);
            }
            return await _asyncKeyedLocker.LockAsync(key, cancellationToken).ConfigureAwait(false);
        }
    }
}
