using Microsoft.AspNetCore.Components.Forms;

namespace FisherTournament.WebServer.Common.Validation
{
    public static partial class ErrorOrValidationExtensions
    {
        public static void AddErrors<T>(this ValidationMessageStore messageStore, T model)
        {

        }

        public static EditContext AddValidationErrors<T>(this EditContext context,
                                                         List<ErrorOr.Error> errors,
                                                         ValidationMessageStore? messageStore,
                                                         T model)
        {
            if (messageStore is null || model is null) return context;

            foreach (var error in errors)
            {
                if (error.Type == ErrorOr.ErrorType.Validation)
                {
                    var fieldIdentifier = new FieldIdentifier(model, error.Code);
                    messageStore?.Add(fieldIdentifier, error.Description);
                }
            }

            context?.NotifyValidationStateChanged();

            return context!;
        }
    }
}