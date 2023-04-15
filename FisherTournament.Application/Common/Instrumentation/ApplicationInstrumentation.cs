using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FisherTournament.Application.Common.Instrumentation
{
    /// <summary>
    /// This class contains the Activity source that will be used by the components of the application layer.
    /// This separation is useful to easily differentiate between the layers in the UI.
    /// </summary>
    public class ApplicationInstrumentation : IDisposable
    {
        public const string ActivitySourceName = "FisherTournament.Application";
        public ActivitySource ActivitySource { get; }

        public ApplicationInstrumentation(ActivitySource activitySource)
        {
            string? version = typeof(ApplicationInstrumentation).Assembly.GetName().Version?.ToString();
            ActivitySource = activitySource;
            this.ActivitySource = new ActivitySource(ActivitySourceName, version);
        }

        public void Dispose()
        {
            this.ActivitySource.Dispose();
        }
    }
}