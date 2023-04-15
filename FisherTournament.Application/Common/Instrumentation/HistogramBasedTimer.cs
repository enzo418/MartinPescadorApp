using System;
using System.Diagnostics.Metrics;

namespace FisherTournament.Application.Common.Metrics
{
    public record Tag(string Key, object? Value);

    public class HistogramBasedTimer
    {
        private readonly Histogram<long> _histogram;

        public HistogramBasedTimer(Meter meter, string name, string? unit = null, string? description = null)
        {
            _histogram = meter.CreateHistogram<long>(
                name, unit, description
            );
        }

        public TimedHistogramMeter<long> Time(params KeyValuePair<string, object?>[] tags)
        {
            return new TimedHistogramMeter<long>(_histogram, tags);
        }

        public TimedHistogramMeter<long> Time(Tag tag)
        {
            return new TimedHistogramMeter<long>(_histogram,
                                                 new KeyValuePair<string, object?>[] { new(tag.Key, tag.Value) });
        }

        public TimedHistogramMeter<long> Time(Tag tag1, Tag tag2)
        {
            return new TimedHistogramMeter<long>(_histogram,
                                                 new KeyValuePair<string, object?>[] { new(tag1.Key, tag1.Value), new(tag2.Key, tag2.Value) });
        }
    }
}