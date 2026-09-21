using FluentValidation;
using StudentPortal.Common.DTOs.Auth;

namespace StudentPortal.Service.Validations.Auth;

public class RefreshTokenRequestValidator
    : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}
