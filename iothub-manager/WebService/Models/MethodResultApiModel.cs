// <copyright file="MethodResultApiModel.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using Mmm.Platform.IoT.IoTHubManager.Services.Models;

namespace Mmm.Platform.IoT.IoTHubManager.WebService.Models
{
    public class MethodResultApiModel : MethodResultServiceModel
    {
        public MethodResultApiModel(MethodResultServiceModel model)
        {
            this.Status = model.Status;
            this.JsonPayload = model.JsonPayload;
        }
    }
}