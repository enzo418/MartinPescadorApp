using Microsoft.Fast.Components.FluentUI;

namespace FisherTournament.WebServer.Common.Errors
{
    public static partial class ErrorOrToastExtensions
    {
        public static void ShowErrors(this IToastService toastService, List<ErrorOr.Error> errors)
        {
            foreach (var error in errors)
            {
                if (error.Type == ErrorOr.ErrorType.Validation)
                {
                    //var fieldIdentifier = new FieldIdentifier(model, error.Code);
                    //messageStore?.Add(fieldIdentifier, error.Description);
                    toastService.ShowError(error.Description, 3);
                } else
                {
                    toastService.ShowError(error.Description, 3);
                }
            }
        }
    }
}
