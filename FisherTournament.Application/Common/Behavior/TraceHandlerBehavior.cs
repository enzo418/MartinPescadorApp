using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Application.Common.Instrumentation;
using MediatR;

namespace FisherTournament.Application.Common.Behavior
{
    public class TraceHandlerBehavior<TRequest, TResponse>
     : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ApplicationInstrumentation _instrumentation;

        public TraceHandlerBehavior(ApplicationInstrumentation instrumentation)
        {
            _instrumentation = instrumentation;
        }

        public Task<TResponse> Handle(TRequest request,
                                      RequestHandlerDelegate<TResponse> next,
                                      CancellationToken cancellationToken)
        {
            var activityName = $"{typeof(TRequest).Name}Handler";
            using var activity = _instrumentation.ActivitySource.StartActivity(activityName);
            return next();
        }
    }
}