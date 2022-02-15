# Lore.QueryCache

[![Build status](https://ci.appveyor.com/api/projects/status/qbpx3i8eq3cl05f1?svg=true)](https://ci.appveyor.com/project/vposd/lore-querycache)
[![Coverage Status](https://coveralls.io/repos/github/vposd/Lore.QueryCache/badge.svg?branch=master)](https://coveralls.io/github/vposd/Lore.QueryCache?branch=master)

A library provides IQueryable results caching using IMemoryCache or IDistributedCache.

## Setup

Setup with DI

```c#
services.AddLoreCache(options =>
    options
        .UseCache<DistributedCache>()
        .UseEntityFramework());

...

app.UseLoreCache();
```

Or Setup with static class

```c#
CacheManager.Cache = new MyCacheImplementation();
CacheManager.CacheKeyFactory = new MyCacheKeyFactory();
```

## Usage

By default, all types from Include and ThenInclude methods are used as cache tags.

Cache keys are generated by CacheKeyFactory class implementation.

### Cache collection

```c#
var results = context.Blogs
    .Include(x => x.Posts)
    .ToCachedListAsync(cancellationToken);
```

### Cache item

```c#
var results = context.Blogs
    .Include(x => x.Posts)
    .Where(x => x.Id == request.Id)
    .CachedFirstOrDefault(cancellationToken);
```

### Invalidate cache

By EF change detector extension. It invalidates all cache linked to default query tags.

```c#
await context.ChangeTracker.ExpireEntitiesCacheAsync();
```

Manual cache invalidation

```c#
CacheManager.ExpireTagsAsync(new List<string> { "tag_1" });
```
