using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.Json.Newtonsoft
{
    class HasKey : IValidator<JToken>
    {
        private readonly bool _ignoreCase;
        private readonly string _key;

        public HasKey(bool ignoreCase, string key)
        {
            _ignoreCase = ignoreCase;
            _key = key;
        }

        public string ObjectName => $"has property {_key}";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (target is JObject targetObject)
            {
                var comparison = _ignoreCase
                    ? StringComparison.InvariantCultureIgnoreCase
                    : StringComparison.InvariantCulture;
                if (!targetObject.TryGetValue(_key, comparison, out _))
                {
                    yield return new DefaultValidationError { Message = $"Doesn't have key '{_key}'." };
                }
            }
            else
            {
                yield return new DefaultValidationError { Message = "Not an object." };
            }
        }
    }
}
