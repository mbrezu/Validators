using Newtonsoft.Json.Linq;
using Validators.Core;

namespace Validators.Json.Newtonsoft
{
    sealed class TypeValidator : IValidator<JToken>
    {
        private readonly JTokenType[] _types;
        private readonly string _message;

        private TypeValidator(string message, string objectName, params JTokenType[] types)
        {
            _types = types;
            _message = message;
            ObjectName = objectName;
        }

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            if (!_types.Contains(target.Type))
            {
                yield return new DefaultValidationError { Message = _message };
            }
        }

        public string ObjectName { get; }

        private static readonly TypeValidator _numberInstance = new("Not a number.", "number", JTokenType.Float, JTokenType.Integer);
#pragma warning disable RCS1085 // Use auto-implemented property.
        public static TypeValidator Number => _numberInstance;

        private static readonly TypeValidator _stringInstance = new("Not a string.", "string", JTokenType.String);
        public static TypeValidator String => _stringInstance;

        private static readonly TypeValidator _boolInstance = new("Not a boolean.", "boolean", JTokenType.Boolean);
        public static TypeValidator Boolean => _boolInstance;

        private static readonly TypeValidator _nullInstance = new("Not 'null'.", "null", JTokenType.Null);
        public static TypeValidator Null => _nullInstance;

        private static readonly TypeValidator _objectInstance = new("Not an object.", "object", JTokenType.Object);

        public static TypeValidator Object => _objectInstance;
#pragma warning restore RCS1085 // Use auto-implemented property.
    }
}
