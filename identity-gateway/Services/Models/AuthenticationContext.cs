﻿using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Mmm.Platform.IoT.Common.Services.Config;
using System.Threading.Tasks;

namespace Mmm.Platform.IoT.IdentityGateway.Services.Models
{
    public class AuthenticationContext : IAuthenticationContext
    {
        private readonly Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext authContext;

        public AuthenticationContext(AppConfig config)
        {
            this.authContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext($"https://login.microsoftonline.com/{config.Global.AzureActiveDirectory.TenantId}");
        }
        public Task<AuthenticationResult> AcquireTokenAsync(string resource, ClientCredential clientCredential)
        {
            return authContext.AcquireTokenAsync(resource, clientCredential);
        }
    }
}