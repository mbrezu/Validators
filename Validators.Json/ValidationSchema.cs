using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Validators.Core;

namespace Validators.Json
{
    public record ValidationSchema<T>(TypeValidation<T> Root, IEnumerable<TypeValidation<T>> AllTypes)
        where T : IJsonDomValidators<JToken>
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

        public static ValidationSchema<T> FromType(
            Type type, ValidationSchemaOptions? options = null)
        {
            if (options == null)
            {
                options = ValidationSchemaOptions.Empty;
            }
            var allTypes = new List<TypeValidation<T>>();
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
            TypeValidation<T> fromType(Type type)
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
                    return new TypeValidation<T>(
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
            return new ValidationSchema<T>(typeValidation, allTypes);
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

        private static NullabilityInfo GetNullabilityInfo(PropertyInfo propertyInfo)
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

    public record TypeValidation<T>(string Name, bool AllowExtras, IEnumerable<FieldValidation> Fields)
        where T : IJsonDomValidators<JToken>
    {
        public IValidator<JToken> GetValidator(bool ignoreCase, Dictionary<string, IValidator<JToken>> existingValidators)
        {
            var validators = new List<IValidator<JToken>>
            {
                T.IsObject
            };
            if (!AllowExtras)
            {
                validators.Add(T.HasValidKeys(
                    ignoreCase,
                    Fields.Select(x => x.Name).ToArray()));
            }
            validators.Add(T.HasRequiredKeys(
                ignoreCase,
                Fields.Where(x => x.IsRequired).Select(x => x.Name).ToArray()));
            foreach (var field in Fields)
            {
                validators.Add(T.DiveInto(
                    ignoreCase, field.Name, field.ValidatorSpec.GetValidator<T>(existingValidators)));
            }
            return T.And(validators.ToArray());
        }
    }

    public record FieldValidation(
        string Name, bool IsRequired, ValidatorSpec ValidatorSpec);

    record TypeAndProperty(Type Type, string Property)
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
        public abstract IValidator<JToken> GetValidator<T>(
            Dictionary<string, IValidator<JToken>> existingValidators)
            where T : IJsonDomValidators<JToken>;
    }
    record NumberSpec() : ValidatorSpec(ValidatorSpecKind.Number)
    {
        public override IValidator<JToken> GetValidator<T>(
            Dictionary<string, IValidator<JToken>> existingValidators) => T.IsNumber;
    }

    record StringSpec() : ValidatorSpec(ValidatorSpecKind.String)
    {
        public override IValidator<JToken> GetValidator<T>(
            Dictionary<string, IValidator<JToken>> existingValidators) => T.IsString;
    }

    record OneOfSpec(bool IgnoreCase, IEnumerable<string> Alternatives)
        : ValidatorSpec(ValidatorSpecKind.OneOf)
    {
        public override IValidator<JToken> GetValidator<T>(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => T.IsOneOf(IgnoreCase, Alternatives.ToArray());
    }

    record RegexSpec(string Regex)
        : ValidatorSpec(ValidatorSpecKind.Regex)
    {
        public override IValidator<JToken> GetValidator<T>(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => T.MatchesRegex(new Regex(Regex));
    }

    record BooleanSpec()
        : ValidatorSpec(ValidatorSpecKind.Boolean)
    {
        public override IValidator<JToken> GetValidator<T>(
            Dictionary<string, IValidator<JToken>> existingValidators) => T.IsBoolean;
    }

    record NullSpec() : ValidatorSpec(ValidatorSpecKind.Null)
    {
        public override IValidator<JToken> GetValidator<T>(
            Dictionary<string, IValidator<JToken>> existingValidators) => T.IsNull;
    }

    record TypeSpec(string TypeName)
        : ValidatorSpec(ValidatorSpecKind.Type)
    {
        public override IValidator<JToken> GetValidator<T>(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => existingValidators.ContainsKey(TypeName)
                ? existingValidators[TypeName]
                : T.DelayedValidator(() => existingValidators.ContainsKey(TypeName)
                    ? existingValidators[TypeName]
                    : T.IsAnything);
    }

    record ArraySpec(ValidatorSpec ElementValidator, int? MinCount, int? MaxCount)
        : ValidatorSpec(ValidatorSpecKind.Array)
    {
        public override IValidator<JToken> GetValidator<T>(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => T.IsArrayOf(ElementValidator.GetValidator<T>(existingValidators), MinCount, MaxCount);
    }

    record DictionarySpec(ValidatorSpec ElementValidator, int? MinCount, int? MaxCount)
    : ValidatorSpec(ValidatorSpecKind.Array)
    {
        public override IValidator<JToken> GetValidator<T>(
            Dictionary<string, IValidator<JToken>> existingValidators)
            => T.IsDictionaryOf(ElementValidator.GetValidator<T>(existingValidators), MinCount, MaxCount);
    }
}
