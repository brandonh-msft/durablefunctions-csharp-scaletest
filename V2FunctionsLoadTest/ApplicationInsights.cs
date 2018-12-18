using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace V2FunctionsLoadTest
{
    static class ApplicationInsights
    {
        private static readonly string Key = TelemetryConfiguration.Active.InstrumentationKey =
            Environment.GetEnvironmentVariable(
                "APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);

        public static TelemetryClient CurrentClient { get; } = new TelemetryClient() { InstrumentationKey = Key };
    }
}
