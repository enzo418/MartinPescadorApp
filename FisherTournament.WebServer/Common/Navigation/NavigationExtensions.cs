using Microsoft.JSInterop;

namespace FisherTournament.WebServer.Common.Navigation
{
	public static partial class NavigationExtensions
	{
		public static ValueTask GoBack(this IJSRuntime runtime)
		{
			return runtime.InvokeVoidAsync("history.back");
		}
	}
}
