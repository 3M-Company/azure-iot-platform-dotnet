// -----------------------------------------------------------------------
// <copyright file="BlankFileContentException.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Mmm.Platform.IoT.AsaManager.Services.Exceptions
{
    public class BlankFileContentException : Exception
    {
        public BlankFileContentException()
            : base()
        {
        }

        public BlankFileContentException(string message)
            : base(message)
        {
        }

        public BlankFileContentException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}