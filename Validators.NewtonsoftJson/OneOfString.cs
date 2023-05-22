using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.NewtonsoftJson
{
    class OneOfString : IValidator<JToken>
    {
        private readonly string[] _options;

        public OneOfString(params string[] options)
        {
            _options = options;
        }

        public string ObjectName => $"one of ({string.Join(", ", _options.Select(x => $"\"{x}\""))})";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (!_options.Contains(target.Value<string>()))
            {
                yield return new DefaultValidationError
                {
                    Message = $"Not {ObjectName}."
                };
            }
        }
    }
}
