using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FisherTournament.Infrastracture.Persistence.Common.Diagnostics
{
    public class EFCoreDiagnosticsObserver : IObserver<KeyValuePair<string, object>>
    {
        private static ConcurrentDictionary<Guid, Activity> _activitiesCommand = new ConcurrentDictionary<Guid, Activity>();
        private static ConcurrentDictionary<Guid, Activity> _activitiesConnection = new ConcurrentDictionary<Guid, Activity>();

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            var activity = Activity.Current;

            if (activity == null)
            {
                return;
            }

            if (value.Key == RelationalEventId.ConnectionCreating.Name)
            {
                var payload = (ConnectionCreatingEventData)value.Value;

                var newActivity = activity.Source.StartActivity(payload?.Context?.GetType().Name ?? "CreatingConnection", ActivityKind.Client);

                if (newActivity == null || payload == null)
                {
                    return;
                }

                _activitiesConnection[payload.ConnectionId] = newActivity;
            }

            if (value.Key == RelationalEventId.ConnectionOpened.Name)
            {
                var payload = (ConnectionEndEventData)value.Value;

                if (_activitiesConnection.TryRemove(payload.ConnectionId, out var connectionActivity))
                {
                    var tags = new KeyValuePair<string, object?>[] {
                        new KeyValuePair<string, object?>("Duration ms", payload.Duration.Milliseconds),
                    };
                    connectionActivity.AddEvent(new ActivityEvent("ConnectionOpened", default, new ActivityTagsCollection(tags)));
                    connectionActivity.Stop();
                }
            }

            // CoreEventId
            if (value.Key == RelationalEventId.CommandCreated.Name)
            {
                var payload = (CommandEndEventData)value.Value;

                Activity? defaultActivity = null;

                if (_activitiesConnection.ContainsKey(payload.ConnectionId))
                {
                    defaultActivity = _activitiesConnection[payload.ConnectionId];
                }
                else
                {
                    defaultActivity = activity.Source.StartActivity(payload?.Context?.GetType().Name ?? "ExecutingQuery", ActivityKind.Client);
                }

                if (defaultActivity == null || payload == null)
                {
                    return;
                }

                _activitiesCommand[payload.CommandId] = defaultActivity;
            }


            if (value.Key == RelationalEventId.CommandExecuted.Name)
            {
                var payload = (CommandExecutedEventData)value.Value;

                if (_activitiesCommand.TryRemove(payload.CommandId, out var commandActivity))
                {
                    var tags = new KeyValuePair<string, object?>[] {
                        new KeyValuePair<string, object?>("Command", payload.Command.CommandText) ,
                        new KeyValuePair<string, object?>("Duration ms", payload.Duration.Milliseconds),
                    };
                    commandActivity.AddEvent(new ActivityEvent("CommandExecuted", default, new ActivityTagsCollection(tags)));
                    commandActivity.Stop();
                }
                // if (!activity.Source.Name.Contains("EntityFramework"))
                // {
                //     return;
                // }

                // var payload = (CommandExecutedEventData)value.Value;

                // var tags = new KeyValuePair<string, object?>[] {
                //     new KeyValuePair<string, object?>("Command", payload.Command.CommandText) ,
                //     new KeyValuePair<string, object?>("Duration ms", payload.Duration.Milliseconds),
                // };

                // Console.WriteLine("Current Activity Source: " + Activity.Current?.Source.Name);

                // Activity.Current?.AddEvent(new ActivityEvent(payload.Command.CommandText, DateTimeOffset.Now, new ActivityTagsCollection(tags)));
            }
        }
    }
}