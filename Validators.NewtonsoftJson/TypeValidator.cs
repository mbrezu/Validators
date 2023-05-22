using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.NewtonsoftJson
{
    class TypeValidator : IValidator<JToken>
    {
        private readonly JTokenType[] _types;
        private readonly string _message;
        private readonly string _objectName;

        private TypeValidator(string message, string objectName, params JTokenType[] types)
        {
            _types = types;
            _message = message;
            _objectName = objectName;
        }

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (!_types.Contains(target.Type))
            {
                yield return new DefaultValidationError { Message = _message };
            }
        }

        public string ObjectName => _objectName;

        private static TypeValidator _numberInstance = new("Not a number.", "number", JTokenType.Float, JTokenType.Integer);
        public static TypeValidator Number => _numberInstance;

        private static TypeValidator _stringInstance = new("Not a string.", "string", JTokenType.String);
        public static TypeValidator String => _stringInstance;

        private static TypeValidator _boolInstance = new("Not a boolean.", "boolean", JTokenType.Boolean);
        public static TypeValidator Boolean => _boolInstance;

        private static TypeValidator _nullInstance = new("Not 'null'.", "null", JTokenType.Null);
        public static TypeValidator Null => _nullInstance;

        private static TypeValidator _objectInstance = new("Not a string.", "string", JTokenType.Object);
        public static TypeValidator Object => _objectInstance;
    }
}
