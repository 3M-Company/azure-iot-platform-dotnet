﻿// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.Exceptions;
using Mmm.Platform.IoT.Common.Services.External.TimeSeries;
using Mmm.Platform.IoT.Common.Services.Http;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Xunit;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.Test.TimeSeries
{
    public class TimeSeriesClientTest
    {
        private readonly Mock<ILogger<TimeSeriesClient>> _logger;
        private readonly Mock<IHttpClient> httpClient;
        private Mock<AppConfig> config;
        private TimeSeriesClient client;

        public TimeSeriesClientTest()
        {
            _logger = new Mock<ILogger<TimeSeriesClient>>();
            this.config = new Mock<AppConfig>();
            this.httpClient = new Mock<IHttpClient>();
            this.client = new TimeSeriesClient(
                this.httpClient.Object,
                this.config.Object,
                _logger.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task QueryThrowsInvalidConfiguration_WhenConfigValuesAreNull()
        {
            // Arrange 
            this.SetupClientWithNullConfigValues();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidConfigurationException>(() =>
                 this.client.QueryEventsAsync(null, null, "desc", 0, 1000, new string[0]));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task PingReturnsFalse_WhenConfigValuesAreNull()
        {
            // Arrange
            this.SetupClientWithNullConfigValues();

            // Act
            var result = await this.client.StatusAsync();

            // Assert
            Assert.False(result.IsHealthy);
            Assert.Contains("TimeSeries check failed", result.Message);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task QueryThrows_IfInvalidAuthParams()
        {
            // Arrange
            this.SetupClientWithConfigValues();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidConfigurationException>(() =>
                this.client.QueryEventsAsync(null, null, "desc", 0, 1000, new string[0]));
        }

        private void SetupClientWithNullConfigValues()
        {
            this.config = new Mock<AppConfig>();
            this.client = new TimeSeriesClient(
                this.httpClient.Object,
                this.config.Object,
                _logger.Object);
        }

        private void SetupClientWithConfigValues()
        {
            this.config.Setup(f => f.TelemetryService.TimeSeries.TsiDataAccessFqdn).Returns("test123");
            this.config.Setup(f => f.TelemetryService.TimeSeries.Audience).Returns("test123");
            this.config.Setup(f => f.TelemetryService.TimeSeries.ApiVersion).Returns("2016-12-12-test");
            this.config.Setup(f => f.TelemetryService.TimeSeries.Timeout).Returns("PT20S");
            this.config.Setup(f => f.Global.AzureActiveDirectory.AadTenantId).Returns("test123");
            this.config.Setup(f => f.Global.AzureActiveDirectory.AadAppId).Returns("test123");
            this.config.Setup(f => f.Global.AzureActiveDirectory.AadAppSecret).Returns("test123");
            this.config.Setup(f => f.TelemetryService.TimeSeries.Authority).Returns("https://login.testing.net/");

            this.client = new TimeSeriesClient(
                this.httpClient.Object,
                this.config.Object,
                _logger.Object);
        }
    }
}
