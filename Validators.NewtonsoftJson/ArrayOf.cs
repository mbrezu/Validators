using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.NewtonsoftJson
{
    class ArrayOf : IValidator<JToken>
    {
        private readonly IValidator<JToken> _elementValidator;

        public ArrayOf(IValidator<JToken> elementValidator)
        {
            _elementValidator = elementValidator;
        }

        public string ObjectName => $"array of {_elementValidator.ObjectName}";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (target is JArray array)
            {
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
