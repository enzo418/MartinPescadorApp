using ClosedXML.Excel;
using ErrorOr;
using FisherTournament.Application.Competitions.Queries.GetCompetition;
using FisherTournament.Application.Competitions.Queries.GetLeaderBoard;
using FisherTournament.Application.Competitions.Queries.GetTournamentCompetitions;
using FisherTournament.Application.Tournaments.Queries.GetTournament;
using FisherTournament.Application.Tournaments.Queries.GetTournamentLeaderBoard;
using MediatR;
using System.Text.RegularExpressions;

namespace FisherTournament.WebServer.Services.ExportLeaderboard
{
    public class ExportLeaderboardService
    {
        private readonly ISender _sender;

        private Regex _asciiRegex = new(@"[^a-zA-Z0-9\s]", (RegexOptions)0);

        public ExportLeaderboardService(ISender sender)
        {
            _sender = sender;
        }

        public async Task<ErrorOr<string>> ExportTournamentLeaderboard(string TournamentId)
        {
            var tournamentDataRequest = await _sender.Send(new GetTournamentQuery(TournamentId));
            if (tournamentDataRequest.IsError) return tournamentDataRequest.Errors;

            var tournamentCompetitionsRequest = await _sender.Send(new GetTournamentCompetitionsQuery(TournamentId));
            if (tournamentCompetitionsRequest.IsError) return tournamentCompetitionsRequest.Errors;

            var categoryLeaderboardReq = await _sender.Send(new GetTournamentLeaderBoardQuery(TournamentId));
            if (categoryLeaderboardReq.IsError) return categoryLeaderboardReq.Errors;

            var sanitizedTournamentName = _asciiRegex.Replace(tournamentDataRequest.Value.Name, "");

            var wb = new XLWorkbook();

            foreach (var cat in categoryLeaderboardReq.Value)
            {
                var sanitizedCatName = _asciiRegex.Replace(cat.Name, "");

                var ws = wb.Worksheets.Add($"{sanitizedCatName}");

                // Header with the tournament name and year and the category name
                var rngHeader = ws.Cell(1, 1).SetValue($"Torneo {sanitizedTournamentName} {tournamentDataRequest.Value.StartDate.Year} - categoría {cat.Name}");
                rngHeader.Style.Font.Bold = true;
                ws.Range(rngHeader, ws.Cell(1, 2 + tournamentCompetitionsRequest.Value.Count)).Merge();
                rngHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Next is Pos final | Nombre | Fecha 1 | Fecha 2 | Fecha 3 | ... | Total
                ws.Cell(2, 1).SetValue("Pos final");
                ws.Cell(2, 2).SetValue("Nombre");

                int col = 3;

                tournamentCompetitionsRequest.Value.OrderBy(c => c.N).ToList().ForEach(comp =>
                    ws.Cell(2, col++).SetValue($"Fecha {comp.N}")
                );

                ws.Cell(2, ++col).SetValue("Total");

                // Now we add the data
                int row = 3;

                cat.LeaderBoard.ToList().ForEach(item =>
                {
                    ws.Cell(row, 1).SetValue(item.Position);
                    ws.Cell(row, 2).SetValue(item.Name);

                    int j = 3;
                    item.CompetitionPositions.ForEach(score =>
                    {
                        ws.Cell(row, j++).SetValue(score);
                    });

                    ws.Cell(row, col).SetValue(item.CompetitionPositions.Sum());

                    row++;
                });
            }

            // Final styling
            wb.Worksheets.ToList().ForEach(ws =>
            {
                ws.Columns().AdjustToContents();
                ws.Rows().AdjustToContents();
            });

            var fileName = $"Torneo {sanitizedTournamentName} {tournamentDataRequest.Value.StartDate.Year} - categorias.xlsx";
            wb.SaveAs(fileName);

            return fileName;
        }

        public async Task<ErrorOr<string>> ExportCompetitionLeaderboard(string CompetitionId)
        {
            var competitionDataRequest = await _sender.Send(new GetCompetitionQuery(CompetitionId));
            if (competitionDataRequest.IsError) return competitionDataRequest.Errors;

            var tournamentDataRequest = await _sender.Send(new GetTournamentQuery(competitionDataRequest.Value.TuornamentId));
            if (tournamentDataRequest.IsError) return tournamentDataRequest.Errors;

            var competitionLeaderboardReq = await _sender.Send(new GetCompetitionLeaderBoardQuery(CompetitionId));
            if (competitionLeaderboardReq.IsError) return competitionLeaderboardReq.Errors;

            var sanitizedTournamentName = _asciiRegex.Replace(tournamentDataRequest.Value.Name, "");

            var wb = new XLWorkbook();

            foreach (var cat in competitionLeaderboardReq.Value)
            {
                var sanitizedCatName = _asciiRegex.Replace(cat.Name, "");

                var ws = wb.Worksheets.Add($"{sanitizedCatName}");

                // Title
                var rngHeader = ws.Cell(1, 1).SetValue($"Categoría {cat.Name} - {competitionDataRequest.Value.N}° Fecha");
                rngHeader.Style.Font.Bold = true;
                ws.Range(rngHeader, ws.Cell(1, 4)).Merge();
                rngHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Colums header
                ws.Cell(2, 1).SetValue("Posición");
                ws.Cell(2, 2).SetValue("Nombre");
                ws.Cell(2, 3).SetValue("Puntaje total");
                ws.Cell(2, 4).SetValue("Desempate");

                // Now we add the data
                int row = 3;

                cat.LeaderBoard.ToList().ForEach(item =>
                {
                    ws.Cell(row, 1).SetValue(item.Position);
                    ws.Cell(row, 2).SetValue(item.Name);
                    ws.Cell(row, 3).SetValue(item.TotalScore);
                    ws.Cell(row, 4).SetValue(item.TieBreakingReason);

                    row++;
                });
            }

            // Final styling
            wb.Worksheets.ToList().ForEach(ws =>
            {
                ws.Columns().AdjustToContents();
                ws.Rows().AdjustToContents();
            });

            var fileName = $"Torneo {sanitizedTournamentName} - {competitionDataRequest.Value.N}° Fecha.xlsx";

            wb.SaveAs(fileName);

            return fileName;
        }
    }
}
