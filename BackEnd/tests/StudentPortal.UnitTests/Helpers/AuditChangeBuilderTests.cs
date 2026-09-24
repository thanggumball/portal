using FluentAssertions;
using StudentPortal.Common.DTOs.AuditLog;
using StudentPortal.Service.Helpers;

namespace StudentPortal.UnitTests.Helpers;

public class AuditChangeBuilderTests
{
    private static readonly IReadOnlyDictionary<Guid, string> NoNames = new Dictionary<Guid, string>();

    private static IReadOnlyList<AuditLogChange> Build(
        string? oldJson,
        string? newJson,
        string entityName = "User",
        IReadOnlyDictionary<Guid, string>? roleNames = null,
        IReadOnlyDictionary<Guid, string>? userNames = null)
        => AuditChangeBuilder.Build(entityName, oldJson, newJson, roleNames ?? NoNames, userNames ?? NoNames);

    // =========================================================
    // WHICH FIELDS ARE RETURNED
    // =========================================================

    [Fact]
    public void Build_OnUpdate_ShouldReturnOnlyFieldsThatChanged()
    {
        // Arrange
        var oldJson = "{\"FullName\":\"An\",\"Email\":\"a@x.com\",\"AvatarUrl\":null,\"UserCode\":\"S1\",\"IsDeleted\":false}";
        var newJson = "{\"FullName\":\"Bình\",\"Email\":\"a@x.com\",\"AvatarUrl\":\"a.png\",\"UserCode\":\"S1\",\"IsDeleted\":false}";

        // Act
        var result = Build(oldJson, newJson);

        // Assert
        result.Should().BeEquivalentTo(new[]
        {
            new AuditLogChange { Field = "FullName", OldValue = "An", NewValue = "Bình" },
            new AuditLogChange { Field = "AvatarUrl", OldValue = null, NewValue = "a.png" }
        }, o => o.WithStrictOrdering());
    }

    [Fact]
    public void Build_WhenOldEqualsNewForEveryField_ShouldReturnEmpty()
    {
        // Arrange - older "login" logs, written before the mapper skipped unchanged columns
        var json = "{\"FullName\":\"An\",\"Status\":1}";

        // Act
        var result = Build(json, json);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Build_OnCreate_ShouldReturnEveryNewFieldWithNullOldValue()
    {
        // Act
        var result = Build(null, "{\"FullName\":\"An\",\"Email\":\"a@x.com\"}");

        // Assert
        result.Select(c => c.Field).Should().Equal("FullName", "Email");
        result.Should().OnlyContain(c => c.OldValue == null);
    }

    [Fact]
    public void Build_OnDelete_ShouldReturnEveryOldFieldWithNullNewValue()
    {
        // Act
        var result = Build("{\"FullName\":\"An\",\"Email\":\"a@x.com\"}", null);

        // Assert
        result.Select(c => c.Field).Should().Equal("FullName", "Email");
        result.Should().OnlyContain(c => c.NewValue == null);
    }

    [Fact]
    public void Build_ShouldListNewFieldsFirstThenFieldsOnlyInOld()
    {
        // Act
        var result = Build("{\"A\":1,\"B\":1}", "{\"C\":2,\"A\":2}");

        // Assert
        result.Select(c => c.Field).Should().Equal("C", "A", "B");
    }

    // =========================================================
    // NAME LOOKUPS
    // =========================================================

    [Fact]
    public void Build_WithKnownRoleId_ShouldReturnRoleName()
    {
        // Arrange
        var student = Guid.NewGuid();
        var staff = Guid.NewGuid();
        var roles = new Dictionary<Guid, string> { [student] = "Student", [staff] = "Staff" };

        // Act
        var result = Build($"{{\"RoleId\":\"{student}\"}}", $"{{\"RoleId\":\"{staff}\"}}", roleNames: roles);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new AuditLogChange { Field = "RoleId", OldValue = "Student", NewValue = "Staff" });
    }

    [Fact]
    public void Build_WithUnknownRoleId_ShouldKeepTheGuid()
    {
        // Arrange
        var unknown = Guid.NewGuid();

        // Act
        var result = Build(null, $"{{\"RoleId\":\"{unknown}\"}}");

        // Assert
        result.Single().NewValue.Should().Be(unknown.ToString());
    }

    [Fact]
    public void Build_WithCreatedBy_ShouldReturnUserName()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var users = new Dictionary<Guid, string> { [adminId] = "admin" };

        // Act
        var result = Build(null, $"{{\"CreatedBy\":\"{adminId}\"}}", userNames: users);

        // Assert
        result.Single().NewValue.Should().Be("admin");
    }

    [Fact]
    public void CollectGuids_ShouldReturnGuidsOfTheGivenFieldsOnly()
    {
        // Arrange
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var oldJson = $"{{\"CreatedBy\":\"{a}\",\"RoleId\":\"{Guid.NewGuid()}\"}}";
        var newJson = $"{{\"CreatedBy\":\"{a}\",\"UpdatedBy\":\"{b}\",\"FullName\":\"not a guid\"}}";

        // Act
        var result = AuditChangeBuilder.CollectGuids(oldJson, newJson, AuditChangeBuilder.UserReferenceFields);

        // Assert
        result.Should().BeEquivalentTo(new[] { a, b });
    }

    // =========================================================
    // VALUE FORMATTING
    // =========================================================

    [Theory]
    [InlineData("User", "Status", 3, "Locked")]
    [InlineData("Announcement", "Status", 2, "Published")]
    [InlineData("Announcement", "RoleReceived", 3, "Staff")]
    [InlineData("User", "Status", 99, "99")]
    public void Build_WithEnumNumber_ShouldReturnEnumName(string entity, string field, int value, string expected)
    {
        // Act
        var result = Build(null, $"{{\"{field}\":{value}}}", entityName: entity);

        // Assert
        result.Single().NewValue.Should().Be(expected);
    }

    [Fact]
    public void Build_WithStatusOfOtherEntity_ShouldKeepTheNumber()
    {
        // Act
        var result = Build(null, "{\"Status\":1}", entityName: "Role");

        // Assert
        result.Single().NewValue.Should().Be("1");
    }

    [Fact]
    public void Build_WithBooleanAndNull_ShouldReturnYesNoAndNull()
    {
        // Act
        var result = Build("{\"IsPinned\":false,\"Note\":\"x\"}", "{\"IsPinned\":true,\"Note\":null}");

        // Assert
        result.Should().BeEquivalentTo(new[]
        {
            new AuditLogChange { Field = "IsPinned", OldValue = "No", NewValue = "Yes" },
            new AuditLogChange { Field = "Note", OldValue = "x", NewValue = null }
        }, o => o.WithStrictOrdering());
    }

    // =========================================================
    // BROKEN INPUT
    // =========================================================

    [Fact]
    public void Build_WithInvalidJson_ShouldNotThrowAndUseTheOtherSide()
    {
        // Act
        var act = () => Build("{abc", "{\"FullName\":\"An\"}");

        // Assert
        act.Should().NotThrow();
        act().Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new AuditLogChange { Field = "FullName", OldValue = null, NewValue = "An" });
    }

    [Fact]
    public void Build_WhenBothSidesAreBroken_ShouldReturnEmpty()
    {
        // Act
        var result = Build("{abc", "[1,2]");

        // Assert
        result.Should().BeEmpty();
    }
}
