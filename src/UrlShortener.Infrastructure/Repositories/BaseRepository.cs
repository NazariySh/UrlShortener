using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using UrlShortener.Domain.Models;
using UrlShortener.Domain.Repositories;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    protected readonly ApplicationDbContext DbContext;

    protected BaseRepository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public TEntity Add(TEntity entity)
    {
        return DbContext.Set<TEntity>().Add(entity).Entity;
    }

    public void Update(TEntity entity)
    {
        DbContext.Set<TEntity>().Update(entity);
    }

    public void Remove(TEntity entity)
    {
        DbContext.Set<TEntity>().Remove(entity);
    }

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
    Expression<Func<TEntity, bool>>? predicate = default,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = default,
    CancellationToken cancellationToken = default)
    {
        var query = DbContext.Set<TEntity>().AsNoTracking();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (include is not null)
        {
            query = include(query);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PagedList<TEntity>> GetAllPaginatedAsync(
        ushort pageNumber,
        ushort pageSize,
        Expression<Func<TEntity, bool>>? predicate = default,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = default,
        Expression<Func<TEntity, TEntity>>? selector = default,
        Expression<Func<TEntity, object>>? ascendingSortKeySelector = default,
        Expression<Func<TEntity, object>>? descendingSortKeySelector = default,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Set<TEntity>().AsNoTracking();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (include is not null)
        {
            query = include(query);
        }

        if (ascendingSortKeySelector is not null)
        {
            query = query.OrderBy(ascendingSortKeySelector);
        }

        if (descendingSortKeySelector is not null)
        {
            query = query.OrderByDescending(descendingSortKeySelector);
        }

        if (selector is not null)
        {
            query = query.Select(selector);
        }

        return await PagedList<TEntity>.CreateAsync(
            query,
            pageNumber,
            pageSize,
            cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TEntity>()
            .AnyAsync(predicate, cancellationToken);
    }
}