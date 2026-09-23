using FluentValidation;
using StudentPortal.Common.DTOs.Announcement;
using StudentPortal.Common.Enums;

namespace StudentPortal.Service.Validations.Announcement;

public class CreateAnnouncementRequestValidator : AbstractValidator<CreateAnnouncementRequest>
{
    public CreateAnnouncementRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(255)
            .WithMessage("Title must not exceed 255 characters.");

        RuleFor(x => x.Summary)
            .MaximumLength(1000)
            .WithMessage("Summary must not exceed 1000 characters.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required.");

        RuleFor(x => x.RoleReceived)
            .IsInEnum()
            .WithMessage("Invalid announcement recipient.");

        RuleFor(x => x.CategoryIds)
            .NotEmpty()
            .WithMessage("At least one category is required.");

        RuleForEach(x => x.CategoryIds)
            .NotEqual(Guid.Empty)
            .WithMessage("Category ID must not be empty.");

        RuleFor(x => x.CategoryIds)
            .Must(categoryIds =>
                categoryIds is not null &&
                categoryIds.Distinct().Count() == categoryIds.Count)
            .WithMessage("Category IDs must not be duplicated.");
    }
}
