using FisherTournament.Domain;
using FisherTournament.Infrastracture.Persistence;
using FisherTournament.Infrastracture.Persistence.Tournaments;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.IntegrationTests;

public static partial class Extension
{
    public static T PrepareAdd<T>(this TournamentFisherDbContext context, T entity) where T : class
    {
        context.Add(entity);
        return entity;
    }

    public static Fisher PrepareFisher(this TournamentFisherDbContext context, string FirstName, string LastName)
    {
        var user = context.PrepareAdd(User.Create(FirstName, LastName));
        var fisher = context.PrepareAdd(Fisher.Create(user.Id));
        return fisher;
    }

    // public static async Task<TEntity?> FindAsync<TEntity>(this TournamentFisherDbContext context, params object[] id)
    //      where TEntity : class
    // {
    //     return await context.Set<TEntity>().FindAsync(id);
    // }

    public static async Task<IEnumerable<TEntity>> FindAllAsync<TEntity, TId>(this TournamentFisherDbContext context, IEnumerable<TId> Ids)
        where TId : notnull
        where TEntity : AggregateRoot<TId>
    {
        return await context.Set<TEntity>().Where(c => Ids.Contains(c.Id)).ToListAsync();
    }

    public static async Task<TEntity> WithAsync<TEntity>(this TournamentFisherDbContext context, TEntity entity, Action<TEntity>? beforeSave = null) where TEntity : class
    {
        await context.Set<TEntity>().AddAsync(entity);

        if (beforeSave is not null)
        {
            beforeSave(entity);
        }

        await context.SaveChangesAsync(default);
        return entity;
    }

    public static async Task<IEnumerable<TEntity>> AddRangeAsync<TEntity>(this TournamentFisherDbContext context, IEnumerable<TEntity> entities) where TEntity : class
    {
        await context.Set<TEntity>().AddRangeAsync(entities);
        await context.SaveChangesAsync(default);
        return entities;
    }

    public static async Task<int> CountAsync<TEntity>(this TournamentFisherDbContext context) where TEntity : class
    {
        return await context.Set<TEntity>().CountAsync();
    }

    public static async Task<Fisher> WithFisherAsync(this TournamentFisherDbContext context, string FirstName, string LastName)
    {
        var fisher = context.PrepareFisher(FirstName, LastName);
        await context.SaveChangesAsync(default);
        return fisher;
    }

    public static async Task SaveChangesAndClear(this TournamentFisherDbContext context)
    {
        await context.SaveChangesAsync(default);
        context.ChangeTracker.Clear();
    }
}