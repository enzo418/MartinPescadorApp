namespace FisherTournament.Application.LeaderBoard
{
    public static class TieBreakingReasonGenerator
    {
        public static string ByLargerPiece(int largerPiece)
        {
            return $"M/P {largerPiece}";
        }

        public static string ByNthLargerPiece(int nth, int pieceValue)
        {
            if (nth < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(nth), "nth must be greater than 0");
            }

            return $"M/{nth}P {pieceValue}";
        }

        public static string ByDefault()
        {
            return "D";
        }
    }
}
