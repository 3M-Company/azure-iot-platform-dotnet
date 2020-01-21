// <copyright file="ClientAuthConfig.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

namespace Mmm.Platform.IoT.Common.Services.Config
{
    public partial class ClientAuthConfig
    {
        public bool CorsEnabled => !string.IsNullOrEmpty(CorsWhitelist?.Trim());
    }
}