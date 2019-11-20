using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Kitakun.VkModules.Core.Extensions
{
	/// <summary>
	/// Async locker for different types & values
	/// </summary>
	/// <typeparam name="T">lock key type</typeparam>
	public readonly struct KeyLocker<T> : IDisposable
		where T : struct
	{
		private static readonly ConcurrentDictionary<T, SemaphoreSlim> _lockDict = new ConcurrentDictionary<T, SemaphoreSlim>();

		private readonly SemaphoreSlim _locker;

		internal KeyLocker(SemaphoreSlim locker)
		{
			_locker = locker ?? throw new ArgumentNullException(nameof(locker));
		}

		/// <summary>
		/// Get & wait free lock by key
		/// </summary>
		/// <param name="key">lock key</param>
		/// <returns>IDisposable</returns>
		public static async Task<IDisposable> LockAsync(T key)
		{
			var groupKeyLock = _lockDict.GetOrAdd(key, new SemaphoreSlim(1, 1));

			var result = new KeyLocker<T>(groupKeyLock);

			await groupKeyLock.WaitAsync();

			return result;
		}

		public void Dispose() => _locker.Release();
	}
}
