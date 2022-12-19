using System.Text.RegularExpressions;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using PMetrium.Native.Common.Contracts;
using PMetrium.Native.Common.Helpers;
using PMetrium.Native.IOS.Contracts;
using Serilog;

namespace PMetrium.Native.IOS.MetricsHandlers;

public class ApplicationMetricsHandler
{
    private InfluxDBSync _influxDbSync;
    private IOSDeviceContext _deviceContext;
    private IOSPerformanceResults _iosPerformanceResults;

    public ApplicationMetricsHandler(
        IOSDeviceContext deviceContext,
        InfluxDBSync influxDbSync,
        IOSPerformanceResults iosPerformanceResults)
    {
        _influxDbSync = influxDbSync;
        _deviceContext = deviceContext;
        _iosPerformanceResults = iosPerformanceResults;
    }

    public async Task ExtractAndSaveMetrics(CancellationToken token)
    {
        var device = _deviceContext.DeviceParameters.Device;

        try
        {
            var logs = _deviceContext.EventsLogs;

            if (logs.Count == 0)
                return;

            Log.Information($"[IOS: {device}] ApplicationMetricsHandler - start to handle application events");

            logs = logs
                .Where(x => x.Contains("[PMETRIUM_NATIVE]"))
                .Select(x => Regex.Replace(x, "^.*\\[PMETRIUM_NATIVE\\]", "")).ToList();
            _iosPerformanceResults.Events = logs;
            var normalizedEvents = NormalizeEvents(logs);

            Log.Debug(
                $"[IOS: {device}] ApplicationMetricsHandler - events: {JsonConvert.SerializeObject(normalizedEvents, Formatting.Indented)}");

            foreach (var @event in normalizedEvents)
            {
                await _influxDbSync.SaveAnnotationToInfluxDBAsync(
                    "ios.annotations",
                    "[ AppEvent ]",
                    new Dictionary<string, string>(),
                    _deviceContext.CommonTags,
                    @event.Timestamp,
                    @event.Name,
                    $"Timestamp: {@event.Timestamp.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds}");

                @event.Name = @event.Name.Replace("[START]", "").Replace("[END]", "").Trim();
            }

            var points = new List<PointData>();
            normalizedEvents = normalizedEvents.Where(x => x.IsStart || x.IsEnd).ToList();

            while (normalizedEvents.Count >= 2)
            {
                var eventName = normalizedEvents[0].Name;
                var matchedEvents = normalizedEvents
                    .Where(x => x.Name == normalizedEvents[0].Name)
                    .OrderBy(x => x.Timestamp)
                    .ToList();

                RemoveWrongEventsAtTheBeginning(matchedEvents);

                while (matchedEvents.Count >= 2)
                {
                    if (matchedEvents[0].IsStart && matchedEvents[1].IsEnd)
                    {
                        var eventTags =
                            _deviceContext.CommonTags.ToDictionary(entry => entry.Key, entry => entry.Value);
                        eventTags.Add("event", matchedEvents[0].Name);

                        var latency = (matchedEvents[1].Timestamp - matchedEvents[0].Timestamp).TotalMilliseconds;
                        points.Add(_influxDbSync.GeneratePoint(
                            "ios.events.app",
                            eventTags,
                            matchedEvents[0].Timestamp,
                            latency,
                            "latency"));

                        _iosPerformanceResults.ComplexEvents.Add(new ComplexEvent
                        {
                            Timestamp = matchedEvents[0].Timestamp,
                            Name = matchedEvents[0].Name,
                            Latency = latency
                        });

                        matchedEvents.RemoveRange(0, 2);

                        continue;
                    }

                    var itemsToRemove = 2;
                    var startItemsToRemove = 2;
                    var endItemsToRemove = 0;

                    for (int i = 2; i < matchedEvents.Count; i++)
                    {
                        if (startItemsToRemove == endItemsToRemove)
                            break;

                        if (matchedEvents[i].IsStart)
                            startItemsToRemove++;

                        if (matchedEvents[i].IsEnd)
                            endItemsToRemove++;

                        itemsToRemove++;
                    }

                    for (int i = 0; i < itemsToRemove; i++)
                    {
                        LogWrongEventOrder(matchedEvents[0]);
                        matchedEvents.RemoveAt(0);
                    }

                    RemoveWrongEventsAtTheBeginning(matchedEvents);
                }

                foreach (var matchedEvent in matchedEvents)
                    LogWrongEventOrder(matchedEvent);

                normalizedEvents.RemoveAll(x => x.Name == eventName);
            }

            await _influxDbSync.SavePoints(points.ToArray());

            Log.Information($"[IOS: {device}] ApplicationMetricsHandler - events have been saved");
        }
        catch (Exception e)
        {
            Log.Error($"[IOS: {device}] ApplicationMetricsHandler - failed to parse events. StackTrace: " +
                      $"\n {e.Message} \n {e.StackTrace}");
        }
    }

    private void RemoveWrongEventsAtTheBeginning(List<IOSEvent> eventsList)
    {
        var itemsToRemove = -1;
        for (int i = 0; i < eventsList.Count; i++)
        {
            if (eventsList[i].IsEnd)
            {
                LogWrongEventOrder(eventsList[i]);

                itemsToRemove = i + 1;
                continue;
            }

            break;
        }

        for (int i = 0; i < itemsToRemove; i++)
            eventsList.RemoveAt(0);
    }

    private void LogWrongEventOrder(IOSEvent iosEvent)
    {
        Log.Warning(
            $"[IOS: {_deviceContext.DeviceParameters.Device}] Event: " +
            $"[Start - {iosEvent.IsStart}] " +
            $"[END - {iosEvent.IsEnd}] " +
            $"'{iosEvent.Name}' {iosEvent.Timestamp.ToString("o")} - in the wrong order");
    }

    private class IOSEvent
    {
        public bool IsStart { get; set; }
        public bool IsEnd { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
    }

    private List<IOSEvent> NormalizeEvents(List<string> logs)
    {
        var device = _deviceContext.DeviceParameters.Device;
        var list = new List<IOSEvent>();

        foreach (var log in logs)
        {
            try
            {
                var @event = new IOSEvent();

                if (log.Contains("[START]"))
                    @event.IsStart = true;

                if (log.Contains("[END]"))
                    @event.IsEnd = true;

                var timestamp = Regex.Match(log, "\\d+$").Value;
                @event.Timestamp = DateTime.UnixEpoch.AddMilliseconds(long.Parse(timestamp));
                @event.Name = log.Replace(timestamp, "").Trim();

                list.Add(@event);
            }
            catch (Exception e)
            {
                Log.Error($"[IOS: {device}] ApplicationMetricsHandler - failed to parse event: {log}. StackTrace: " +
                          $"\n {e.Message} \n {e.StackTrace}");
            }
        }

        return list;
    }
}