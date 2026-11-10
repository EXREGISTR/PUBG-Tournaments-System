namespace Infrastructure.Database.Repositories;

public static class QueryableExtensions {
    public static IQueryable<T> ApplyPagedPagination<T>(this IQueryable<T> query, int page, int pageSize) =>
        query.Skip((page - 1) * pageSize).Take(pageSize);
}