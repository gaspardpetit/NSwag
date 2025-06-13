using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

namespace NSwag.CodeGeneration.CSharp
{
    /// <summary>
    /// Generates property names without modifying their casing.
    /// </summary>
    public sealed class OriginalCSharpPropertyNameGenerator : IPropertyNameGenerator
    {
        private const string FirstPassChars = "\"'@?!$[]().=+|";
#if NET8_0_OR_GREATER
        private static readonly System.Buffers.SearchValues<char> _reservedFirstPassChars = System.Buffers.SearchValues.Create(FirstPassChars);
#else
        private static readonly char[] _reservedFirstPassChars = FirstPassChars.ToCharArray();
#endif

        private const string SecondPassChars = "*:-#&";
#if NET8_0_OR_GREATER
        private static readonly System.Buffers.SearchValues<char> _reservedSecondPassChars = System.Buffers.SearchValues.Create(SecondPassChars);
#else
        private static readonly char[] _reservedSecondPassChars = SecondPassChars.ToCharArray();
#endif

        /// <summary>
        /// Returns the property name unchanged except for removing invalid characters.
        /// </summary>
        /// <param name="property">The JSON schema property.</param>
        /// <returns>The generated name.</returns>
        public string Generate(JsonSchemaProperty property)
        {
            var name = property.Name;

            if (name.AsSpan().IndexOfAny(_reservedFirstPassChars) != -1)
            {
                name = name.Replace("\"", string.Empty)
                    .Replace("'", string.Empty)
                    .Replace("@", string.Empty)
                    .Replace("?", string.Empty)
                    .Replace("!", string.Empty)
                    .Replace("$", string.Empty)
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                    .Replace("(", "_")
                    .Replace(")", string.Empty)
                    .Replace(".", "_")
                    .Replace("=", "_")
                    .Replace("+", "plus")
                    .Replace("|", "_");
            }

            if (name.AsSpan().IndexOfAny(_reservedSecondPassChars) != -1)
            {
                name = name
                    .Replace("*", "Star")
                    .Replace(":", "_")
                    .Replace("-", "_")
                    .Replace("#", "_")
                    .Replace("&", "And");
            }

            return name;
        }
    }
}
