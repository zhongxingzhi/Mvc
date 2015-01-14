﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class FormFileModelBinder : IModelBinder
    {
        public async Task<bool> BindModelAsync([NotNull] ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType == typeof(IFormFile))
            {
                var postedFiles = await GetFormFilesAsync(bindingContext);
                bindingContext.Model = postedFiles.FirstOrDefault();

                return true;
            }
            else if (typeof(IEnumerable<IFormFile>).GetTypeInfo().IsAssignableFrom(
                    bindingContext.ModelType.GetTypeInfo()))
            {
                var postedFiles = await GetFormFilesAsync(bindingContext);
                bindingContext.Model = ModelBindingHelper.ConvertValuesToCollectionType(bindingContext.ModelType, postedFiles);

                return true;
            }
            return false;
        }

        private async Task<List<IFormFile>> GetFormFilesAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.OperationBindingContext.HttpContext.Request;
            var postedFiles = new List<IFormFile>();
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();

                foreach (var file in form.Files)
                {
                    if (string.IsNullOrEmpty(file.ContentDisposition))
                    {
                        continue;
                    }

                    var parsedContentDisposition = file.ParseContentDisposition();
                    var modelName = parsedContentDisposition.Key;
                    if (modelName.Equals(bindingContext.ModelName, StringComparison.OrdinalIgnoreCase))
                    {
                        postedFiles.Add(file);
                    }
                }
            }

            return postedFiles;
        }
    }
}