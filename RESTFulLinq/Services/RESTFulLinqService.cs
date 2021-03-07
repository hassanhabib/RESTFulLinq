// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using RESTFulLinq.Models;

namespace RESTFulLinq
{
    public class RESTFulLinqService
    {
        public static async ValueTask<object> RunQueryAsync<T>(string linQuery, IGlobals<T> globals)
        {
            ScriptOptions scriptOptions = ScriptOptions.Default;
            scriptOptions = scriptOptions.AddReferences("System");
            scriptOptions = scriptOptions.AddReferences("System.Linq");
            scriptOptions = scriptOptions.AddReferences("System.Collections.Generic");

            var state = await CSharpScript.RunAsync($@"
                using System;
                using System.Linq;
                using System.Collections.Generic;
                
                return DataSource.{linQuery ?? "AsQueryable()"};", scriptOptions, globals);

            return state.ReturnValue;
        }
    }
}
