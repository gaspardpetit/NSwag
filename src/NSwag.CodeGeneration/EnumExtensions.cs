using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema;

namespace NSwag.CodeGeneration
{
    internal static class EnumExtensions
    {
        public static void RemoveCaseInsensitiveEnumDuplicates(this OpenApiDocument document)
        {
            if (document == null)
            {
                return;
            }

            var visited = new HashSet<JsonSchema>();
            foreach (var schema in document.Components.Schemas.Values)
            {
                RemoveCaseInsensitiveEnumDuplicates(schema, visited);
            }

            foreach (var path in document.Paths.Values)
            {
                foreach (var operation in path.ActualPathItem.ActualOperations.Values)
                {
                    foreach (var parameter in operation.ActualParameters)
                    {
                        RemoveCaseInsensitiveEnumDuplicates(parameter.ActualSchema, visited);
                    }

                    var requestBody = operation.ActualRequestBody;
                    if (requestBody?.Content != null)
                    {
                        foreach (var content in requestBody.Content.Values)
                        {
                            RemoveCaseInsensitiveEnumDuplicates(content.Schema, visited);
                        }
                    }

                    foreach (var response in operation.ActualResponses)
                    {
                        if (response.Value.Content != null)
                        {
                            foreach (var content in response.Value.Content.Values)
                            {
                                RemoveCaseInsensitiveEnumDuplicates(content.Schema, visited);
                            }
                        }
                    }
                }
            }
        }

        private static void RemoveCaseInsensitiveEnumDuplicates(JsonSchema schema, HashSet<JsonSchema> visited)
        {
            if (schema == null || !visited.Add(schema))
            {
                return;
            }

            schema = schema.ActualSchema;
            if (schema.Enumeration?.Count > 1 && schema.Enumeration.All(e => e is string))
            {
                var unique = new List<object>();
                var uniqueNames = new List<string?>();
                var comparer = StringComparer.OrdinalIgnoreCase;
                for (int i = 0; i < schema.Enumeration.Count; i++)
                {
                    var value = (string)schema.Enumeration[i];
                    if (!unique.Cast<string>().Any(u => comparer.Equals(u, value)))
                    {
                        unique.Add(value);
                        if (schema.EnumerationNames?.Count > i)
                        {
                            uniqueNames.Add(schema.EnumerationNames[i]);
                        }
                    }
                }

                if (unique.Count != schema.Enumeration.Count)
                {
                    schema.Enumeration.Clear();
                    foreach (var item in unique)
                    {
                        schema.Enumeration.Add(item);
                    }

                    if (schema.EnumerationNames != null)
                    {
                        schema.EnumerationNames.Clear();
                        foreach (var name in uniqueNames)
                        {
                            schema.EnumerationNames.Add(name);
                        }
                    }
                }
            }

            foreach (var property in schema.ActualProperties.Values)
            {
                RemoveCaseInsensitiveEnumDuplicates(property.ActualSchema, visited);
            }

            if (schema.Item != null)
            {
                RemoveCaseInsensitiveEnumDuplicates(schema.Item, visited);
            }

            if (schema.Items != null)
            {
                foreach (var item in schema.Items)
                {
                    RemoveCaseInsensitiveEnumDuplicates(item, visited);
                }
            }

            if (schema.AdditionalPropertiesSchema != null)
            {
                RemoveCaseInsensitiveEnumDuplicates(schema.AdditionalPropertiesSchema, visited);
            }
        }
    }
}
