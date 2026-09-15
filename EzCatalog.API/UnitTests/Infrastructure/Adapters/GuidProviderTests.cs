using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EzCatalog.Infrastructure.Adapters;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Infrastructure.Adapters;

public sealed class GuidProviderTests
{
    private readonly GuidProvider guidProvider = new GuidProvider();

    [Fact]
    public void GetNewGuid_CalledRepeatedly_ReturnsStrictlyIncreasingGuids()
    {
        // Arrange
        const int count = 10_000; // Enough calls for many of them to land within the same millisecond.

        // Act
        var guids = Enumerable.Range(0, count).Select(_ => guidProvider.GetNewGuid()).ToList();

        // Assert
        guids.Should().OnlyHaveUniqueItems()
            .And.BeInAscendingOrder();
    }

    [Fact]
    public async Task GetNewGuid_CalledOverTime_ReturnsStrictlyIncreasingGuids()
    {
        // Arrange
        const int count = 10;
        var guids = new List<Guid>(count);

        // Act
        for (var i = 0; i < count; i++)
        {
            guids.Add(guidProvider.GetNewGuid());
            await Task.Delay(TimeSpan.FromMilliseconds(2), TestContext.Current.CancellationToken);
        }

        // Assert
        guids.Should().OnlyHaveUniqueItems()
            .And.BeInAscendingOrder();
    }
}
