// <copyright file="IUserInput.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

namespace Mmm.Platform.IoT.IdentityGateway.Services
{
    public interface IUserInput<TModel>
    {
        string UserId { get; set; }
    }
}