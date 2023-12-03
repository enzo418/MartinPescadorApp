namespace FisherTournament.Contracts.Tournaments
{
    public class EditTournamentContract
    {
        public string TournamentId { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public bool? TournamentFinishedState { get; set; }

        public EditTournamentContract(string tournamentId, string name, DateTime startDate, bool endTournament)
        {
            Name = name;
            StartDate = startDate;
            TournamentFinishedState = endTournament;
            TournamentId = tournamentId;
        }
    }
}
