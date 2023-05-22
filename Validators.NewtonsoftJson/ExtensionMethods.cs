using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.NewtonsoftJson
{
    public static class ExtensionMethods
    {
        public static JToken? ExtractInvalidContent(this IValidationError error, JToken? content)
        {
            static JToken? extract(JToken? content, IEnumerable<string> path)
            {
                if (!path.Any())
                {
                    return content;
                }
                else if (content is JObject obj && obj.ContainsKey(path.First()))
                {
                    return extract(content[path.First()], path.Skip(1));
                }
                else if (content is JArray arr && int.TryParse(path.First(), out var index))
                {
                    return extract(content[index], path.Skip(1));
                }
                else
                {
                    return content;
                }
            }
            return extract(content, error.Path);
        }
    }
}
