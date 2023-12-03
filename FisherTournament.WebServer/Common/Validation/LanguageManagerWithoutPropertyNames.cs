namespace FisherTournament.WebServer.Common.Validation
{
    public class LanguageManagerWithoutPropertyNames : FluentValidation.Resources.LanguageManager
    {
        // default: https://github.com/FluentValidation/FluentValidation/blob/main/src/FluentValidation/Resources/Languages/EnglishLanguage.cs
        public LanguageManagerWithoutPropertyNames()
        {
            AddTranslation("es", "NotNullValidator", "Es requerido.");
            AddTranslation("en-US", "NotNullValidator", "Is required.");

            AddTranslation("es", "NotEmptyValidator", "No debe estar vacío.");
            AddTranslation("en-US", "NotEmptyValidator", "Must not be empty.");

            AddTranslation("es", "MinimumLengthValidator", "Debe tener al menos {MinLength} caracteres. Usted ingresó {TotalLength} caracteres.");
            AddTranslation("en-US", "MinimumLengthValidator", "Must be at least {MinLength} characters. You entered {TotalLength} characters.");

            AddTranslation("es", "MaximumLengthValidator", "No debe tener más de {MaxLength} caracteres. Usted ingresó {TotalLength} caracteres.");
            AddTranslation("en-US", "MaximumLengthValidator", "Must be {MaxLength} characters or fewer. You entered {TotalLength} characters.");

            AddTranslation("es", "GreaterThanValidator", "Debe ser mayor que {ComparisonValue}.");
            AddTranslation("en-US", "GreaterThanValidator", "Must be greater than {ComparisonValue}.");

            AddTranslation("es", "GreaterThanOrEqualValidator", "Debe ser mayor o igual que {ComparisonValue}.");
            AddTranslation("en-US", "GreaterThanOrEqualValidator", "Must be greater than or equal to {ComparisonValue}.");

            AddTranslation("es", "LessThanValidator", "Debe ser menor que {ComparisonValue}.");
            AddTranslation("en-US", "LessThanValidator", "Must be less than {ComparisonValue}.");

            AddTranslation("es", "LessThanOrEqualValidator", "Debe ser menor o igual que {ComparisonValue}.");
            AddTranslation("en-US", "LessThanOrEqualValidator", "Must be less than or equal to {ComparisonValue}.");
        }
    }
}
