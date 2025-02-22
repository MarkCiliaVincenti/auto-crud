using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Models;
using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Web.Sample.DomainEvents
{
    public partial class EfPersonDomainEventHandler : BaseDisposable, IEntityAddedDomainEventSubscriber<EfPerson>,
        IEntityUpdatedDomainEventSubscriber<EfPerson>
    {
        [LoggerMessage(EventId = 0, Message = "Person Added! Person: {modifiedJson}. Context: {contextJson}", Level = LogLevel.Debug)]
        public static partial void LogPersonAdded(ILogger logger, string modifiedJson, string contextJson);

        [LoggerMessage(EventId = 2, Message = "Catch Phrase: {catchPhrase}", Level = LogLevel.Debug)]
        public static partial void LogCatchPhrase(ILogger logger, string catchPhrase);

        [LoggerMessage(EventId = 3, Message = "Catch Phrase From Scope: {catchPhrase}", Level = LogLevel.Debug)]
        public static partial void LogCatchPhraseFromScope(ILogger logger, string catchPhrase);

        [LoggerMessage(EventId = 4, Message = "No Scope Context", Level = LogLevel.Debug)]
        public static partial void LogNoScopeContext(ILogger logger);

        [LoggerMessage(EventId = 5, Message = "Person Updated! Operations: {operationsJson} Original: {originalJson}. Modified: {modifiedJson}. Context: {contextJson}",
            Level = LogLevel.Debug)]
        public static partial void LogPersonUpdated(ILogger logger, string operationsJson, string originalJson, string modifiedJson, string contextJson);

        private readonly ILogger _logger;
        private readonly ScopedConsumeContextProvider _scoped;

        public EfPersonDomainEventHandler(ILogger<EfPersonDomainEventHandler> logger,
            ScopedConsumeContextProvider scoped)
        {
            _logger = logger;
            _scoped = scoped;
        }

        public Task EntityAddedAsync(EntityAddedDomainEvent<EfPerson> domainEvent, CancellationToken cancellationToken = default)
        {
            var modified = domainEvent.Entity;
            var modifiedJson = JsonConvert.SerializeObject(modified, Formatting.Indented);
            var contextJson = JsonConvert.SerializeObject(domainEvent.EventContext, Formatting.Indented);

            LogPersonAdded(_logger, modifiedJson, contextJson);
            LogCatchPhrase(_logger, domainEvent.EventContext.GetCustomContext<SampleDomainEventContext>()?.CatchPhraseModel?.CatchPhrase);

            if (_scoped.HasContext && _scoped.GetContext().TryGetMessage(out ConsumeContext<EntityAddedDomainEvent<EfPerson>> consumeContext))
            {
                LogCatchPhraseFromScope(_logger, consumeContext?.Message?.EventContext?.GetCustomContext<SampleDomainEventContext>()?.CatchPhraseModel?.CatchPhrase);
            }
            else
            {
                LogNoScopeContext(_logger);
            }

            return Task.CompletedTask;
        }

        public Task EntityUpdatedAsync(EntityUpdatedDomainEvent<EfPerson> domainEvent, CancellationToken cancellationToken = default)
        {
            var original = domainEvent.Previous;
            var modified = domainEvent.Modified;
            var originalJson = JsonConvert.SerializeObject(original, Formatting.Indented);
            var modifiedJson = JsonConvert.SerializeObject(modified, Formatting.Indented);
            var contextJson = JsonConvert.SerializeObject(domainEvent.EventContext, Formatting.Indented);
            var operationsJson = JsonConvert.SerializeObject(domainEvent.Operations, Formatting.Indented);

            LogPersonUpdated(_logger, operationsJson, originalJson, modifiedJson, contextJson);

            LogCatchPhrase(_logger, domainEvent.EventContext.GetCustomContext<SampleDomainEventContext>()?.CatchPhraseModel?.CatchPhrase);

            if (_scoped.HasContext && _scoped.GetContext().TryGetMessage(out ConsumeContext<EntityUpdatedDomainEvent<EfPerson>> consumeContext))
            {
                LogCatchPhraseFromScope(_logger, consumeContext?.Message?.EventContext?.GetCustomContext<SampleDomainEventContext>()?.CatchPhraseModel?.CatchPhrase);
            }
            else
            {
                LogNoScopeContext(_logger);
            }

            return Task.CompletedTask;
        }
    }
}
