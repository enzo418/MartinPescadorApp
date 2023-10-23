using Microsoft.AspNetCore.Components.Routing;

namespace FisherTournament.WebServer.Navigation
{
    public class NavigationHistory
    {
        private readonly Stack<string> _history;

        public NavigationHistory()
        {
            _history = new Stack<string>();
        }

        public void OnLocationChanged(object? _, LocationChangedEventArgs args) =>
            Record(args.Location);

        public void Record(string path)
        {
            if (_history.Count > 0 && _history.Peek() == path)
                return;
            else if (_history.Count > 200) _history.Pop();

            _history.Push(path);
        }

        public string LastOrHome() =>
            _history.Any() ? _history.Pop() : "/";
    }
}