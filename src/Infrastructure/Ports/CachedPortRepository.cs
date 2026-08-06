using Microsoft.Extensions.Caching.Memory;
using ShipSharp.Domain.Ports;

namespace ShipSharp.Infrastructure.Ports;

public class CachedPortRepository : IPortRepository
{
    private readonly IPortRepository _inner;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(30);
    private const string AllPortsCacheKey = "ports:all";

    public CachedPortRepository(IPortRepository inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Port?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"ports:{id}";
        return await _cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DefaultCacheDuration;
            return _inner.GetByIdAsync(id, cancellationToken);
        });
    }

    public async Task<Port?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"ports:code:{code.ToLower()}";
        return await _cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DefaultCacheDuration;
            return _inner.GetByCodeAsync(code, cancellationToken);
        });
    }

    public async Task<IReadOnlyList<Port>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(AllPortsCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DefaultCacheDuration;
            return _inner.GetAllAsync(cancellationToken);
        }) ?? Array.Empty<Port>();
    }

    public async Task AddAsync(Port port, CancellationToken cancellationToken = default)
    {
        await _inner.AddAsync(port, cancellationToken);
        _cache.Remove(AllPortsCacheKey);
    }
}
