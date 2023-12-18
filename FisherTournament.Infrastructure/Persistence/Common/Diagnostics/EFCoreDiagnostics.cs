using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FisherTournament.Infrastructure.Persistence.Common.Diagnostics
{
    public class EFCoreDiagnosticsListener : IObserver<DiagnosticListener>
    {
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == "Microsoft.EntityFrameworkCore")
            {
                value.Subscribe(new EFCoreDiagnosticsObserver());
            }
        }
    }
}