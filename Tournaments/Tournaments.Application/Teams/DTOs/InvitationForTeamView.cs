using Domain.Teams.Members;

namespace Application.Teams.DTOs;

public record InvitationForTeamView(
    Guid TeamId,
    Sender Sender,
    Guid ReceiverId, 
    TeamMemberRole Role);

public record Sender(Guid Id, string Nickname, string? AvatarUrl);