// <copyright file="Theme.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

namespace Mmm.Platform.IoT.Config.Services.Models
{
    public class Theme
    {
        public static readonly Theme Default = new Theme
        {
            Name = "My Solution",
            Description = "My Solution Description",
        };

        public string Name { get; private set; }

        public string Description { get; private set; }
    }
}