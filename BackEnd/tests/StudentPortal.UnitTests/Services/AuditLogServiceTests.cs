using FluentAssertions;
using Moq;
using StudentPortal.Common.DTOs.AuditLog;
using StudentPortal.Common.Exceptions;
using StudentPortal.Repository.Entities;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Service.Implementations;

namespace StudentPortal.UnitTests.Services;

public class AuditLogServiceTests
{
    private readonly Mock<IAuditLogRepository> _auditLogRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private AuditLogService CreateService()
    {
        return new AuditLogService(
            _auditLogRepository.Object,
            _unitOfWork.Object);
    }

    private static AuditLogListItem CreateListItem(string action = "Update")
    {
        return new AuditLogListItem
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserName = "admin",
            Action = action,
            EntityName = "User",
            EntityId = Guid.NewGuid().ToString(),
            IpAddress = "::1",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static AuditLog CreateAuditLog(User? user = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = user?.Id,
            User = user,
            Action = "Update",
            EntityName = "User",
            EntityId = Guid.NewGuid().ToString(),
            OldValue = "{\"FullName\":\"An\"}",
            NewValue = "{\"FullName\":\"Binh\"}",
            IpAddress = "127.0.0.1",
            CreatedAt = new DateTime(2026, 9, 24, 3, 0, 0, DateTimeKind.Utc)
        };
    }

    private static User CreateUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@staff.avepoint.com",
            UserName = "admin",
            FullName = "System Administrator"
        };
    }

    // =========================================================
    // SEARCH
    // =========================================================

    [Fact]
    public async Task SearchAsync_WithMatchingLogs_ShouldReturnItemsAndTotalFromRepository()
    {
        // Arrange
        var filter = new AuditLogFilter { Page = 2, PageSize = 2 };
        var items = new List<AuditLogListItem> { CreateListItem("Create"), CreateListItem("Delete") };

        _auditLogRepository
            .Setup(r => r.SearchAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 5));

        var service = CreateService();

        // Act
        var result = await service.SearchAsync(filter);

        // Assert
        result.Items.Should().BeEquivalentTo(items, o => o.WithStrictOrdering());
        result.Total.Should().Be(5);
    }

    [Fact]
    public async Task SearchAsync_ShouldTakePageAndPageSizeFromFilterAndComputeTotalPages()
    {
        // Arrange
        var filter = new AuditLogFilter { Page = 3, PageSize = 20 };

        _auditLogRepository
            .Setup(r => r.SearchAsync(It.IsAny<AuditLogFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AuditLogListItem>(), 45));

        var service = CreateService();

        // Act
        var result = await service.SearchAsync(filter);

        // Assert
        result.Page.Should().Be(3);
        result.PageSize.Should().Be(20);
        result.TotalPages.Should().Be(3, "45 logs at 20 per page need 3 pages");
    }

    [Fact]
    public async Task SearchAsync_WhenNoLogsMatch_ShouldReturnEmptyResult()
    {
        // Arrange
        _auditLogRepository
            .Setup(r => r.SearchAsync(It.IsAny<AuditLogFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AuditLogListItem>(), 0));

        var service = CreateService();

        // Act
        var result = await service.SearchAsync(new AuditLogFilter());

        // Assert
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_ShouldPassFilterAndCancellationTokenToRepository()
    {
        // Arrange
        var filter = new AuditLogFilter { Action = "Delete", EntityName = "Announcement" };
        using var cts = new CancellationTokenSource();

        _auditLogRepository
            .Setup(r => r.SearchAsync(It.IsAny<AuditLogFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AuditLogListItem>(), 0));

        var service = CreateService();

        // Act
        await service.SearchAsync(filter, cts.Token);

        // Assert
        _auditLogRepository.Verify(
            r => r.SearchAsync(It.Is<AuditLogFilter>(f => ReferenceEquals(f, filter)), cts.Token),
            Times.Once);
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    [Fact]
    public async Task GetByIdAsync_WithExistingLog_ShouldMapEveryField()
    {
        // Arrange
        var user = CreateUser();
        var log = CreateAuditLog(user);

        _auditLogRepository
            .Setup(r => r.GetByIdAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(log.Id);

        // Assert
        result.Id.Should().Be(log.Id);
        result.UserId.Should().Be(user.Id);
        result.UserName.Should().Be("admin");
        result.Action.Should().Be("Update");
        result.EntityName.Should().Be("User");
        result.EntityId.Should().Be(log.EntityId);
        result.IpAddress.Should().Be("127.0.0.1");
        result.CreatedAt.Should().Be(log.CreatedAt);
        result.OldValue.Should().Be("{\"FullName\":\"An\"}");
        result.NewValue.Should().Be("{\"FullName\":\"Binh\"}");
    }

    [Fact]
    public async Task GetByIdAsync_WhenLogHasNoUser_ShouldReturnNullUserName()
    {
        // Arrange - e.g. a change made during login, before anyone is signed in
        var log = CreateAuditLog(user: null);

        _auditLogRepository
            .Setup(r => r.GetByIdAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(log.Id);

        // Assert
        result.UserId.Should().BeNull();
        result.UserName.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenLogDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _auditLogRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditLog?)null);

        var service = CreateService();

        // Act
        var act = () => service.GetByIdAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Audit log {id} was not found.");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldPassIdAndCancellationTokenToRepository()
    {
        // Arrange
        var log = CreateAuditLog();
        using var cts = new CancellationTokenSource();

        _auditLogRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var service = CreateService();

        // Act
        await service.GetByIdAsync(log.Id, cts.Token);

        // Assert
        _auditLogRepository.Verify(r => r.GetByIdAsync(log.Id, cts.Token), Times.Once);
    }

    // =========================================================
    // LOG (manual audit row)
    // =========================================================

    [Fact]
    public async Task LogAsync_ShouldAddAuditLogWithGivenValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AuditLog? added = null;

        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => added = log)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var before = DateTime.UtcNow;

        // Act
        await service.LogAsync("LoginFailed", "User", "user-123", userId, "10.0.0.1");

        // Assert
        added.Should().NotBeNull();
        added!.Action.Should().Be("LoginFailed");
        added.EntityName.Should().Be("User");
        added.EntityId.Should().Be("user-123");
        added.UserId.Should().Be(userId);
        added.IpAddress.Should().Be("10.0.0.1");
        added.OldValue.Should().BeNull("a manual log records an event, not a data change");
        added.NewValue.Should().BeNull();
        added.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
        added.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task LogAsync_ShouldSaveChangesOnceAfterAdding()
    {
        // Arrange - record the order in which the two calls happen
        var calls = new List<string>();

        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("add"))
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("save"))
            .ReturnsAsync(1);

        var service = CreateService();

        // Act
        await service.LogAsync("Logout", "User", null, null, null);

        // Assert
        calls.Should().Equal("add", "save");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogAsync_ShouldPassCancellationTokenToAddAndSave()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.LogAsync("Logout", "User", null, null, null, cts.Token);

        // Assert
        _auditLogRepository.Verify(r => r.AddAsync(It.IsAny<AuditLog>(), cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task LogAsync_WhenAddFails_ShouldNotSaveChanges()
    {
        // Arrange
        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var service = CreateService();

        // Act
        var act = () => service.LogAsync("Logout", "User", null, null, null);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
