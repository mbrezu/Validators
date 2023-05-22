using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.NewtonsoftJson
{
    class DiveInto : IValidator<JToken>
    {
        private readonly bool _ignoreCase;
        private readonly string _key;
        private readonly IValidator<JToken> _valueValidator;

        public DiveInto(bool ignoreCase, string key, IValidator<JToken> valueValidator)
        {
            _ignoreCase = ignoreCase;
            _key = key;
            _valueValidator = valueValidator;
        }

        public string ObjectName => $"value for '{_key}' satisfies {_valueValidator.ObjectName}.";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (target is JObject targetObject)
            {
                var comparison = _ignoreCase
                    ? StringComparison.InvariantCultureIgnoreCase
                    : StringComparison.InvariantCulture;
                if (!targetObject.TryGetValue(_key, comparison, out var value))
                {
                    yield break;
                }
                foreach (var error in _valueValidator.Validate(value))
                {
                    yield return error.Wrap(_key);
                }
            }
        }
    }
}
