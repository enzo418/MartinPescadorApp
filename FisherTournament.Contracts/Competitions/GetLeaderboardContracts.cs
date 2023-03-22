namespace FisherTournament.Contracts.Competitions;

public record struct CompetitionLeaderBoardItem(
    string FisherId,
    string FirstName,
    string LastName,
    int TotalScore
);

public record struct CompetitionCategoryLeaderBoard(
    string CategoryId,
    string CategoryName,
    IEnumerable<CompetitionLeaderBoardItem> LeaderBoard
);