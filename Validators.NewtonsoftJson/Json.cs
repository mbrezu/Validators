using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Validators.Core;

namespace Validators.NewtonsoftJson
{
    public static class Json
    {
        public static IValidator<JToken> IsNumber => TypeValidator.Number;
        public static IValidator<JToken> IsString => TypeValidator.String;
        public static IValidator<JToken> IsOneOf(params string[] options) => new OneOfString(options);
        public static IValidator<JToken> MatchesRegex(Regex regex) => new MatchesRegex(regex);
        public static IValidator<JToken> CustomValidator(string name, Func<JToken, bool> validator)
            => new CustomValidator(name, validator);
        public static IValidator<JToken> IsBoolean => TypeValidator.Boolean;
        public static IValidator<JToken> IsNull => TypeValidator.Null;
        public static IValidator<JToken> IsObject => TypeValidator.Object;
        public static IValidator<JToken> IsArrayOf(
            IValidator<JToken> elementValidator, int? minCount = null, int? maxCount = null)
            => new ArrayOf(elementValidator, minCount, maxCount);
        public static IValidator<JToken> HasKey(string key) => new HasKey(key);
        public static IValidator<JToken> DiveInto(string key, IValidator<JToken> valueValidator)
            => new DiveInto(key, valueValidator);
        public static IValidator<JToken> HasValidKeys(bool ignoreCase, params string[] validKeys)
            => new ValidKeys(ignoreCase, validKeys);
        public static IValidator<JToken> HasValidKeys(params string[] validKeys)
            => new ValidKeys(true, validKeys);
        public static IValidator<JToken> HasRequiredKeys(bool ignoreCase, params string[] requiredKeys)
            => new RequiredKeys(ignoreCase, requiredKeys);
        public static IValidator<JToken> HasRequiredKeys(params string[] requiredKeys)
            => new RequiredKeys(true, requiredKeys);
        public static IValidator<JToken> And(params IValidator<JToken>[] children) => new And<JToken>(children);
        public static IValidator<JToken> Or(params IValidator<JToken>[] children) => new Or<JToken>(children);
    }
}
