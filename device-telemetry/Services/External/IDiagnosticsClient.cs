// <copyright file="IDiagnosticsClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;

namespace Mmm.Platform.IoT.DeviceTelemetry.Services.External
{
    public interface IDiagnosticsClient : IStatusOperation
    {
        bool CanLogToDiagnostics { get; }

        Task LogEventAsync(string eventName);

        Task LogEventAsync(string eventName, Dictionary<string, object> eventProperties);
    }
}