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
    public partial class ExportLeaderboardService
    {
        private readonly ISender _sender;

        [GeneratedRegex("[^\\p{L}\\p{N}\\s\\p{M}]", RegexOptions.None)]
        private static partial Regex SanitizeRegex();

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

            var sanitizedTournamentName = SanitizeRegex().Replace(tournamentDataRequest.Value.Name, "");

            var wb = new XLWorkbook();

            foreach (var cat in categoryLeaderboardReq.Value)
            {
                var sanitizedCatName = SanitizeRegex().Replace(cat.Name, "");

                var ws = wb.Worksheets.Add($"{sanitizedCatName}");

                // Header with the tournament name and year and the category name
                var rngHeader = ws.Cell(1, 1).SetValue($"Torneo {sanitizedTournamentName} {tournamentDataRequest.Value.StartDate.Year} - categoría {cat.Name}".ToUpper());

                // Next is Pos final | Nombre | Fecha 1 | Fecha 2 | Fecha 3 | ... | Total
                ws.Cell(2, 1).SetValue("POS");
                ws.Cell(2, 2).SetValue("PESCADOR");

                int col = 3;

                tournamentCompetitionsRequest.Value.OrderBy(c => c.N).ToList().ForEach(comp =>
                    ws.Cell(2, col++).SetValue($"{comp.N}ª")
                );

                ws.Cell(2, col).SetValue("TOTAL");


                // Now we add the data
                int row = 3;

                cat.LeaderBoard.ToList().ForEach(item =>
                {
                    ws.Cell(row, 1).SetValue(item.Position);
                    ws.Cell(row, 2).SetValue(item.Name.ToUpper());

                    int j = 3;
                    item.CompetitionPositions.ForEach(score =>
                    {
                        ws.Cell(row, j++).SetValue(score);
                    });

                    ws.Cell(row, col).SetValue(item.CompetitionPositions.Sum());

                    row++;
                });

                ApplyCategoryHeaderStyle(ws.Range(1, 1, 1, ws.Worksheet.ColumnsUsed().Count()));
                ApplyCategoryColumsHeaderStyle(ws.Range(2, 1, 2, ws.Worksheet.ColumnsUsed().Count()));
                ApplyPositionsRowStyle(ws.Range(2, 1, ws.Worksheet.RowsUsed().Count(), 1));

                CenterHorizontally(ws.Range(2, 1, ws.Worksheet.RowsUsed().Count(), 1)); // Positions
                CenterHorizontally(ws.Range(2, 3, ws.Worksheet.RowsUsed().Count(), ws.Worksheet.ColumnsUsed().Count())); // All except names

                ApplyDataStyle(ws.Range(3, 2, ws.Worksheet.RowsUsed().Count(), ws.Worksheet.ColumnsUsed().Count()));
            }

            ApplyDefaultStyle(wb);

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

            var sanitizedTournamentName = SanitizeRegex().Replace(tournamentDataRequest.Value.Name, "");

            var wb = new XLWorkbook();

            foreach (var cat in competitionLeaderboardReq.Value)
            {
                var sanitizedCatName = SanitizeRegex().Replace(cat.Name, "");

                var ws = wb.Worksheets.Add($"{sanitizedCatName}");

                // Title
                ws.Cell(1, 1).SetValue($"Categoría {cat.Name} - {competitionDataRequest.Value.N}° Fecha".ToUpper());
                ApplyCategoryHeaderStyle(ws.Range(1, 1, 1, 4));

                // Colums header
                ws.Cell(2, 1).SetValue("POS");
                ws.Cell(2, 2).SetValue("PESCADOR");
                ws.Cell(2, 3).SetValue("TOTAL");
                ws.Cell(2, 4).SetValue("DESEMPATE");

                var rngHeader2 = ws.Range(2, 1, 2, 4);
                ApplyCategoryColumsHeaderStyle(rngHeader2);

                // Now we add the data
                int row = 3;

                cat.LeaderBoard.ToList().ForEach(item =>
                {
                    ws.Cell(row, 1).SetValue(item.Position);
                    ws.Cell(row, 2).SetValue(item.Name.ToUpper());
                    ws.Cell(row, 3).SetValue(item.TotalScore);
                    ws.Cell(row, 4).SetValue(item.TieBreakingReason);

                    row++;
                });

                ApplyPositionsRowStyle(ws.Range(2, 1, ws.Worksheet.RowsUsed().Count(), 1));

                CenterHorizontally(ws.Range(2, 1, ws.Worksheet.RowsUsed().Count(), 1)); // Positions
                CenterHorizontally(ws.Range(2, 3, ws.Worksheet.RowsUsed().Count(), ws.Worksheet.ColumnsUsed().Count())); // All except names

                ApplyDataStyle(ws.Range(3, 2, ws.Worksheet.RowsUsed().Count(), ws.Worksheet.ColumnsUsed().Count()));
            }

            ApplyDefaultStyle(wb);


            var fileName = $"Torneo {sanitizedTournamentName} - {competitionDataRequest.Value.N}° Fecha.xlsx";

            wb.SaveAs(fileName);

            return fileName;
        }

        internal readonly static XLColor Color1 = XLColor.FromArgb(139, 174, 182);
        internal readonly static XLColor Color2 = XLColor.FromArgb(98, 167, 183);
        internal readonly static XLColor Color3 = XLColor.FromArgb(252, 215, 173);

        internal static void ApplyCategoryHeaderStyle(IXLRange range)
        {
            range.Merge();
            range.Style.Fill.SetBackgroundColor(Color1);
            range.Style.Font.Bold = true;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        internal static void ApplyCategoryColumsHeaderStyle(IXLRange range)
        {
            range.Style.Fill.SetBackgroundColor(Color2)
                            .Font.SetBold(true)
                            .Font.SetItalic(true);
        }

        internal static void ApplyPositionsRowStyle(IXLRange range)
        {
            range.Style.Fill.SetBackgroundColor(Color2)
                       .Font.SetBold(true);
        }

        internal static void ApplyDefaultStyle(IXLRange range)
        {
            range.Style.Font.SetFontName("Arial")
                            .Font.SetFontSize(12)
                            .Border.SetTopBorder(XLBorderStyleValues.Thin)
                            .Border.SetRightBorder(XLBorderStyleValues.Thin)
                            .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                            .Border.SetLeftBorder(XLBorderStyleValues.Thin);
        }

        internal static void ApplyDataStyle(IXLRange range)
        {
            range.Style.Fill.SetBackgroundColor(Color3);
        }

        internal static void ApplyDefaultStyle(XLWorkbook wb)
        {
            wb.Worksheets.ToList().ForEach(ws =>
            {
                ApplyDefaultStyle(ws.RangeUsed());
                ws.Columns().AdjustToContents(10, double.MaxValue);
                ws.Rows().AdjustToContents(10, double.MaxValue);
            });
        }

        internal static void CenterHorizontally(IXLRange range)
        {
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
    }

}
