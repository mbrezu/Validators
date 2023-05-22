using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.NewtonsoftJson
{
    class CustomValidator : IValidator<JToken>
    {
        private readonly Func<JToken, bool> _validator;

        public CustomValidator(string name, Func<JToken, bool> validator)
        {
            ObjectName = name;
            _validator = validator;
        }

        public string ObjectName { get; }

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (!_validator(target))
            {
                yield return new DefaultValidationError
                {
                    Message = $"Not {ObjectName}."
                };
            }
        }
    }
}
