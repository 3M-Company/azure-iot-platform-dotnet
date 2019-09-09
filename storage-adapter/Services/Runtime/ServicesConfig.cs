﻿// Copyright (c) Microsoft. All rights reserved.
using System;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Helpers;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Runtime
{
    public interface IServicesConfig
    {
        bool AuthRequired { get; set; }
        string StorageType { get; set; }
        string DocumentDbDatabase { get; set; }
        string DocumentDbConnString { get; set; }
        int DocumentDbRUs { get; set; }
        IAppConfigurationHelper AppConfig { get; set; }

        string DocumentDbCollection(string tenant, string dataType);
    }

    public class ServicesConfig : IServicesConfig
    {
        public bool AuthRequired { get; set; }
        public string StorageType { get; set; }
        public string DocumentDbDatabase { get; set; }
        public string DocumentDbConnString { get; set; }
        public int DocumentDbRUs { get; set; }
        public string UserManagementApiUrl { get; set; }
        public IAppConfigurationHelper AppConfig { get; set; }

        /// <summary>
        /// Use the object initializer pattern to set the object's attributes
        /// bool AuthRequire
        /// string StorageType
        /// string DocumentDbConnStringKey
        /// int DocumentDbRUs
        /// string UserManagementApiUrl
        /// IConfigurationRoot AppConfig
        /// </summary>
        public ServicesConfig() { }

        public string DocumentDbCollection(string tenant, string dataType)
        {
            if (String.IsNullOrEmpty(tenant))
            {
                throw new NullReferenceException("The given tenant value was null. Ensure that your request has attached an ApplicationTenantId in the headers.");
            }
            return this.AppConfig.GetValue($"tenant:{tenant}:{dataType}-collection");
        }
    }
}
