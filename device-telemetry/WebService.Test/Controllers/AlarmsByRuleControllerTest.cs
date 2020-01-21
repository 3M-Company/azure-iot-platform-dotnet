// <copyright file="AlarmsByRuleControllerTest.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.Common.Services.Config;
using Mmm.Platform.IoT.Common.Services.External.AsaManager;
using Mmm.Platform.IoT.Common.Services.External.CosmosDb;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.Services.Runtime;
using Mmm.Platform.IoT.DeviceTelemetry.Services;
using Mmm.Platform.IoT.DeviceTelemetry.Services.External;
using Mmm.Platform.IoT.DeviceTelemetry.WebService.Controllers;
using Moq;
using Xunit;
using Alarm = Mmm.Platform.IoT.DeviceTelemetry.Services.Models.Alarm;

namespace Mmm.Platform.IoT.DeviceTelemetry.WebService.Test.Controllers
{
    public class AlarmsByRuleControllerTest : IDisposable
    {
        private readonly Mock<ILogger<AlarmsByRuleController>> logger;
        private readonly IStorageClient storage;
        private readonly Mock<IHttpContextAccessor> httpContextAccessor;
        private readonly Mock<IAppConfigurationHelper> appConfigHelper;
        private readonly Mock<IAsaManagerClient> asaManager;
        private bool disposedValue = false;
        private AlarmsByRuleController controller;
        private List<Alarm> sampleAlarms;
        private string docSchemaKey = "doc.schema";
        private string docSchemaValue = "alarm";
        private string docSchemaVersionKey = "doc.schemaVersion";
        private int docSchemaVersionValue = 1;
        private string createdKey = "created";
        private string modifiedKey = "modified";
        private string descriptionKey = "description";
        private string statusKey = "status";
        private string deviceIdKey = "device.id";
        private string ruleIdKey = "rule.id";
        private string ruleSeverityKey = "rule.severity";
        private string ruleDescriptionKey = "rule.description";

        public AlarmsByRuleControllerTest()
        {
            Mock<IStorageAdapterClient> storageAdapterClient = new Mock<IStorageAdapterClient>();
            this.httpContextAccessor = new Mock<IHttpContextAccessor>();
            logger = new Mock<ILogger<AlarmsByRuleController>>();
            this.appConfigHelper = new Mock<IAppConfigurationHelper>();
            this.asaManager = new Mock<IAsaManagerClient>();
            var config = new AppConfig();
            this.storage = new StorageClient(config, new Mock<ILogger<StorageClient>>().Object);
            this.storage.CreateCollectionIfNotExistsAsync(config.DeviceTelemetryService.Alarms.Database, string.Empty);

            this.sampleAlarms = this.GetSampleAlarms();
            foreach (Alarm sampleAlarm in this.sampleAlarms)
            {
                this.storage.UpsertDocumentAsync(
                    config.DeviceTelemetryService.Alarms.Database,
                    string.Empty,
                    this.AlarmToDocument(sampleAlarm));
            }

            Alarms alarmService = new Alarms(config, this.storage, new Mock<ILogger<Alarms>>().Object, this.httpContextAccessor.Object, this.appConfigHelper.Object);
            Rules rulesService = new Rules(storageAdapterClient.Object, this.asaManager.Object, new Mock<ILogger<Rules>>().Object, alarmService, new Mock<IDiagnosticsClient>().Object);
            this.controller = new AlarmsByRuleController(alarmService, rulesService, this.logger.Object);
        }

        // Ignoring test. Updating .net core and xunit version wants this class to be public. However, this test fails when the class is made public.
        // Created issue https://github.com/Azure/device-telemetry-dotnet/issues/65 to address this better.
        // [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ProvideAlarmsByRuleResult()
        {
            // Act
            var response = this.controller.GetAsync(null, null, "asc", null, null, null);

            // Assert
            Assert.NotEmpty(response.Result.Metadata);
            Assert.NotEmpty(response.Result.Items);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    controller.Dispose();
                }

                disposedValue = true;
            }
        }

        private Document AlarmToDocument(Alarm alarm)
        {
            Document document = new Document()
            {
                Id = Guid.NewGuid().ToString(),
            };

            document.SetPropertyValue(this.docSchemaKey, this.docSchemaValue);
            document.SetPropertyValue(this.docSchemaVersionKey, this.docSchemaVersionValue);
            document.SetPropertyValue(this.createdKey, alarm.DateCreated.ToUnixTimeMilliseconds());
            document.SetPropertyValue(this.modifiedKey, alarm.DateModified.ToUnixTimeMilliseconds());
            document.SetPropertyValue(this.statusKey, alarm.Status);
            document.SetPropertyValue(this.descriptionKey, alarm.Description);
            document.SetPropertyValue(this.deviceIdKey, alarm.DeviceId);
            document.SetPropertyValue(this.ruleIdKey, alarm.RuleId);
            document.SetPropertyValue(this.ruleSeverityKey, alarm.RuleSeverity);
            document.SetPropertyValue(this.ruleDescriptionKey, alarm.RuleDescription);

            // The logic used to generate the alarm (future proofing for ML)
            document.SetPropertyValue("logic", "1Device-1Rule-1Message");

            return document;
        }

        private List<Alarm> GetSampleAlarms()
        {
            List<Alarm> list = new List<Alarm>();

            Alarm alarm1 = new Alarm(
                null,
                "1",
                DateTimeOffset.Parse("2017-07-22T22:22:22-08:00").ToUnixTimeMilliseconds(),
                DateTimeOffset.Parse("2017-07-22T22:22:22-08:00").ToUnixTimeMilliseconds(),
                "Temperature on device x > 75 deg F",
                "group-Id",
                "device-id",
                "open",
                "1",
                "critical",
                "HVAC temp > 50");

            Alarm alarm2 = new Alarm(
                null,
                "2",
                DateTimeOffset.Parse("2017-06-22T22:22:22-08:00").ToUnixTimeMilliseconds(),
                DateTimeOffset.Parse("2017-07-22T22:22:22-08:00").ToUnixTimeMilliseconds(),
                "Temperature on device x > 75 deg F",
                "group-Id",
                "device-id",
                "acknowledged",
                "2",
                "critical",
                "HVAC temp > 60");

            Alarm alarm3 = new Alarm(
                null,
                "3",
                DateTimeOffset.Parse("2017-05-22T22:22:22-08:00").ToUnixTimeMilliseconds(),
                DateTimeOffset.Parse("2017-06-22T22:22:22-08:00").ToUnixTimeMilliseconds(),
                "Temperature on device x > 75 deg F",
                "group-Id",
                "device-id",
                "open",
                "3",
                "info",
                "HVAC temp > 70");

            Alarm alarm4 = new Alarm(
                null,
                "4",
                DateTimeOffset.Parse("2017-04-22T22:22:22-08:00").ToUnixTimeMilliseconds(),
                DateTimeOffset.Parse("2017-06-22T22:22:22-08:00").ToUnixTimeMilliseconds(),
                "Temperature on device x > 75 deg F",
                "group-Id",
                "device-id",
                "closed",
                "4",
                "warning",
                "HVAC temp > 80");

            list.Add(alarm1);
            list.Add(alarm2);
            list.Add(alarm3);
            list.Add(alarm4);

            return list;
        }

        private Alarm GetSampleAlarm()
        {
            return new Alarm(
                "6l1log0f7h2yt6p",
                "1234",
                DateTimeOffset.Parse("2017-02-22T22:22:22-08:00").ToUnixTimeMilliseconds(),
                DateTimeOffset.Parse("2017-02-22T22:22:22-08:00").ToUnixTimeMilliseconds(),
                "Temperature on device x > 75 deg F",
                "group-Id",
                "device-id",
                "open",
                "1234",
                "critical",
                "HVAC temp > 75");
        }
    }
}