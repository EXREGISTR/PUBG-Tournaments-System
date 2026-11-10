using Domain.Teams.Members;

namespace Domain.Teams.Invitations;

public class InvitationForTeam {
    public Guid TeamId { get; private init; }
    public Guid SenderId { get; private init; }
    public Guid ReceiverId { get; private init; }
    public TeamMemberRole Role { get; private init; }
    
    public TeamMember Sender { get; private init; } = null!;

    private InvitationForTeam() { }
    
    public InvitationForTeam(Guid teamId, Guid senderId, Guid receiverId, TeamMemberRole role) {
        TeamId = teamId;
        SenderId = senderId;
        ReceiverId = receiverId;
        Role = role;
    }
}