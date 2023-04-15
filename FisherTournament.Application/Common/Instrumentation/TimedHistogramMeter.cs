using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices;

namespace FisherTournament.Application.Common.Metrics
{
    /// <summary>
    /// A simple wrapper around the Histogram class that allows
    /// you to time a block of code and record the result in milliseconds.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TimedHistogramMeter<T> : IDisposable
    where T : struct
    {
        private readonly Histogram<T> _histogram;
        private readonly Stopwatch _stopwatch;

        private readonly List<KeyValuePair<string, object?>> _tags = new(); // TagList doesn't work  ¯\_(ツ)_/¯

        public TimedHistogramMeter(Histogram<T> histogram,
                                   params KeyValuePair<string, object?>[] tags)
        {
            _histogram = histogram;
            _stopwatch = Stopwatch.StartNew();
            _tags.AddRange(tags);
        }

        public void AddTag(string key, object? value)
        {
            _tags.Append(new KeyValuePair<string, object?>(key, value));
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _histogram.Record((T)Convert.ChangeType(_stopwatch.ElapsedMilliseconds, typeof(T)),
                              CollectionsMarshal.AsSpan(_tags));
        }
    }
}