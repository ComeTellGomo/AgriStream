namespace AgriStream.Models
{
    public class TelemetryReading
    {
        public string DeviceId { get; set; } = string.Empty;

        public string SensorReadings { get; set; } = string.Empty;

        public string IrrigationEvents { get; set; } = string.Empty;

        public string EnvironmentMonitoring { get; set; } = string.Empty;

        public DateTimeOffset RecordedAt { get; set; }
    }
}