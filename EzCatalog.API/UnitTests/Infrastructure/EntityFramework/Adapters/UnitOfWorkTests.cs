using System;
using System.Threading.Tasks;
using EzCatalog.Infrastructure.EntityFramework;
using EzCatalog.Infrastructure.EntityFramework.Adapters;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Testing;
using Xunit;

namespace EzCatalog.UnitTests.Infrastructure.EntityFramework.Adapters;

public sealed class UnitOfWorkTests : IAsyncDisposable
{
    private readonly InMemoryCatalogDatabase database = new InMemoryCatalogDatabase();
    private readonly CatalogDbContext dbContext;
    private readonly FakeLogger<UnitOfWork> logger = new FakeLogger<UnitOfWork>();
    private readonly UnitOfWork unitOfWork;

    public UnitOfWorkTests()
    {
        dbContext = database.CreateContext();
        unitOfWork = new UnitOfWork(logger, dbContext);
    }

    [Fact]
    public void HasOngoingTransaction_WhenCreated_IsFalse()
    {
        // Act & Assert
        unitOfWork.HasOngoingTransaction.Should().BeFalse();
    }

    [Fact]
    public async Task BeginTransactionAsync_WithoutOngoingTransaction_StartsTransaction()
    {
        // Act
        await unitOfWork.BeginTransactionAsync(TestContext.Current.CancellationToken);

        // Assert
        unitOfWork.HasOngoingTransaction.Should().BeTrue();
    }

    [Fact]
    public async Task BeginTransactionAsync_WithoutOngoingTransaction_LogsStepWithTransactionId()
    {
        // Act
        await unitOfWork.BeginTransactionAsync(TestContext.Current.CancellationToken);

        // Assert
        var record = logger.LatestRecord;
        record.GetStructuredStateValue("step").Should().Be(nameof(UnitOfWork.BeginTransactionAsync));
        Guid.TryParse(record.GetStructuredStateValue("transactionId"), out _).Should().BeTrue();
    }

    [Fact]
    public async Task BeginTransactionAsync_WithOngoingTransaction_ThrowsInvalidOperationException()
    {
        // Arrange
        await unitOfWork.BeginTransactionAsync(TestContext.Current.CancellationToken);

        // Act
        var act = () => unitOfWork.BeginTransactionAsync(TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This unit of work has already initiated a transaction.");
        unitOfWork.HasOngoingTransaction.Should().BeTrue();
    }

    [Fact]
    public async Task CommitTransactionAsync_WithoutOngoingTransaction_ThrowsInvalidOperationException()
    {
        // Act
        var act = () => unitOfWork.CommitTransactionAsync(TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot commit; a transaction has not been initiated.");
    }

    [Fact]
    public async Task CommitTransactionAsync_WithOngoingTransaction_SavesPendingChanges()
    {
        // Arrange
        await unitOfWork.BeginTransactionAsync(TestContext.Current.CancellationToken);
        dbContext.Products.Add(ProductTestData.CreateDbModel(id: ProductTestData.DefaultId));

        // Act
        await unitOfWork.CommitTransactionAsync(TestContext.Current.CancellationToken);

        // Assert
        var stored = await database.FindProductAsync(ProductTestData.DefaultId);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitTransactionAsync_WithOngoingTransaction_EndsTransaction()
    {
        // Arrange
        await unitOfWork.BeginTransactionAsync(TestContext.Current.CancellationToken);

        // Act
        await unitOfWork.CommitTransactionAsync(TestContext.Current.CancellationToken);

        // Assert
        unitOfWork.HasOngoingTransaction.Should().BeFalse();
    }

    [Fact]
    public async Task CommitTransactionAsync_WithOngoingTransaction_AllowsBeginningNextTransaction()
    {
        // Arrange
        await unitOfWork.BeginTransactionAsync(TestContext.Current.CancellationToken);
        await unitOfWork.CommitTransactionAsync(TestContext.Current.CancellationToken);

        // Act
        await unitOfWork.BeginTransactionAsync(TestContext.Current.CancellationToken);

        // Assert
        unitOfWork.HasOngoingTransaction.Should().BeTrue();
    }

    [Fact]
    public async Task CommitTransactionAsync_WithOngoingTransaction_LogsStepWithSameTransactionIdAsBegin()
    {
        // Arrange
        await unitOfWork.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var beginTransactionId = logger.LatestRecord.GetStructuredStateValue("transactionId");

        // Act
        await unitOfWork.CommitTransactionAsync(TestContext.Current.CancellationToken);

        // Assert
        var record = logger.LatestRecord;
        record.GetStructuredStateValue("step").Should().Be(nameof(UnitOfWork.CommitTransactionAsync));
        record.GetStructuredStateValue("transactionId").Should().Be(beginTransactionId);
    }

    [Fact]
    public async Task CommitTransactionAsync_WhenSavingChangesFails_PropagatesExceptionAndKeepsTransactionOngoing()
    {
        // Arrange
        await unitOfWork.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var invalidProduct = ProductTestData.CreateDbModel();
        invalidProduct.Name = null!;
        dbContext.Products.Add(invalidProduct);

        // Act
        var act = () => unitOfWork.CommitTransactionAsync(TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
        unitOfWork.HasOngoingTransaction.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_WithOngoingTransaction_DisposesTransactionAndLogsStepWithTransactionId()
    {
        // Arrange
        await unitOfWork.BeginTransactionAsync(TestContext.Current.CancellationToken);
        var beginTransactionId = logger.LatestRecord.GetStructuredStateValue("transactionId");

        // Act
        var act = async () => await unitOfWork.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
        var record = logger.LatestRecord;
        record.GetStructuredStateValue("step").Should().Be(nameof(UnitOfWork.DisposeAsync));
        record.GetStructuredStateValue("transactionId").Should().Be(beginTransactionId);
    }

    [Fact]
    public async Task DisposeAsync_WithoutOngoingTransaction_LogsStepWithoutTransactionId()
    {
        // Act
        await unitOfWork.DisposeAsync();

        // Assert
        var record = logger.LatestRecord;
        record.GetStructuredStateValue("step").Should().Be(nameof(UnitOfWork.DisposeAsync));
        record.GetStructuredStateValue("transactionId").Should().BeNull();
    }

    public async ValueTask DisposeAsync()
    {
        await unitOfWork.DisposeAsync();
        await dbContext.DisposeAsync();
    }
}
