
namespace FisherTournament.Infrastructure.Settings
{
    public class OpenTelemetrySettings
    {
        public string ApplicationVersion { get; set; } = null!;
        public OpenTelemetryExporterSettings Exporter { get; set; } = null!;
    }

    public class OpenTelemetryExporterSettings
    {
        public OpenTelemetryConsoleExporterSettings Console { get; set; } = null!;
        public OpenTelemetryOtlpExporterSettings Otlp { get; set; } = null!;
    }

    public class OpenTelemetryOtlpExporterSettings
    {
        public string Endpoint { get; set; } = null!;
    }

    public class OpenTelemetryConsoleExporterSettings
    {
        public bool EnableOnTrace { get; set; } = true;
        public bool EnableOnMetric { get; set; } = true;
        public bool EnableOnLog { get; set; } = true;
    }
}