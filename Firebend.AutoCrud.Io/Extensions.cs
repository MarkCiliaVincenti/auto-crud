using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Io
{
    public static class Extensions
    {
        /// <summary>
        /// Enables csv and excel export of entity records via an `/export` endpoint
        /// </summary>
        /// <param name="configure">A callback allowing for further configuration of the export settings</param>
        /// <example>
        /// <code>
        /// public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        ///  .ConfigureWebHostDefaults(webbuilder => { webBuilder.UseStartup<Startup>(); })
        ///  .ConfigureServices((hostContext, services) => {
        ///      services.UsingEfCrud(ef =>
        ///     {
        ///         ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///             forecast.WithDbContext<AppDbContext>()
        ///                 .AddCrud()
        ///                 .AddIo<Guid, WeatherForecast, V1>(io => io.WithMapper(x => new WeatherForecastExport(x)))
        ///                 .AddControllers<Guid, WeatherForecast, V1>(controllers => controllers
        ///                         .WithAllControllers(true)
        ///                         .WithOpenApiGroupName("WeatherForecasts")
        ///                         .WithIoControllers()
        ///                         .WithChangeTrackingControllers()
        ///                     )
        ///             )
        ///         });
        ///     })
        /// </code>
        /// </example>
        public static EntityCrudBuilder<TKey, TEntity> AddIo<TKey, TEntity, TVersion>(this EntityCrudBuilder<TKey, TEntity> builder,
            Action<IoConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity, TVersion>> configure = null)
            where TKey : struct
            where TEntity : class, IEntity<TKey>
            where TVersion : class, IAutoCrudApiVersion
        {
            using var config = new IoConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity, TVersion>(builder);
            configure?.Invoke(config);
            return builder;
        }
    }
}
