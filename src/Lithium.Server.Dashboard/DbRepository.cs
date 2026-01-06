using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Lithium.Server.Dashboard;

public abstract class DbRepository<TContext, TEntity>(TContext db)
    where TContext : DbContext
    where TEntity : class
{
    // -----------------------------
    // CREATE
    // -----------------------------
    public virtual async Task<TEntity> InsertAsync(TEntity entity)
    {
        await db.Set<TEntity>().AddAsync(entity);
        await db.SaveChangesAsync();

        return entity;
    }

    public virtual async Task InsertRangeAsync(IReadOnlyList<TEntity> entities)
    {
        await db.Set<TEntity>().AddRangeAsync(entities);
        await db.SaveChangesAsync();
    }

    // -----------------------------
    // READ
    // -----------------------------
    public virtual async Task<TEntity?> GetByIdAsync(object id)
    {
        return await db.Set<TEntity>().FindAsync(id);
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await db.Set<TEntity>().AsNoTracking().ToListAsync();
    }

    public virtual async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await db.Set<TEntity>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
    }

    public virtual async Task<TEntity?> FirstAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await db.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate);
    }

    // -----------------------------
    // UPDATE
    // -----------------------------
    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        db.Set<TEntity>().Update(entity);
        await db.SaveChangesAsync();

        return entity;
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        db.Set<TEntity>().UpdateRange(entities);
        await db.SaveChangesAsync();
    }

    // -----------------------------
    // DELETE
    // -----------------------------
    public virtual async Task<bool> DeleteAsync(object id)
    {
        var entity = await db.Set<TEntity>().FindAsync(id);
        if (entity is null) return false;

        db.Set<TEntity>().Remove(entity);
        await db.SaveChangesAsync();

        return true;
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        db.Set<TEntity>().Remove(entity);
        await db.SaveChangesAsync();
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        db.Set<TEntity>().RemoveRange(entities);
        await db.SaveChangesAsync();
    }

    // -----------------------------
    // EXISTS
    // -----------------------------
    public virtual async Task<bool> ExistsAsync(object id)
    {
        return await db.Set<TEntity>().FindAsync(id) is not null;
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await db.Set<TEntity>().CountAsync(predicate) is not 0;
    }
}