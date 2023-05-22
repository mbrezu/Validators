using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.NewtonsoftJson
{
    class HasKey : IValidator<JToken>
    {
        private readonly string _key;

        public HasKey(string key)
        {
            _key = key;
        }

        public string ObjectName => $"has property {_key}";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (target is JObject targetObject)
            {
                if (!targetObject.ContainsKey(_key))
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
