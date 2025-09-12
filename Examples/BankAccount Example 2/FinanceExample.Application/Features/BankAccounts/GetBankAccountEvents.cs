using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.BankAccounts;
using FinanceExample.Application.Contracts.Models;
using FinanceExample.Domain.Accounts;
using System.Text.Json;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Application.Features.BankAccounts
{
    public sealed class GetBankAccountEvents
    {
        public sealed record Query(
            Guid BankAccountId,
            int PageNumber = 1,
            int PageSize = 10) : IQuery<Result<GetBankAccountEventsResponse>>;

        internal sealed class Handler(IEventStore eventStore) 
            : IRequestHandler<Query, Result<GetBankAccountEventsResponse>>
        {
            private readonly IEventStore _eventStore = eventStore;

            public async Task<Result<GetBankAccountEventsResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var bankAccountId = BankAccountId.From(request.BankAccountId);
                
                var pagedResult = await _eventStore.GetEventsPagedAsync<BankAccountId>(
                    bankAccountId, 
                    request.PageNumber, 
                    request.PageSize, 
                    cancellationToken);

                var dynamicEvents = pagedResult.Items.Select(domainEvent => new DynamicEventDetail
                {
                    EventId = domainEvent.Id,
                    EventType = domainEvent.GetType().Name,
                    Timestamp = DateTime.UtcNow, // For display purposes - could be extracted from event if available
                    EventData = JsonSerializer.SerializeToElement(domainEvent, domainEvent.GetType(), new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    })
                }).ToList();

                return new GetBankAccountEventsResponse(
                    request.BankAccountId,
                    dynamicEvents,
                    pagedResult.TotalCount,
                    pagedResult.PageNumber,
                    pagedResult.PageSize,
                    pagedResult.TotalPages,
                    pagedResult.HasNextPage,
                    pagedResult.HasPreviousPage);
            }
        }
    }
}