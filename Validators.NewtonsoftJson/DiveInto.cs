using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.NewtonsoftJson
{
    class DiveInto : IValidator<JToken>
    {
        private readonly string _key;
        private readonly IValidator<JToken> _valueValidator;

        public DiveInto(string key, IValidator<JToken> valueValidator)
        {
            _key = key;
            _valueValidator = valueValidator;
        }

        public string ObjectName => $"value for '{_key}' satisfies {_valueValidator.ObjectName}.";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (target is JObject targetObject)
            {
                if (!targetObject.ContainsKey(_key))
                {
                    yield break;
                }
                var value = targetObject.GetValue(_key)!;
                foreach (var error in _valueValidator.Validate(value))
                {
                    yield return error.Wrap(_key);
                }
            }
        }
    }
}
