using Domain.Common;

namespace Application.Teams.DTOs;

public record TeamEditModel(
    string Tag,
    string Title, 
    string? LogotypeUrl, 
    Region Region);