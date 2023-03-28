namespace FisherTournament.Contracts.Tournaments;

public record TournamentLeaderBoardCategory(
    string CategoryId,
    string CategoryName,
    IEnumerable<TournamentLeaderBoardItem> LeaderBoard
);

public record TournamentLeaderBoardItem(
    string FisherId,
    string FirstName,
    string LastName,
    int Position
);