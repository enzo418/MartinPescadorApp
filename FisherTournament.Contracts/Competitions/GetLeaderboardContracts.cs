namespace FisherTournament.Contracts.Competitions;

public record struct CompetitionLeaderBoardItem(
    string FisherId,
    string FirstName,
    string LastName,
    int Position,
    int TotalScore
);

public record struct CompetitionCategoryLeaderBoard(
    string CategoryId,
    string CategoryName,
    IEnumerable<CompetitionLeaderBoardItem> LeaderBoard
);