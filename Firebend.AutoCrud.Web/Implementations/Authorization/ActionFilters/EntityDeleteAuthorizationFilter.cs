using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public class EntityDeleteAuthorizationFilter<TKey, TEntity> : EntityAuthorizationFilter<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private static readonly string IdArg = nameof(IEntity<TKey>.Id).ToLower();

    public EntityDeleteAuthorizationFilter(string policy) : base(policy)
    {
    }

    protected override async Task AuthorizeRequestAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (await TryAuthorizeById(context, IdArg))
        {
            await next();
        }
    }
}
