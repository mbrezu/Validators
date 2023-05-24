using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Validators.Core;
using static Validators.NewtonsoftJson.Json;

namespace Validators.NewtonsoftJson
{
    public record ValidationSchema(TypeValidation Root, IEnumerable<TypeValidation> AllTypes)
    {
        public IValidator<JToken> GetValidator(bool ignoreCase = true)
        {
            var validators = new Dictionary<string, IValidator<JToken>>();
            foreach (var type in AllTypes)
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
            var allTypes = new List<TypeValidation>();
            var stack = new Stack<string>();
            ValidatorSpec getValidatorSpec(Type type, Type parentType, string name)
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
                    return new OneOfSpec(options!.EnumIgnoreCase, Enum.GetNames(type));
                }
                else if (type.IsDictionaryOfString())
                {
                    var elementType = type.GetDictionaryOfStringValueType();
                    return new DictionarySpec(
                        getValidatorSpec(elementType, typeof(IDictionary<,>), ""),
                        options!.GetMinCount(parentType, name),
                        options!.GetMaxCount(parentType, name));
                }
                else if (type.IsArray())
                {
                    var elementType = type.GetArrayElementType();
                    return new ArraySpec(
                        getValidatorSpec(elementType, typeof(IEnumerable<>), ""),
                        options!.GetMinCount(parentType, name),
                        options!.GetMaxCount(parentType, name));
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return getValidatorSpec(type.GetGenericArguments().First(), parentType, name);
                }
                else
                {
                    var typeName = type.GetFullNameOrName();
                    if (typeName != "System.Object" && !stack!.Contains(typeName))
                    {
                        var fromOthers = allTypes!.Find(x => x.Name == typeName);
                        if (fromOthers == null)
                        {
                            allTypes!.Add(fromType(type));
                        }
                    }
                    return new TypeSpec(typeName);
                }
            }
            TypeValidation fromType(Type type)
            {
                stack!.Push(type.GetFullNameOrName());
                try
                {
                    var properties = type.GetProperties();
                    var fieldValidations = properties
                        .Select(x => new FieldValidation(
                            x.Name,
                            options!.IsRequired(x, type, x.Name, x.PropertyType),
                            getValidatorSpec(x.PropertyType, type, x.Name)))
                        .ToList();
                    return new TypeValidation(
                        type.GetFullNameOrName(),
                        options!.AllowExtras,
                        fieldValidations);
                }
                finally
                {
                    stack.Pop();
                }
            }
            var typeValidation = fromType(type);
            allTypes.Add(typeValidation);
            return new ValidationSchema(typeValidation, allTypes);
        }
    }

    public record TypeAndProperty(Type Type, string Property)
    {
        public static TypeAndProperty From<T>(Expression<Func<T, object?>> selector)
        {
            var expression = selector.Body as UnaryExpression;
            var operand = expression?.Operand as MemberExpression
                ?? selector.Body as MemberExpression;
            var name = operand?.Member?.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"Invalid selector expression: {selector}", nameof(selector));
            }
            return new TypeAndProperty(typeof(T), name);
        }
    }

    public record ValidationSchemaOptions(
        bool EnumIgnoreCase,
        bool AllowExtras)
    {
        private ImmutableList<TypeAndProperty> _required = ImmutableList<TypeAndProperty>.Empty;
        private ImmutableList<TypeAndProperty> _optional = ImmutableList<TypeAndProperty>.Empty;
        private ImmutableDictionary<TypeAndProperty, int> _minCounts = ImmutableDictionary<TypeAndProperty, int>.Empty;
        private ImmutableDictionary<TypeAndProperty, int> _maxCounts = ImmutableDictionary<TypeAndProperty, int>.Empty;

        private static readonly ValidationSchemaOptions _empty = new(true, true);
#pragma warning disable RCS1085 // Use auto-implemented property.
        public static ValidationSchemaOptions Empty => _empty;
#pragma warning restore RCS1085 // Use auto-implemented property.

        public ValidationSchemaOptions SetEnumIgnoreCase(bool value)
            => this with { EnumIgnoreCase = value };

        public ValidationSchemaOptions SetAllowExtras(bool value)
            => this with { AllowExtras = value };

        public ValidationSchemaOptions SetRequired<T>(Expression<Func<T, object?>> selector)
            => this with
            {
                _required = _required.Add(TypeAndProperty.From(selector))
            };

        public ValidationSchemaOptions SetOptional<T>(Expression<Func<T, object?>> selector)
            => this with
            {
                _optional = _optional.Add(TypeAndProperty.From(selector))
            };

        public ValidationSchemaOptions SetMinCount<T>(Expression<Func<T, object?>> selector, int minSize)
            => this with
            {
                _minCounts = _minCounts.Add(TypeAndProperty.From(selector), minSize)
            };

        public ValidationSchemaOptions SetMaxCount<T>(Expression<Func<T, object?>> selector, int maxSize)
            => this with
            {
                _maxCounts = _maxCounts.Add(TypeAndProperty.From(selector), maxSize)
            };

        public int? GetMinCount(Type type, string propertyName)
        {
            return _minCounts.TryGetValue(new TypeAndProperty(type, propertyName), out int result)
                ? result
                : null;
        }

        public int? GetMaxCount(Type type, string propertyName)
        {
            return _maxCounts.TryGetValue(new TypeAndProperty(type, propertyName), out int result)
                ? result
                : null;
        }

        private readonly static NullabilityInfoContext _nullabilityInfoContext = new();

        private NullabilityInfo GetNullabilityInfo(PropertyInfo propertyInfo)
        {
            lock (_nullabilityInfoContext)
            {
                return _nullabilityInfoContext.Create(propertyInfo);
            }
        }

        public bool IsRequired(PropertyInfo propertyInfo, Type type, string propertyName, Type propertyType)
        {
            bool isMentioned(IEnumerable<TypeAndProperty>? list)
                => list?.Any(x => x.Type == type && x.Property == propertyName) == true;
            if (isMentioned(_required))
            {
                return true;
            }
            if (isMentioned(_optional))
            {
                return false;
            }
            var isNullable = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (!isNullable)
            {
                if (propertyInfo.CustomAttributes.Any(x => x.GetType().Name == "NullableAttribute"))
                {
                    return false;
                }
                else if (GetNullabilityInfo(propertyInfo).WriteState == NullabilityState.Nullable)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
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
        Array,
        Dictionary
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
                : DelayedValidator(() => existingValidators.ContainsKey(TypeName)
                    ? existingValidators[TypeName]
                    : IsAnything);
    }

    public record ArraySpec(ValidatorSpec ElementValidator, int? MinCount, int? MaxCount)
        : ValidatorSpec(ValidatorSpecKind.Array)
    {
        public override IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => IsArrayOf(ElementValidator.GetValidator(existingValidators), MinCount, MaxCount);
    }

    public record DictionarySpec(ValidatorSpec ElementValidator, int? MinCount, int? MaxCount)
    : ValidatorSpec(ValidatorSpecKind.Array)
    {
        public override IValidator<JToken> GetValidator(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => IsDictionaryOf(ElementValidator.GetValidator(existingValidators), MinCount, MaxCount);
    }
}
