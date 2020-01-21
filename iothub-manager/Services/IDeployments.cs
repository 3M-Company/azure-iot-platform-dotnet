// <copyright file="IDeployments.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Mmm.Platform.IoT.IoTHubManager.Services.Models;

namespace Mmm.Platform.IoT.IoTHubManager.Services
{
    public interface IDeployments
    {
        Task<DeploymentServiceModel> CreateAsync(DeploymentServiceModel model);

        Task<DeploymentServiceListModel> ListAsync();

        Task<DeploymentServiceModel> GetAsync(string id, bool includeDeviceStatus);

        Task DeleteAsync(string deploymentId);
    }
}