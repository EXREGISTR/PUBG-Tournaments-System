using Domain.Common;
using Domain.Teams.Members;

namespace Application.Teams.DTOs;

public record TeamView(
    Guid Id, 
    string Tag,
    string Title,
    string? LogotypeUrl,
    Region Region,
    IReadOnlyCollection<TeamMemberView> Members);

public record TeamMemberView(
    Guid Id,
    string Nickname,
    string? AvatarUrl, 
    TeamMemberRole Role);