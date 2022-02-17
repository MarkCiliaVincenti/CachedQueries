using CachedQueries.Core;
using CachedQueries.Core.Interfaces;
using CachedQueries.EntityFramework;
using FluentAssertions;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CachedQueries.DependencyInjection.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddLoreCache_Should_Configure_Cache()
    {
        // Given
        var services = new ServiceCollection();

        // When
        services.AddDistributedMemoryCache();
        services.AddLoreCache(options => options.UseCache<DistributedCache>());

        // Then
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICache>();

        cache.Should().BeOfType<DistributedCache>();
    }

    [Fact]
    public void AddLoreCache_Should_Configure_EntityFramework()
    {
        // Given
        var services = new ServiceCollection();

        // When
        services.AddDistributedMemoryCache();
        services.AddLoreCache(options => options.UseEntityFramework());

        // Then
        CacheManager.CacheKeyFactory.Should().BeOfType<QueryCacheKeyFactory>();
    }

    [Fact]
    public void UseLoreCache_Should_Set_Cache_To_Cache_Manager()
    {
        // Given
        var services = new ServiceCollection();

        // When
        services.AddMemoryCache();
        services.AddLoreCache(options => options.UseCache<MemoryCache>().UseEntityFramework());

        var builder = new ApplicationBuilder(services.BuildServiceProvider());
        builder.UseLoreCache();

        // Then
        CacheManager.Cache.Should().BeOfType<MemoryCache>();
    }
}