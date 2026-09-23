using FluentValidation;
using StudentPortal.Common.DTOs.Announcement;

namespace StudentPortal.Service.Validations.Announcement;

public class AnnouncementFilterValidator : AbstractValidator<AnnouncementFilter>
{
    public AnnouncementFilterValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(255);

        RuleFor(x => x.Page)
            .InclusiveBetween(1, 1000000);

        RuleFor(x => x.Status)
            .Must(value => value is null || Enum.IsDefined(value.Value))
            .WithMessage("Invalid announcement status.");
        RuleFor(x => x.RoleReceived)
            .Must(value => value is null || Enum.IsDefined(value.Value))
            .WithMessage("Invalid announcement receiver.");

        RuleFor(x => x.CategoryId)
            .Must(value => value is null || value != Guid.Empty)
            .WithMessage("Category ID must not be empty.");
    }
}
