namespace Application.Common;

public interface IUserContext {
    public Guid GetUserId();
    public bool IsAdmin();
}