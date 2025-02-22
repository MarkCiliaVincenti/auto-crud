using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client;

public abstract class AbstractDbContextProvider<TKey, TEntity, TContext> : IDbContextProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
    where TContext : DbContext, IDbContext
{
    private readonly ILogger _logger;
    private readonly IDbContextConnectionStringProvider<TKey, TEntity> _connectionStringProvider;
    private readonly IDbContextOptionsProvider<TKey, TEntity> _optionsProvider;
    private readonly IMemoizer _memoizer;

    protected AbstractDbContextProvider(IDbContextConnectionStringProvider<TKey, TEntity> connectionStringProvider,
        IDbContextOptionsProvider<TKey, TEntity> optionsProvider,
        ILoggerFactory loggerFactory,
        IMemoizer memoizer)
    {
        _connectionStringProvider = connectionStringProvider;
        _optionsProvider = optionsProvider;
        _memoizer = memoizer;
        _logger = loggerFactory.CreateLogger<AbstractDbContextProvider<TKey, TEntity, TContext>>();
    }

    protected async Task<IDbContext> CreateContextAsync(IDbContextFactory<TContext> factory,
        CancellationToken cancellationToken)
    {
        var context = await factory.CreateDbContextAsync(cancellationToken);

        if (context is DbContext dbContext)
        {
            await _memoizer.MemoizeAsync<
                bool,
                (AbstractDbContextProvider<TKey, TEntity, TContext> self, DbContext dbContext, CancellationToken
                cancellationToken)>(
                GetMemoizeKey(typeof(TContext)),
                static arg => arg.self.InitContextAsync(arg.dbContext, arg.cancellationToken),
                (this, dbContext, cancellationToken),
                cancellationToken);
        }

        return context;
    }

    protected virtual async Task<bool> InitContextAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call ensure created");
        }

        try
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to call migrations");
        }

        return true;
    }

    public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = await _connectionStringProvider
            .GetConnectionStringAsync(cancellationToken)
            .ConfigureAwait(false);

        var options = _optionsProvider.GetDbContextOptions<TContext>(connectionString);

        var factory = _memoizer.Memoize(
                GetPooledKey(typeof(TContext)),
                static opts => new PooledDbContextFactory<TContext>(opts),
                options);

        return await CreateContextAsync(factory, cancellationToken);
    }

    public async Task<IDbContext> GetDbContextAsync(DbConnection connection,
        CancellationToken cancellationToken = default)
    {
        var options = _optionsProvider.GetDbContextOptions<TContext>(connection);

        return await CreateContextAsync(new PooledDbContextFactory<TContext>(options), cancellationToken);
    }

    private string _memoizeKey;
    protected virtual string GetMemoizeKey(Type dbContextType) => _memoizeKey ??= $"{dbContextType.FullName}.Init";

    private string _poolKey;
    protected virtual string GetPooledKey(Type dbContextType) => _poolKey ??= $"{dbContextType.FullName}.Pooled";
}
