// <copyright file="IActions.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Mmm.Platform.IoT.Config.Services.Models.Actions;

namespace Mmm.Platform.IoT.Config.Services
{
    public interface IActions
    {
        Task<List<IActionSettings>> GetListAsync();
    }
}