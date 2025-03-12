using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using UrlShortener.Domain.Models;

namespace UrlShortener.Domain.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    TEntity Add(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);

    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = default,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = default,
        CancellationToken cancellationToken = default);

    Task<PagedList<TEntity>> GetAllPaginatedAsync(
        ushort pageNumber,
        ushort pageSize,
        Expression<Func<TEntity, bool>>? predicate = default,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = default,
        Expression<Func<TEntity, TEntity>>? selector = default,
        Expression<Func<TEntity, object>>? ascendingSortKeySelector = default,
        Expression<Func<TEntity, object>>? descendingSortKeySelector = default,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
}