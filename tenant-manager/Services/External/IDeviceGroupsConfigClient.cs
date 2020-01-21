// <copyright file="IDeviceGroupsConfigClient.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Mmm.Platform.IoT.Common.Services;
using Mmm.Platform.IoT.TenantManager.Services.Models;

namespace Mmm.Platform.IoT.TenantManager.Services.External
{
    public interface IDeviceGroupsConfigClient : IStatusOperation
    {
        Task<DeviceGroupApiModel> CreateDefaultDeviceGroupAsync(string tenantId);
    }
}