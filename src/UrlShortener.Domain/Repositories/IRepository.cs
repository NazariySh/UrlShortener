using System.Linq.Expressions;

namespace UrlShortener.Domain.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    TEntity Add(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);

    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
}