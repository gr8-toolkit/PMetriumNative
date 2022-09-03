using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace PMetrium.Native.Metrics.Android.Implementation.Helpers
{
    public class InfluxDBSync
    {
        private InfluxDBClient _influxdbClient;
        private Dictionary<string, string> _globalTags = new();

        public InfluxDBSync(string url, string userName, string password, string databaseName)
        {
            _influxdbClient =
                InfluxDBClientFactory.CreateV1(url, userName, password.ToCharArray(), databaseName, "autogen");
        }

        public Dictionary<string, string> GlobalMetricTags
        {
            get => _globalTags;
            set => _globalTags = value;
        }

        public async Task SaveAnnotationToInfluxDBAsync(
            string measurement,
            string annotationTitle,
            Dictionary<string, string> annotationTags,
            Dictionary<string, string> commonTags,
            string text = "")
        {
            string tags = "";

            foreach (var tag in annotationTags)
                tags += $"{tag.Key}: {tag.Value},";

            if (tags.Length > 0)
                tags = tags.Substring(0, tags.Length - 1);

            var point = PointData.Measurement(measurement)
                .Tag("title", annotationTitle)
                .Tag("tags", tags)
                .Field("text", text);

            point = AddTags(point, commonTags);

            await _influxdbClient.GetWriteApiAsync().WritePointAsync(point);
        }

        public async Task SaveAnnotationToInfluxDBAsync(
            string measurement,
            string annotationTitle,
            Dictionary<string, string> annotationTags,
            Dictionary<string, string> commonTags,
            DateTime timestamp,
            string text = "")
        {
            string tags = "";

            foreach (var tag in annotationTags)
                tags += $"{tag.Key}: {tag.Value},";

            if (tags.Length > 0)
                tags = tags.Substring(0, tags.Length - 1);

            var point = PointData.Measurement(measurement)
                .Tag("title", annotationTitle)
                .Tag("tags", tags)
                .Field("text", text);

            point = AddTags(point, commonTags);
            point = point.Timestamp(timestamp, WritePrecision.Ms);

            await _influxdbClient.GetWriteApiAsync().WritePointAsync(point);
        }

        public async Task SaveValueToInfluxDBAsync(
            string measurement,
            Dictionary<string, string> customTags,
            string fieldKey,
            double fieldValue)
        {
            var point = PointData.Measurement(measurement)
                .Field(fieldKey, fieldValue);
            point = AddTags(point, customTags);

            await _influxdbClient.GetWriteApiAsync().WritePointAsync(point);
        }

        public async Task SaveValueToInfluxDBAsync(
            string measurement,
            Dictionary<string, string> customTags,
            string fieldKey,
            double fieldValue,
            DateTime timestamp)
        {
            var point = PointData.Measurement(measurement)
                .Field(fieldKey, fieldValue);
            point = AddTags(point, customTags);
            point = point.Timestamp(timestamp, WritePrecision.Ms);

            await _influxdbClient.GetWriteApiAsync().WritePointAsync(point);
        }

        private PointData AddTags(PointData point, Dictionary<string, string> commonTags)
        {
            foreach (var tag in _globalTags)
                point = point.Tag(tag.Key, tag.Value);

            foreach (var tag in commonTags)
                point = point.Tag(tag.Key, tag.Value);

            return point;
        }

        public async Task SavePoints(PointData[] points)
        {
            List<PointData> batchPoints = new List<PointData>();

            foreach (var point in points)
            {
                if (batchPoints.Count == 100)
                {
                    await _influxdbClient.GetWriteApiAsync().WritePointsAsync(batchPoints);
                    batchPoints = new List<PointData>();
                }

                batchPoints.Add(point);
            }

            if (batchPoints.Count > 0)
                await _influxdbClient.GetWriteApiAsync().WritePointsAsync(batchPoints);
        }

        public PointData GeneratePoint(
            string measurement,
            Dictionary<string, string> commonTags,
            double fieldValue,
            string fieldKey = "value")
        {
            var point = PointData.Measurement(measurement);
            point = AddTags(point, commonTags);
            point = point.Field(fieldKey, fieldValue);

            return point;
        }

        public PointData GeneratePoint(
            string measurement,
            Dictionary<string, string> commonTags,
            DateTime timestamp,
            double fieldValue,
            string fieldKey = "value")
        {
            var point = PointData.Measurement(measurement);
            point = AddTags(point, commonTags);
            point = point.Field(fieldKey, fieldValue);
            point = point.Timestamp(timestamp, WritePrecision.Ms);

            return point;
        }
    }
}
