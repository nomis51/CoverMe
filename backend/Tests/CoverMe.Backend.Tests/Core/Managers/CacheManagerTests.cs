using System.Collections.Concurrent;
using System.Reflection;
using CoverMe.Backend.Core.Managers;
using Shouldly;

namespace CoverMe.Backend.Tests.Core.Managers;

public class CacheManagerTests
{
    #region Members

    private readonly CacheManager<int> _sut = new();

    #endregion

    #region Tests

    [Fact]
    public async Task GetOrSetAsync_ShouldReturnCachedValue_WhenValueExistsInCache()
    {
        // Arrange
        const string key = "key1";
        const int value = 42;
        ExistsInCache(key, value).ShouldBeFalse();
        AddToCache(key, value);

        // Act
        var result = await _sut.GetOrSetAsync(key, () =>
        {
            Assert.Fail("Should not have been called");
            return Task.FromResult(value);
        });

        // Assert
        result.ShouldBe(value);
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldReturnNewValue_WhenValueDoesNotExistInCache()
    {
        // Arrange
        const string key = "key1";
        const int value = 42;
        ExistsInCache(key, value).ShouldBeFalse();
        var flag = false;

        // Act
        var result = await _sut.GetOrSetAsync(key, () =>
        {
            flag = true;
            return Task.FromResult(value);
        });

        // Assert
        result.ShouldBe(value);
        ExistsInCache(key, value).ShouldBeTrue();
        flag.ShouldBeTrue();
    }

    [Fact]
    public async Task GetOrSetAsync_ShouldBeThreadSafe_WithFifoBehavior()
    {
        // Arrange
        const string key = "key1";
        var flag = 0;

        // Act
        var task1 = Task.Run(async () =>
        {
            await _sut.GetOrSetAsync(key, async () =>
            {
                await Task.Delay(1000);
                Interlocked.Increment(ref flag);
                return 1;
            });
        });
        var task2 = Task.Run(async () =>
        {
            await Task.Delay(500);
            await _sut.GetOrSetAsync(key, () =>
            {
                Interlocked.Increment(ref flag);
                return Task.FromResult(2);
            });
        });

        // Assert
        await Task.WhenAll(task1, task2);
        ExistsInCache(key, 1).ShouldBeTrue();
        flag.ShouldBe(1);
    }

    [Fact]
    public void Remove_ShouldRemoveValueFromCache()
    {
        // Arrange
        const string key = "key1";
        const int value = 42;
        AddToCache(key, value);

        // Act
        _sut.Remove(key);

        // Assert
        ExistsInCache(key, value).ShouldBeFalse();
    }

    [Fact]
    public void RemoveAll_ShouldRemoveAllValuesFromCache()
    {
        // Arrange
        const string key1 = "key1";
        const string key2 = "key2";
        const int value = 42;
        AddToCache(key1, value);
        AddToCache(key2, value);

        // Act
        _sut.RemoveAll();

        // Assert
        ExistsInCache(key1, value).ShouldBeFalse();
        ExistsInCache(key2, value).ShouldBeFalse();
    }

    #endregion

    #region Private methods

    private bool ExistsInCache(string key, int value)
    {
        var values = (ConcurrentDictionary<string, int>)_sut.GetType()
            .GetField("_values", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(_sut)!;

        return values.ContainsKey(key) && values[key] == value;
    }

    private void AddToCache(string key, int value)
    {
        var values = (ConcurrentDictionary<string, int>)_sut.GetType()
            .GetField("_values", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(_sut)!;

        values.TryAdd(key, value).ShouldBeTrue();
    }

    #endregion
}