using Domain.Common;
using Domain.Common.Extensions;
using FluentValidation;

namespace Domain.Teams.TeamMembers;

public static class TeamMemberValidator {
    private static readonly Validator validator = new();
    private record struct Parameters(string Nickname, string? AvatarUrl);
    
    private class Validator : AbstractValidator<Parameters> {
        public Validator() {
            RuleFor(x => x.Nickname)
                .Length(3, 20)
                .Matches("^[a-zA-Z0-9_-]+$");

            RuleFor(x => x.AvatarUrl)
                .CorrectHostAndScheme()
                .When(x => x.AvatarUrl != null);
        }
    }
    
    public static void EnsureValid(string nickname, string? avatarUrl) =>
        ExceptionFactories.TeamMember.ThrowValidationExceptionIfAnyError(
            validator.Validate(new Parameters(nickname, avatarUrl)));
}