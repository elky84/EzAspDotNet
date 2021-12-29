using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EzAspDotNet.Swagger;

namespace EzAspDotNet.Swagger
{
    public class SwaggerExcludeFilter : ISchemaFilter
    {
        #region ISchemaFilter Members

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null || null == context.Type)
                return;

            var excludedProperties = context.Type.GetProperties()
                                         .Where(t =>
                                                t.GetCustomAttribute<SwaggerExcludeAttribute>()
                                                != null);

            foreach (var excludedProperty in excludedProperties)
            {
                if (schema.Properties.ContainsKey(excludedProperty.Name))
                    schema.Properties.Remove(excludedProperty.Name);
            }
        }

        #endregion
    }
}
