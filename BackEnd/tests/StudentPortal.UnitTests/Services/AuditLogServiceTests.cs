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
    private readonly Mock<IRoleRepository> _roleRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public AuditLogServiceTests()
    {
        // Default: empty lookups, so tests that do not care about names still run
        _roleRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role>());

        _userRepository
            .Setup(r => r.GetUserNamesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string>());
    }

    private AuditLogService CreateService()
    {
        return new AuditLogService(
            _auditLogRepository.Object,
            _roleRepository.Object,
            _userRepository.Object,
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
        result.CreatedAt.Kind.Should().Be(DateTimeKind.Utc, "the JSON must end with Z");
        result.Changes.Should().ContainSingle();
        result.Changes[0].Field.Should().Be("FullName");
        result.Changes[0].OldValue.Should().Be("An");
        result.Changes[0].NewValue.Should().Be("Binh");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCreatedAtIsUnspecified_ShouldReturnItAsUtc()
    {
        // Arrange - EF reads datetime2 back as Unspecified
        var log = CreateAuditLog();
        log.CreatedAt = new DateTime(2026, 9, 24, 3, 0, 0, DateTimeKind.Unspecified);

        _auditLogRepository
            .Setup(r => r.GetByIdAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(log.Id);

        // Assert
        result.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        result.CreatedAt.Ticks.Should().Be(log.CreatedAt.Ticks, "only the Kind changes, not the time");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReplaceRoleAndUserIdsWithNames()
    {
        // Arrange
        var studentRole = new Role { Id = Guid.NewGuid(), Name = "Student" };
        var staffRole = new Role { Id = Guid.NewGuid(), Name = "Staff" };
        var editorId = Guid.NewGuid();

        var log = CreateAuditLog();
        log.OldValue = $"{{\"RoleId\":\"{studentRole.Id}\",\"UpdatedBy\":null}}";
        log.NewValue = $"{{\"RoleId\":\"{staffRole.Id}\",\"UpdatedBy\":\"{editorId}\"}}";

        _auditLogRepository
            .Setup(r => r.GetByIdAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        _roleRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { studentRole, staffRole });

        _userRepository
            .Setup(r => r.GetUserNamesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string> { [editorId] = "admin" });

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(log.Id);

        // Assert
        result.Changes.Should().BeEquivalentTo(new[]
        {
            new AuditLogChange { Field = "RoleId", OldValue = "Student", NewValue = "Staff" },
            new AuditLogChange { Field = "UpdatedBy", OldValue = null, NewValue = "admin" }
        }, o => o.WithStrictOrdering());

        _userRepository.Verify(
            r => r.GetUserNamesAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(new[] { editorId })),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
    // GET BY ID - CHANGES
    // =========================================================

    // Runs GetByIdAsync on a log with the given JSON and returns only its changes
    private async Task<IReadOnlyList<AuditLogChange>> GetChangesAsync(
        string? oldJson,
        string? newJson,
        string entityName = "User")
    {
        var log = CreateAuditLog();
        log.EntityName = entityName;
        log.OldValue = oldJson;
        log.NewValue = newJson;

        _auditLogRepository
            .Setup(r => r.GetByIdAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var result = await CreateService().GetByIdAsync(log.Id);

        return result.Changes;
    }

    [Fact]
    public async Task GetByIdAsync_OnUpdate_ShouldReturnOnlyFieldsThatChanged()
    {
        // Arrange
        var oldJson = "{\"FullName\":\"An\",\"Email\":\"a@x.com\",\"AvatarUrl\":null}";
        var newJson = "{\"FullName\":\"Bình\",\"Email\":\"a@x.com\",\"AvatarUrl\":\"a.png\"}";

        // Act
        var changes = await GetChangesAsync(oldJson, newJson);

        // Assert
        changes.Should().BeEquivalentTo(new[]
        {
            new AuditLogChange { Field = "FullName", OldValue = "An", NewValue = "Bình" },
            new AuditLogChange { Field = "AvatarUrl", OldValue = null, NewValue = "a.png" }
        }, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GetByIdAsync_WhenNothingChanged_ShouldReturnNoChanges()
    {
        // Arrange - older "login" logs, written before the mapper skipped unchanged columns
        var json = "{\"FullName\":\"An\",\"Status\":1}";

        // Act
        var changes = await GetChangesAsync(json, json);

        // Assert
        changes.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_OnCreate_ShouldReturnNewValuesWithNullOldValues()
    {
        // Act
        var changes = await GetChangesAsync(null, "{\"FullName\":\"An\",\"Email\":\"a@x.com\"}");

        // Assert
        changes.Select(c => c.Field).Should().Equal("FullName", "Email");
        changes.Should().OnlyContain(c => c.OldValue == null);
    }

    [Fact]
    public async Task GetByIdAsync_OnDelete_ShouldReturnOldValuesWithNullNewValues()
    {
        // Act
        var changes = await GetChangesAsync("{\"FullName\":\"An\",\"Email\":\"a@x.com\"}", null);

        // Assert
        changes.Select(c => c.Field).Should().Equal("FullName", "Email");
        changes.Should().OnlyContain(c => c.NewValue == null);
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownRoleId_ShouldKeepTheGuid()
    {
        // Arrange - no roles set up, so the id cannot be found
        var unknownRoleId = Guid.NewGuid().ToString();

        // Act
        var changes = await GetChangesAsync(null, $"{{\"RoleId\":\"{unknownRoleId}\"}}");

        // Assert
        changes.Single().NewValue.Should().Be(unknownRoleId);
    }

    [Theory]
    [InlineData("User", "Status", 3, "Locked")]
    [InlineData("Announcement", "Status", 2, "Published")]
    [InlineData("Announcement", "RoleReceived", 3, "Staff")]
    [InlineData("User", "Status", 99, "99")]
    [InlineData("Role", "Status", 1, "1")]
    public async Task GetByIdAsync_WithEnumNumber_ShouldReturnEnumName(
        string entityName, string field, int value, string expected)
    {
        // Act
        var changes = await GetChangesAsync(null, $"{{\"{field}\":{value}}}", entityName);

        // Assert
        changes.Single().NewValue.Should().Be(expected);
    }

    [Fact]
    public async Task GetByIdAsync_WithBooleanAndNull_ShouldReturnYesNoAndNull()
    {
        // Act
        var changes = await GetChangesAsync(
            "{\"IsPinned\":false,\"Note\":\"x\"}",
            "{\"IsPinned\":true,\"Note\":null}");

        // Assert
        changes.Should().BeEquivalentTo(new[]
        {
            new AuditLogChange { Field = "IsPinned", OldValue = "No", NewValue = "Yes" },
            new AuditLogChange { Field = "Note", OldValue = "x", NewValue = null }
        }, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GetByIdAsync_WithBrokenJson_ShouldNotThrowAndUseTheOtherSide()
    {
        // Act
        var changes = await GetChangesAsync("{abc", "{\"FullName\":\"An\"}");

        // Assert
        changes.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new AuditLogChange { Field = "FullName", OldValue = null, NewValue = "An" });
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
