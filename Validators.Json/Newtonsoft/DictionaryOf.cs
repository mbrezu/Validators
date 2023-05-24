using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.Json.Newtonsoft
{
    class DictionaryOf : IValidator<JToken>
    {
        private readonly IValidator<JToken> _elementValidator;
        private readonly int? _minCount;
        private readonly int? _maxCount;

        public DictionaryOf(IValidator<JToken> elementValidator, int? minCount, int? maxCount)
        {
            _elementValidator = elementValidator;
            _minCount = minCount;
            _maxCount = maxCount;
        }

        public string ObjectName => $"dictionary of (string, {_elementValidator.ObjectName})";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (target is JObject obj)
            {
                if (_minCount.HasValue && obj.Count < _minCount.Value)
                {
                    yield return new DefaultValidationError
                    {
                        Message = $"Object property count is {obj.Count}, but should be at least {_minCount.Value}."
                    };
                }
                if (_maxCount.HasValue && obj.Count > _maxCount.Value)
                {
                    yield return new DefaultValidationError
                    {
                        Message = $"Object property count is {obj.Count}, but should be at most {_maxCount.Value}."
                    };
                }
                foreach (var kv in obj as IEnumerable<KeyValuePair<string, JToken>>)
                {
                    foreach (var error in _elementValidator.Validate(kv.Value))
                    {
                        yield return error.Wrap(kv.Key);
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
