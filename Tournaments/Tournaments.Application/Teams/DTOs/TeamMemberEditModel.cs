using Domain.Teams.Members;

namespace Application.Teams.DTOs;

public record TeamMemberEditModel(
    string Nickname,
    string? AvatarUrl,
    TeamMemberRole Role);