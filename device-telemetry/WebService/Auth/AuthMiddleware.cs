﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.External;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Newtonsoft.Json;
using Microsoft.Azure.IoTSolutions.Auth;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService
{
    /// <summary>
    /// Validate every incoming request checking for a valid authorization header.
    /// The header must containg a valid JWT token. Other than the usual token
    /// validation, the middleware also restrict the allowed algorithms to block
    /// tokens created with a weak algorithm.
    /// Validations used:
    /// * The issuer must match the one in the configuration
    /// * The audience must match the one in the configuration
    /// * The token must not be expired, some configurable clock skew is allowed
    /// * Signature is required
    /// * Signature must be valid
    /// * Signature must be from the issuer
    /// * Signature must use one of the algorithms configured
    /// </summary>
    public class AuthMiddleware
    {
        // The authorization header carries a bearer token, with this prefix
        private const string AUTH_HEADER_PREFIX = "Bearer ";

        // Usual authorization header, carrying the bearer token
        private const string AUTH_HEADER = "Authorization";

        // User requests are marked with this header by the reverse proxy
        // TODO ~devis: this is a temporary solution for public preview only
        // TODO ~devis: remove this approach and use the service to service authentication
        // https://github.com/Azure/pcs-auth-dotnet/issues/18
        // https://github.com/Azure/azure-iot-pcs-remote-monitoring-dotnet/issues/11
        private const string EXT_RESOURCES_HEADER = "X-Source";

        private const string ERROR401 = @"{""Error"":""Authentication required""}";
        private const string ERROR503_AUTH = @"{""Error"":""Authentication service not available""}";

        private readonly RequestDelegate requestDelegate;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> openIdCfgMan;
        private readonly IClientAuthConfig config;
        private readonly ILogger log;
        private TokenValidationParameters tokenValidationParams;
        private readonly bool authRequired;     
        private bool tokenValidationInitialized;
        private readonly IUserManagementClient userManagementClient;
        private readonly IServicesConfig servicesConfig;

        public AuthMiddleware(
            // ReSharper disable once UnusedParameter.Local
            RequestDelegate requestDelegate, // Required by ASP.NET
            IConfigurationManager<OpenIdConnectConfiguration> openIdCfgMan,
            IClientAuthConfig config,
            IUserManagementClient userManagementClient,
            ILogger log,
            IServicesConfig servicesConfig)
        {
            this.requestDelegate = requestDelegate;
            this.openIdCfgMan = openIdCfgMan;
            this.config = config;
            this.log = log;
            this.authRequired = config.AuthRequired;
            this.tokenValidationInitialized = false;
            this.userManagementClient = userManagementClient;
            this.servicesConfig = servicesConfig;

            // This will show in development mode, or in case auth is turned off
            if (!this.authRequired)
            {
                this.log.Warn("### AUTHENTICATION IS DISABLED! ###", () => { });
                this.log.Warn("### AUTHENTICATION IS DISABLED! ###", () => { });
                this.log.Warn("### AUTHENTICATION IS DISABLED! ###", () => { });
            }
            else
            {
                this.log.Info("Auth config", () => new
                {
                    this.config.AuthType,
                    this.config.JwtIssuer,
                    this.config.JwtAudience,
                    this.config.JwtAllowedAlgos,
                    this.config.JwtClockSkew
                });

                this.InitializeTokenValidationAsync(CancellationToken.None).Wait();
            }

            // TODO ~devis: this is a temporary solution for public preview only
            // TODO ~devis: remove this approach and use the service to service authentication
            // https://github.com/Azure/pcs-auth-dotnet/issues/18
            // https://github.com/Azure/azure-iot-pcs-remote-monitoring-dotnet/issues/11
            this.log.Warn("### Service to service authentication is not available in public preview ###", () => { });
            this.log.Warn("### Service to service authentication is not available in public preview ###", () => { });
            this.log.Warn("### Service to service authentication is not available in public preview ###", () => { });
        }

        public Task Invoke(HttpContext context)
        {
            var header = string.Empty;
            var token = string.Empty;

            // Store this setting to skip validating authorization in the controller if enabled
            context.Request.SetAuthRequired(this.config.AuthRequired);

            if (!context.Request.Headers.ContainsKey(EXT_RESOURCES_HEADER) )
            {
                // This is a service to service request running in the private
                // network, so we skip the auth required for user requests
                // Note: this is a temporary solution for public preview
                // https://github.com/Azure/pcs-auth-dotnet/issues/18
                // https://github.com/Azure/azure-iot-pcs-remote-monitoring-dotnet/issues/11

                // Call the next delegate/middleware in the pipeline
                this.log.Debug("Skipping auth for service to service request", () => { });
                context.Request.SetExternalRequest(false);
                return this.requestDelegate(context);
            }

            context.Request.SetExternalRequest(true);

            if (!this.authRequired)
            {
                // Call the next delegate/middleware in the pipeline
                this.log.Debug("Skipping auth (auth disabled)", () => { });
                return this.requestDelegate(context);
            }

            if (!this.InitializeTokenValidationAsync(context.RequestAborted).Result)
            {
                context.Response.StatusCode = (int) HttpStatusCode.ServiceUnavailable;
                context.Response.Headers["Content-Type"] = "application/json";
                context.Response.WriteAsync(ERROR503_AUTH);
                return Task.CompletedTask;
            }

            if (context.Request.Headers.ContainsKey(AUTH_HEADER))
            {
                header = context.Request.Headers[AUTH_HEADER].SingleOrDefault();
            }
            else
            {
                this.log.Error("Authorization header not found", () => { });
            }

            if (header != null && header.StartsWith(AUTH_HEADER_PREFIX))
            {
                token = header.Substring(AUTH_HEADER_PREFIX.Length).Trim();
            }
            else
            {
                this.log.Error("Authorization header prefix not found", () => { });
            }

            if (this.ValidateToken(token, context) || !this.authRequired)
            {
                // Call the next delegate/middleware in the pipeline
                return this.requestDelegate(context);
            }

            this.log.Warn("Authentication required", () => { });
            context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
            context.Response.Headers["Content-Type"] = "application/json";
            context.Response.WriteAsync(ERROR401);

            return Task.CompletedTask;
        }

        private bool ValidateToken(string token, HttpContext context)
        {
            if (string.IsNullOrEmpty(token)) return false;

            try
            {
                SecurityToken validatedToken;
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, this.tokenValidationParams, out validatedToken);
                var jwtToken = new JwtSecurityToken(token);

                // Validate the signature algorithm
                if (this.config.JwtAllowedAlgos.Contains(jwtToken.SignatureAlgorithm))
                {
                    // Store the user info in the request context, so the authorization
                    // header doesn't need to be parse again later in the User controller.
                    context.Request.SetCurrentUserClaims(jwtToken.Claims);

                    // Store the user allowed actions in the request context to validate
                    // authorization later in the controller.
                    var userObjectId = context.Request.GetCurrentUserObjectId();
                    var roles = context.Request.GetCurrentUserRoleClaim().ToList();
                    List<string> allowedActions = new List<string>();
                    if (roles.Any())
                    {
                        foreach (string role in roles)
                        {
                            allowedActions.AddRange(this.servicesConfig.UserPermissions[role]);
                        }

                    }
                    else
                    {
                        this.log.Warn("JWT token doesn't include any role claims.", () => { });
                    }
                    // Add Allowed Actions
                    context.Request.SetCurrentUserAllowedActions(allowedActions);

                    //Set Tenant Information
                    context.Request.SetTenant();

                    return true;
                }

                this.log.Error("JWT token signature algorithm is not allowed.", () => new { jwtToken.SignatureAlgorithm });
            }
            catch (Exception e)
            {
                this.log.Error("Failed to validate JWT token", () => new { e });
            }

            return false;
        }

        private async Task<bool> InitializeTokenValidationAsync(CancellationToken token)
        {
            if (this.tokenValidationInitialized) return true;

            try
            {
                this.log.Info("Initializing OpenID configuration", () => { });
                var openIdConfig = await this.openIdCfgMan.GetConfigurationAsync(token);

                //Attempted to do it myself still issue with SSL
                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.config.JwtIssuer+ "/.well-known/openid-configuration/jwks");
                //request.AutomaticDecompression = DecompressionMethods.GZip;
                //IdentityKeys

                //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                //using (Stream stream = response.GetResponseStream())
                //using (StreamReader reader = new StreamReader(stream))
                //{
                //    keys = JsonConvert.DeserializeObject<IdentityGatewayKeys>(reader.ReadToEnd());
                //}

                this.tokenValidationParams = new TokenValidationParameters
                {
                    // Validate the token signature
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys,

                    // Validate the token issuer
                    ValidateIssuer = true,
                    ValidIssuer = this.config.JwtIssuer,

                    // Validate the token audience
                    ValidateAudience = false,
                    ValidAudience = this.config.JwtAudience,

                    // Validate token lifetime
                    ValidateLifetime = true,
                    ClockSkew = this.config.JwtClockSkew
                };

                this.tokenValidationInitialized = true;
            }
            catch (Exception e)
            {
                this.log.Error("Failed to setup OpenId Connect", () => new { e });
            }

            return this.tokenValidationInitialized;
        }
    }
}
