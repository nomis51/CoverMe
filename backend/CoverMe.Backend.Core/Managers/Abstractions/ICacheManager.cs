namespace CoverMe.Backend.Core.Managers.Abstractions;

public interface ICacheManager<T>
{
    Task<T> GetOrSetAsync(string key, Func<Task<T>> setter);
    bool Remove(string key);
    void RemoveAll();
}