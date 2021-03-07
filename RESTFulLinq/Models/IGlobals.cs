// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System.Collections.Generic;

namespace RESTFulLinq.Models
{
    public interface IGlobals<T>
    {
        IEnumerable<T> DataSource { get; set; }
    }
}
