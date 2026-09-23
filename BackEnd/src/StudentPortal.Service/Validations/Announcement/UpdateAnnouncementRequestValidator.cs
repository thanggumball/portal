using FluentValidation;
using StudentPortal.Common.DTOs.Announcement;

namespace StudentPortal.Service.Validations.Announcement;

public class UpdateAnnouncementRequestValidator
    : AbstractValidator<UpdateAnnouncementRequest>
{
    public UpdateAnnouncementRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(255)
            .WithMessage("Title must not exceed 255 characters.");

        RuleFor(request => request.Summary)
            .MaximumLength(1000)
            .WithMessage("Summary must not exceed 1000 characters.");

        RuleFor(request => request.Content)
            .NotEmpty()
            .WithMessage("Content is required.");

        RuleFor(request => request.RoleReceived)
            .IsInEnum()
            .WithMessage("Invalid announcement recipient.");

        RuleFor(request => request.CategoryIds)
            .NotEmpty()
            .WithMessage("At least one category is required.");

        RuleForEach(request => request.CategoryIds)
            .NotEqual(Guid.Empty)
            .WithMessage("Category ID must not be empty.");

        RuleFor(request => request.CategoryIds)
            .Must(categoryIds =>
                categoryIds is not null &&
                categoryIds.Distinct().Count() == categoryIds.Count)
            .WithMessage("Category IDs must not be duplicated.");
    }
}
