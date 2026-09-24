using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentPortal.API.Controllers;
using StudentPortal.Common.Constants;
using StudentPortal.Common.DTOs.AuditLog;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Common.Exceptions;
using StudentPortal.Service.Interfaces;

namespace StudentPortal.UnitTests.Controllers;

public class AuditLogsControllerTests
{
    private readonly Mock<IAuditLogService> _auditLogService = new();

    private AuditLogsController CreateController()
    {
        return new AuditLogsController(_auditLogService.Object);
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

    private static AuditLogDetail CreateDetail(Guid id)
    {
        return new AuditLogDetail
        {
            Id = id,
            UserId = Guid.NewGuid(),
            UserName = "admin",
            Action = "Update",
            EntityName = "User",
            EntityId = Guid.NewGuid().ToString(),
            IpAddress = "::1",
            CreatedAt = DateTime.UtcNow,
            Changes = new[]
            {
                new AuditLogChange { Field = "FullName", OldValue = "An", NewValue = "Binh" }
            }
        };
    }

    // =========================================================
    // SEARCH
    // =========================================================

    [Fact]
    public async Task Search_WithFilter_ShouldReturnOkWithPagedResultFromService()
    {
        // Arrange
        var filter = new AuditLogFilter { Page = 1, PageSize = 20 };
        var paged = new PagedResult<AuditLogListItem>
        {
            Items = new[] { CreateListItem("Create"), CreateListItem("Update") },
            Total = 2,
            Page = 1,
            PageSize = 20
        };

        _auditLogService
            .Setup(s => s.SearchAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var controller = CreateController();

        // Act
        var result = await controller.Search(filter, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ApiResponse<PagedResult<AuditLogListItem>>>().Subject;

        body.Success.Should().BeTrue();
        body.Message.Should().BeNull();
        body.Data.Should().BeSameAs(paged);
        body.Data!.Items.Should().HaveCount(2);
        body.Data.Total.Should().Be(2);
    }

    [Fact]
    public async Task Search_ShouldPassSameFilterAndCancellationTokenToService()
    {
        // Arrange
        var filter = new AuditLogFilter
        {
            Page = 3,
            PageSize = 50,
            UserId = Guid.NewGuid(),
            EntityName = "Announcement",
            Action = "Delete",
            From = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc)
        };

        using var cts = new CancellationTokenSource();

        _auditLogService
            .Setup(s => s.SearchAsync(It.IsAny<AuditLogFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<AuditLogListItem>());

        var controller = CreateController();

        // Act
        await controller.Search(filter, cts.Token);

        // Assert - the controller must not rebuild or alter the filter
        _auditLogService.Verify(
            s => s.SearchAsync(
                It.Is<AuditLogFilter>(f => ReferenceEquals(f, filter)),
                cts.Token),
            Times.Once);
        _auditLogService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Search_WhenNoLogsMatch_ShouldReturnOkWithEmptyItems()
    {
        // Arrange
        _auditLogService
            .Setup(s => s.SearchAsync(It.IsAny<AuditLogFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<AuditLogListItem>
            {
                Items = Array.Empty<AuditLogListItem>(),
                Total = 0,
                Page = 1,
                PageSize = 20
            });

        var controller = CreateController();

        // Act
        var result = await controller.Search(new AuditLogFilter(), CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ApiResponse<PagedResult<AuditLogListItem>>>().Subject;

        body.Success.Should().BeTrue();
        body.Data!.Items.Should().BeEmpty();
        body.Data.Total.Should().Be(0);
    }

    [Fact]
    public async Task Search_WhenServiceThrows_ShouldPropagateException()
    {
        // Arrange
        _auditLogService
            .Setup(s => s.SearchAsync(It.IsAny<AuditLogFilter>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid filter."));

        var controller = CreateController();

        // Act
        var act = () => controller.Search(new AuditLogFilter(), CancellationToken.None);

        // Assert - ExceptionHandlingMiddleware turns it into the HTTP error, not the controller
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid filter.");
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturnOkWithDetailFromService()
    {
        // Arrange
        var id = Guid.NewGuid();
        var detail = CreateDetail(id);

        _auditLogService
            .Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        var controller = CreateController();

        // Act
        var result = await controller.GetById(id, CancellationToken.None);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ApiResponse<AuditLogDetail>>().Subject;

        body.Success.Should().BeTrue();
        body.Data.Should().BeSameAs(detail);
        body.Data!.Id.Should().Be(id);
        body.Data.Changes.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new AuditLogChange { Field = "FullName", OldValue = "An", NewValue = "Binh" });
    }

    [Fact]
    public async Task GetById_ShouldPassIdAndCancellationTokenToService()
    {
        // Arrange
        var id = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        _auditLogService
            .Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDetail(id));

        var controller = CreateController();

        // Act
        await controller.GetById(id, cts.Token);

        // Assert
        _auditLogService.Verify(s => s.GetByIdAsync(id, cts.Token), Times.Once);
        _auditLogService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetById_WhenLogDoesNotExist_ShouldPropagateNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _auditLogService
            .Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Audit log {id} was not found."));

        var controller = CreateController();

        // Act
        var act = () => controller.GetById(id, CancellationToken.None);

        // Assert - ExceptionHandlingMiddleware maps NotFoundException to 404
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Audit log {id} was not found.");
    }

    // =========================================================
    // ROUTING & SECURITY ATTRIBUTES
    // =========================================================

    [Fact]
    public void Controller_ShouldOnlyAllowStaffRole()
    {
        // Act
        var authorize = typeof(AuditLogsController).GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        authorize.Should().NotBeNull("audit logs must never be readable anonymously");
        authorize!.Roles.Should().Be(RoleConstants.Admin);
        RoleConstants.Admin.Should().Be("Staff", "the admin account in the seed data has the Staff role");
    }

    [Fact]
    public void Controller_ShouldBeRoutedUnderApiAuditLogs()
    {
        // Act
        var route = typeof(AuditLogsController).GetCustomAttribute<RouteAttribute>();

        // Assert
        route.Should().NotBeNull();
        route!.Template.Should().Be("api/audit-logs");
        typeof(AuditLogsController).GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();
    }

    [Fact]
    public void Search_ShouldBeHttpGetWithFilterBoundFromQuery()
    {
        // Arrange
        var method = typeof(AuditLogsController).GetMethod(nameof(AuditLogsController.Search))!;

        // Act
        var httpGet = method.GetCustomAttribute<HttpGetAttribute>();
        var filterParam = method.GetParameters().Single(p => p.ParameterType == typeof(AuditLogFilter));

        // Assert
        httpGet.Should().NotBeNull();
        httpGet!.Template.Should().BeNull("Search is served at the controller route itself");
        filterParam.GetCustomAttribute<FromQueryAttribute>().Should().NotBeNull();
    }

    [Fact]
    public void GetById_ShouldBeHttpGetWithGuidRouteConstraint()
    {
        // Arrange
        var method = typeof(AuditLogsController).GetMethod(nameof(AuditLogsController.GetById))!;

        // Act
        var httpGet = method.GetCustomAttribute<HttpGetAttribute>();

        // Assert
        httpGet.Should().NotBeNull();
        httpGet!.Template.Should().Be("{id:guid}");
    }

    [Fact]
    public void Controller_ShouldExposeNoWriteEndpoints()
    {
        // Arrange - audit logs are read-only by design
        var writeVerbs = new[] { "POST", "PUT", "PATCH", "DELETE" };

        var actions = typeof(AuditLogsController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        // Act
        var writeActions = actions
            .Where(m => m.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>()
                .Any(a => a.HttpMethods.Any(v => writeVerbs.Contains(v))))
            .Select(m => m.Name)
            .ToList();

        // Assert
        writeActions.Should().BeEmpty();
        actions.Select(m => m.Name).Should().BeEquivalentTo(
            nameof(AuditLogsController.Search),
            nameof(AuditLogsController.GetById));
    }
}
