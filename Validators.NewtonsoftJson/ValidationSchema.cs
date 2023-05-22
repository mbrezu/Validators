using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Validators.Core;
using static Validators.NewtonsoftJson.Json;

namespace Validators.NewtonsoftJson
{
    public record ValidationSchema(TypeValidation Root, IEnumerable<TypeValidation> Others)
    {
        public IValidator<JToken> GetValidator(bool ignoreCase = true)
        {
            var validators = new Dictionary<string, IValidator<JToken>>();
            foreach (var type in Others.Reverse())
            {
                validators[type.Name] = type.GetValidator(ignoreCase, validators);
            }
            return Root.GetValidator(ignoreCase, validators);
        }

        private static readonly Type[] numberTypes = new Type[]
        {
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(nint),
            typeof(nuint),
            typeof(double),
            typeof(float),
            typeof(decimal)
        };

        public static ValidationSchema FromType(Type type, bool ignoreCase = true, bool allowExtras = true)
        {
            var others = new List<TypeValidation>();
            string getTypeName(Type type) => type.FullName ?? type.Name;
            bool isArray(Type type)
                => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            ValidatorSpec getValidatorSpec(Type type)
            {
                if (numberTypes.Contains(type))
                {
                    return new NumberSpec();
                }
                else if (type == typeof(string) || type == typeof(DateTime) || type == typeof(TimeSpan))
                {
                    return new StringSpec();
                }
                else if (type == typeof(bool))
                {
                    return new BooleanSpec();
                }
                else if (type.IsEnum)
                {
                    return new OneOfSpec(ignoreCase, Enum.GetNames(type));
                }
                else if (isArray(type))
                {
                    var elementType = type.GetGenericArguments().First();
                    return new ArraySpec(getValidatorSpec(elementType));
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return getValidatorSpec(type.GetGenericArguments().First());
                }
                else
                {
                    var name = getTypeName(type);
                    var fromOthers = others!.Find(x => x.Name == name);
                    if (fromOthers == null)
                    {
                        others!.Add(fromType(type));
                    }
                    return new TypeSpec(name);
                }
            }
            bool isRequired(Type type)
            {
                var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
                return !isNullable;
            }
            TypeValidation fromType(Type type)
            {
                var properties = type.GetProperties();
                var fieldValidations = properties
                    .Select(x => new FieldValidation(x.Name, isRequired(x.PropertyType), getValidatorSpec(x.PropertyType)))
                    .ToList();
                return new TypeValidation(
                    getTypeName(type),
                    allowExtras,
                    fieldValidations);
            }
            return new ValidationSchema(fromType(type), others);
        }
    }

    public record TypeValidation(string Name, bool AllowExtras, IEnumerable<FieldValidation> Fields)
    {
        public IValidator<JToken> GetValidator(bool ignoreCase, Dictionary<string, IValidator<JToken>> existingValidators)
        {
            var validators = new List<IValidator<JToken>>
            {
                IsObject
            };
            if (!AllowExtras)
            {
                validators.Add(HasValidKeys(
                    ignoreCase,
                    Fields.Select(x => x.Name).ToArray()));
            }
            validators.Add(HasRequiredKeys(
                ignoreCase,
                Fields.Where(x => x.IsRequired).Select(x => x.Name).ToArray()));
            foreach (var field in Fields)
            {
                validators.Add(DiveInto(
                    ignoreCase, field.Name, field.ValidatorSpec.GetValidator(existingValidators)));
            }
            return And(validators.ToArray());
        }
    }

    public record FieldValidation(
        string Name, bool IsRequired, ValidatorSpec ValidatorSpec);

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ValidatorSpecKind
    {
        Number,
        String,
        OneOf,
        Regex,
        Boolean,
        Null,
        Type,
        Array
    }

    public abstract record ValidatorSpec(ValidatorSpecKind Kind)
    {
        public abstract IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators);
    }
    public record NumberSpec() : ValidatorSpec(ValidatorSpecKind.Number)
    {
        public override IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators) => IsNumber;
    }

    public record StringSpec() : ValidatorSpec(ValidatorSpecKind.String)
    {
        public override IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators) => IsString;
    }

    public record OneOfSpec(bool IgnoreCase, IEnumerable<string> Alternatives)
        : ValidatorSpec(ValidatorSpecKind.OneOf)
    {
        public override IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => IsOneOf(IgnoreCase, Alternatives.ToArray());
    }

    public record RegexSpec(string Regex)
        : ValidatorSpec(ValidatorSpecKind.Regex)
    {
        public override IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => MatchesRegex(new Regex(Regex));
    }

    public record BooleanSpec()
        : ValidatorSpec(ValidatorSpecKind.Boolean)
    {
        public override IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators) => IsBoolean;
    }

    public record NullSpec() : ValidatorSpec(ValidatorSpecKind.Null)
    {
        public override IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators) => IsNull;
    }

    public record TypeSpec(string TypeName)
        : ValidatorSpec(ValidatorSpecKind.Type)
    {
        public override IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => existingValidators[TypeName];
    }

    public record ArraySpec(ValidatorSpec ElementValidator)
        : ValidatorSpec(ValidatorSpecKind.Array)
    {
        public override IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => IsArrayOf(ElementValidator.GetValidator(existingValidators));
    }
}
