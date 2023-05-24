using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.Json.Newtonsoft
{
    class ArrayOf : IValidator<JToken>
    {
        private readonly IValidator<JToken> _elementValidator;
        private readonly int? _minCount;
        private readonly int? _maxCount;

        public ArrayOf(IValidator<JToken> elementValidator, int? minCount, int? maxCount)
        {
            _elementValidator = elementValidator;
            _minCount = minCount;
            _maxCount = maxCount;
        }

        public string ObjectName => $"array of {_elementValidator.ObjectName}";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (target is JArray array)
            {
                if (_minCount.HasValue && array.Count < _minCount.Value)
                {
                    yield return new DefaultValidationError
                    {
                        Message = $"Array count is {array.Count}, but should be at least {_minCount.Value}."
                    };
                }
                if (_maxCount.HasValue && array.Count > _maxCount.Value)
                {
                    yield return new DefaultValidationError
                    {
                        Message = $"Array count is {array.Count}, but should be at most {_maxCount.Value}."
                    };
                }
                for (int i = 0; i < array.Count; i++)
                {
                    foreach (var error in _elementValidator.Validate(array[i]))
                    {
                        yield return error.Wrap(i.ToString());
                    }
                }
            }
            else
            {
                yield return new DefaultValidationError { Message = "Not an array." };
            }
        }
    }
}
