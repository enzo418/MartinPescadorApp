using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Contracts.Common;

namespace FisherTournament.Contracts.Fisher
{
    public record struct GetFishersRequest(string? Name, int Page, int PageSize);

    public record struct FisherItem(string Id, string Name);

    public class GetFishersResponse : PagedListResponse<FisherItem> { }
}