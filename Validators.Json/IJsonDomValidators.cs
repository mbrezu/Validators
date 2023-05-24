using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Validators.Core;

namespace Validators.Json
{
    public interface IJsonDomValidators<T> where T : class
    {
        abstract static IValidator<T> IsAnything { get; }
        abstract static IValidator<T> IsNumber { get; }
        abstract static IValidator<T> IsString { get; }
        abstract static IValidator<JToken> IsOneOf(bool ignoreCase, params string[] options);
        abstract static IValidator<JToken> IsOneOf(params string[] options);
        abstract static IValidator<JToken> MatchesRegex(Regex regex);
        abstract static IValidator<JToken> CustomValidator(string name, Func<JToken, bool> validator);
        abstract static IValidator<JToken> IsBoolean { get; }
        abstract static IValidator<JToken> IsNull { get; }
        abstract static IValidator<JToken> IsObject { get; }
        abstract static IValidator<JToken> IsArrayOf(
            IValidator<JToken> elementValidator, int? minCount = null, int? maxCount = null);
        abstract static IValidator<JToken> IsDictionaryOf(
            IValidator<JToken> elementValidator, int? minCount = null, int? maxCount = null);
        abstract static IValidator<JToken> HasKey(bool ignoreCase, string key);
        abstract static IValidator<JToken> HasKey(string key);
        abstract static IValidator<JToken> DiveInto(
            bool ignoreCase, string key, IValidator<JToken> valueValidator);
        abstract static IValidator<JToken> DiveInto(string key, IValidator<JToken> valueValidator);
        abstract static IValidator<JToken> HasValidKeys(bool ignoreCase, params string[] validKeys);
        abstract static IValidator<JToken> HasValidKeys(params string[] validKeys);
        abstract static IValidator<JToken> HasRequiredKeys(bool ignoreCase, params string[] requiredKeys);
        abstract static IValidator<JToken> HasRequiredKeys(params string[] requiredKeys);
        abstract static IValidator<JToken> And(params IValidator<JToken>[] children);
        abstract static IValidator<JToken> Or(params IValidator<JToken>[] children);
        abstract static IValidator<JToken> DelayedValidator(Func<IValidator<JToken>> validatorProvider);
    }
}
