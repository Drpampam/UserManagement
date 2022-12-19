using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Core.Services
{
    public class FileUploadOperation : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.OperationId == "MyOperation")
            {
                operation.Parameters.Clear();
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "uploadedFile",
                    In = ParameterLocation.Header,
                    Description = "Upload File",
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "file",
                        Format = "binary"
                    }
                });
                var uploadFileMediaType = new OpenApiMediaType()
                {
                    Schema = new OpenApiSchema()
                    {
                        Type = "object",
                        Properties =
                    {
                        ["uploadedFile"] = new OpenApiSchema()
                        {
                            Description = "Upload File",
                            Type = "file",
                            Format = "binary"
                        }
                    },
                        Required = new HashSet<string>()
                        {
                            "uploadedFile"
                        }
                    }
                };
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        ["multipart/form-data"] = uploadFileMediaType
                    }
                };
            }
        }  
    }
}
