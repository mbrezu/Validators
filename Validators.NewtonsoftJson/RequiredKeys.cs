using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.NewtonsoftJson
{
    class RequiredKeys : IValidator<JToken>
    {
        private readonly bool _ignoreCase;
        private readonly string[] _requiredKeys;

        public RequiredKeys(bool ignoreCase, string[] requiredKeys)
        {
            _ignoreCase = ignoreCase;
            _requiredKeys = requiredKeys;
        }

        public string ObjectName => $"object with required keys: [{string.Join(", ", _requiredKeys.Select(x => $"'{x}'"))}]";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (target is JObject targetObject)
            {
                var comparer = _ignoreCase
                    ? StringComparer.InvariantCultureIgnoreCase
                    : StringComparer.InvariantCulture;
                var kvs = targetObject as IEnumerable<KeyValuePair<string, JToken>>;
                var keys = kvs.Select(kv => kv.Key).ToList();
                foreach (var requiredKey in _requiredKeys)
                {
                    if (!keys.Contains(requiredKey, comparer))
                    {
                        yield return new DefaultValidationError { Message = $"Key '{requiredKey}' is missing." };
                    }
                }
            }
        }
    }
}
