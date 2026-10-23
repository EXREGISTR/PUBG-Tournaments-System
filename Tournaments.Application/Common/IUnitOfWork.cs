namespace Application.Common;

public interface IUnitOfWork {
    public Task<int> SaveChanges(CancellationToken token = default);
}