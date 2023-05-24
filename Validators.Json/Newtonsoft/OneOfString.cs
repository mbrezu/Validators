using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.Json.Newtonsoft
{
    class OneOfString : IValidator<JToken>
    {
        private readonly bool _ignoreCase;
        private readonly string[] _options;

        public OneOfString(bool ignoreCase, params string[] options)
        {
            _ignoreCase = ignoreCase;
            _options = options;
        }

        public string ObjectName => $"one of ({string.Join(", ", _options.Select(x => $"\"{x}\""))})";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            var comparer = _ignoreCase
                ? StringComparer.InvariantCultureIgnoreCase
                : StringComparer.InvariantCulture;
            if (!_options.Contains(target.Value<string>(), comparer))
            {
                yield return new DefaultValidationError
                {
                    Message = $"Not {ObjectName}."
                };
            }
        }
    }
}
