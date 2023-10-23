namespace FisherTournament.Application.Common.Requests
{
    public interface IPagedListQuery
    {
        int Page { get; set; }
        int PageSize { get; set; }
    }
}