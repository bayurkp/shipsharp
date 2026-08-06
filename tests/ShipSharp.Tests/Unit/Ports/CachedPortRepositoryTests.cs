using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using ShipSharp.Domain.Ports;
using ShipSharp.Infrastructure.Ports;
using Xunit;

namespace ShipSharp.Tests.Unit.Ports;

public class CachedPortRepositoryTests
{
    private readonly Mock<IPortRepository> _innerRepoMock = new();
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    private readonly CachedPortRepository _sut;

    public CachedPortRepositoryTests()
    {
        _sut = new CachedPortRepository(_innerRepoMock.Object, _memoryCache);
    }

    [Fact]
    public async Task GetAllAsync_OnFirstCall_ShouldFetchFromInnerRepoAndCacheResult()
    {
        // Arrange
        var ports = new List<Port>
        {
            new() { Id = Guid.NewGuid(), Name = "Surabaya", Code = "SUB", Country = "ID" },
            new() { Id = Guid.NewGuid(), Name = "Singapore", Code = "SIN", Country = "SG" }
        };

        _innerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ports);

        // Act 1: First call (Cache Miss)
        var result1 = await _sut.GetAllAsync();

        // Act 2: Second call (Cache Hit)
        var result2 = await _sut.GetAllAsync();

        // Assert
        result1.Should().HaveCount(2);
        result2.Should().HaveCount(2);

        // Inner repo must only be called ONCE due to caching
        _innerRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldEvictAllPortsCache()
    {
        // Arrange
        var ports = new List<Port>
        {
            new() { Id = Guid.NewGuid(), Name = "Surabaya", Code = "SUB", Country = "ID" }
        };

        _innerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ports);

        // Populate Cache
        await _sut.GetAllAsync();

        // Act: Add new port (should evict cache)
        var newPort = new Port { Id = Guid.NewGuid(), Name = "Jakarta", Code = "JKT", Country = "ID" };
        await _sut.AddAsync(newPort);

        // Act: Call GetAllAsync again (Cache Miss after eviction)
        await _sut.GetAllAsync();

        // Assert: Inner repo should be called TWICE now
        _innerRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
