// <copyright file="ConversionApiModel.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;

namespace Mmm.Platform.IoT.AsaManager.Services.Models
{
    public class ConversionApiModel
    {
        public ConversionApiModel()
        {
        }

        public string BlobFilePath { get; set; }

        public string TenantId { get; set; }

        public string OperationId { get; set; }

        public ValueListApiModel Entities { get; set; }
    }
}