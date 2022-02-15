﻿using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Lore.QueryCache.EntityFramework.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Cache and return query results with write-through strategy.
    /// </summary>
    /// <param name="query">Query to cache</param>
    /// <param name="tags">Invalidation tags</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>List query results</returns>
    public static async Task<IEnumerable<T>> ToCachedListAsync<T>(this IQueryable<T> query,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken = default) where T : class
    {
        var key = CacheManager.CacheKeyFactory.GetCacheKey(query, tags);
        if (string.IsNullOrEmpty(key))
            return query;

        var cached = await CacheManager.Cache.GetAsync<IEnumerable<T>>(key);
        if (cached is not null)
            return cached;

        var value = await query.ToListAsync(cancellationToken);
        
        await CacheManager.Cache.SetAsync(key, value);
        await CacheManager.LinkTagsAsync(key, tags);
        
        return value;
    }

    /// <summary>
    /// Cache query results with write-through strategy.
    /// Using tags for invalidation as type names from Include and ThenInclude methods.
    /// </summary>
    /// <param name="query">Query to cache</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>List query results</returns>
    public static Task<IEnumerable<T>> ToCachedListAsync<T>(this IQueryable<T> query,
        CancellationToken cancellationToken = default) where T : class
    {
        var tags = RetrieveInvalidationTagsFromQuery(query);
        return query.ToCachedListAsync(tags, cancellationToken);
    }

    /// <summary>
    /// Cache and return query first result with write-through strategy
    /// </summary>
    /// <param name="query">Query to cache</param>
    /// <param name="tags">Invalidation tags</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>FirstOrDefault query result</returns>
    public static async Task<T?> CachedFirstOrDefaultAsync<T>(this IQueryable<T> query,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken = default) where T : class
    {
        var key = CacheManager.CacheKeyFactory.GetCacheKey(query, tags);
        if (string.IsNullOrEmpty(key))
            return await query.FirstOrDefaultAsync(cancellationToken);

        var cached = await CacheManager.Cache.GetAsync<T>(key);
        if (cached is not null)
            return cached;

        var value = await query.FirstOrDefaultAsync(cancellationToken);
        
        await CacheManager.Cache.SetAsync(key, value);
        await CacheManager.LinkTagsAsync(key, tags);
        
        return value;
    }
    
    /// <summary>
    /// Cache and return query first result with write-through strategy
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="query">Query to cache</param>
    /// <param name="tags">Invalidation tags</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>FirstOrDefault query result</returns>
    public static async Task<T?> CachedFirstOrDefaultAsync<T>(this IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken = default) where T : class
    {
        var key = CacheManager.CacheKeyFactory.GetCacheKey(query, tags);
        if (string.IsNullOrEmpty(key))
            return await query.FirstOrDefaultAsync(predicate, cancellationToken);

        var cached = await CacheManager.Cache.GetAsync<T>(key);
        if (cached is not null)
            return cached;

        var value = await query.FirstOrDefaultAsync(predicate, cancellationToken);
        
        await CacheManager.Cache.SetAsync(key, value);
        await CacheManager.LinkTagsAsync(key, tags);
        
        return value;
    }

    /// <summary>
    /// Cache and return query first result with write-through strategy.
    /// Using tags for invalidation as type names from Include and ThenInclude methods.
    /// </summary>
    /// <param name="query">Query to cache</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>FirstOrDefault query result</returns>
    public static Task<T?> CachedFirstOrDefaultAsync<T>(this IQueryable<T> query,
        CancellationToken cancellationToken = default) where T : class
    {
        var tags = RetrieveInvalidationTagsFromQuery(query);
        return query.CachedFirstOrDefaultAsync(tags, cancellationToken);
    }

    /// <summary>
    /// Cache and return query first result with write-through strategy.
    /// Using tags for invalidation as type names from Include and ThenInclude methods.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="query">Query to cache</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>FirstOrDefault query result</returns>
    public static Task<T?> CachedFirstOrDefaultAsync<T>(this IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) where T : class
    {
        var tags = RetrieveInvalidationTagsFromQuery(query);
        return query.CachedFirstOrDefaultAsync(predicate, tags, cancellationToken);
    }
    
    private static List<string> RetrieveInvalidationTagsFromQuery<T>(IQueryable<T> query) where T : class
    {
        var includedTypes = query.GetIncludeTypes();
        var tags = includedTypes
            .Select(x => x.FullName)
            .Where(x => !string.IsNullOrEmpty(x))
            .Cast<string>()
            .ToList();
        return tags;
    }
}