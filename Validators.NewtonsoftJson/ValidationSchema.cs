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
            foreach (var type in Others)
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

        public static ValidationSchema FromType(
            Type type, ValidationSchemaOptions? options = null)
        {
            if (options == null)
            {
                options = ValidationSchemaOptions.Empty;
            }
            var others = new List<TypeValidation>();
            string getTypeName(Type type) => type.FullName ?? type.Name;
            Type? getFirstIDictionary(Type type)
                => type.GetInterfaces().FirstOrDefault(
                    x => x.IsGenericType
                    && x.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    && x.GetGenericArguments().First() == typeof(string));
            bool isDictionary(Type type) => getFirstIDictionary(type) != null;
            Type? getFirstIEnumerable(Type type)
                => type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            bool isArray(Type type) => getFirstIEnumerable(type) != null;
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
                    return new OneOfSpec(options!.IgnoreCase, Enum.GetNames(type));
                }
                else if (isDictionary(type))
                {
                    var elementType = getFirstIDictionary(type)!.GetGenericArguments().ElementAt(1);
                    return new ArraySpec(getValidatorSpec(elementType));
                }
                else if (isArray(type))
                {
                    var elementType = getFirstIEnumerable(type)!.GetGenericArguments().First();
                    return new ArraySpec(getValidatorSpec(elementType));
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return getValidatorSpec(type.GetGenericArguments().First());
                }
                else
                {
                    var name = getTypeName(type);
                    if (name != "System.Object")
                    {
                        var fromOthers = others!.Find(x => x.Name == name);
                        if (fromOthers == null)
                        {
                            others!.Add(fromType(type));
                        }
                    }
                    return new TypeSpec(name);
                }
            }
            TypeValidation fromType(Type type)
            {
                var properties = type.GetProperties();
                var fieldValidations = properties
                    .Select(x => new FieldValidation(
                        x.Name,
                        options!.IsRequired(getTypeName(type), x.Name, x.PropertyType),
                        getValidatorSpec(x.PropertyType)))
                    .ToList();
                return new TypeValidation(
                    getTypeName(type),
                    options!.AllowExtras,
                    fieldValidations);
            }
            return new ValidationSchema(fromType(type), others);
        }
    }

    public record TypeAndProperty(string Type, string Property);

    public record ValidationSchemaOptions(
        bool IgnoreCase,
        bool AllowExtras,
        IEnumerable<TypeAndProperty>? Required = null,
        IEnumerable<TypeAndProperty>? Optional = null)
    {
        private static readonly ValidationSchemaOptions _empty = new(true, true);
#pragma warning disable RCS1085 // Use auto-implemented property.
        public static ValidationSchemaOptions Empty => _empty;
#pragma warning restore RCS1085 // Use auto-implemented property.

        public bool IsRequired(string typeName, string propertyName, Type propertyType)
        {
            bool isMentioned(IEnumerable<TypeAndProperty>? list)
                => list?.Any(x => x.Type == typeName && x.Property == propertyName) == true;
            if (isMentioned(Required))
            {
                return true;
            }
            if (isMentioned(Optional))
            {
                return false;
            }
            var isNullable = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
            return !isNullable;
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
            => existingValidators.ContainsKey(TypeName)
            ? existingValidators[TypeName]
            : IsAnything;
    }

    public record ArraySpec(ValidatorSpec ElementValidator)
        : ValidatorSpec(ValidatorSpecKind.Array)
    {
        public override IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => IsArrayOf(ElementValidator.GetValidator(existingValidators));
    }
}
