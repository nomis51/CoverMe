using System.Collections.Concurrent;
using CoverMe.Backend.Core.Managers.Abstractions;

namespace CoverMe.Backend.Core.Managers;

public class CacheManager<T> : ICacheManager<T>
{
    #region Members

    private readonly ConcurrentDictionary<string, T?> _values = [];
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = [];

    #endregion

    #region Public methods

    public async Task<T> GetOrSetAsync(string key, Func<Task<T>> setter)
    {
        if (_values.TryGetValue(key, out var value)) return value!;

        var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();

        try
        {
            if (_values.TryGetValue(key, out value)) return value!;

            value = await setter();
            _values[key] = value;
            return value;
        }
        finally
        {
            semaphore.Release();

            if (semaphore.CurrentCount == 1)
            {
                _semaphores.TryRemove(key, out _);
            }
        }
    }

    public bool Remove(string key)
    {
        return _values.TryRemove(key, out _) &&
               _semaphores.TryRemove(key, out _);
    }

    public void RemoveAll()
    {
        _values.Clear();
        _semaphores.Clear();
    }

    #endregion
}