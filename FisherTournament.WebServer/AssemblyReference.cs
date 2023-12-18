using System.Reflection;

namespace FisherTournament.WebServer
{
    public class AssemblyReference
    {
        public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
    }
}