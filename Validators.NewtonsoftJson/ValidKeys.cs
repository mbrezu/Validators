using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.NewtonsoftJson
{
    class ValidKeys : IValidator<JToken>
    {
        private readonly bool _ignoreCase;
        private readonly string[] _validKeys;

        public ValidKeys(bool ignoreCase, string[] validKeys)
        {
            _ignoreCase = ignoreCase;
            _validKeys = validKeys;
        }

        public string ObjectName => $"object with valid keys: [{string.Join(", ", _validKeys.Select(x => $"'{x}'"))}]";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (target is JObject targetObject)
            {
                var comparer = _ignoreCase
                    ? StringComparer.InvariantCultureIgnoreCase
                    : StringComparer.InvariantCulture;
                foreach (var (k, _) in targetObject)
                {
                    if (!_validKeys.Contains(k, comparer))
                    {
                        yield return new DefaultValidationError { Message = $"Key '{k}' is not valid." };
                    }
                }
            }
            else
            {
                yield return new DefaultValidationError { Message = "Not an object." };
            }
        }
    }
}
