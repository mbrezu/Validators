using FluentAssertions;
using Newtonsoft.Json.Linq;
using Validators.NewtonsoftJson;

namespace Validators.Test.JsonValidationSchema
{
    public class Basic
    {
        [Fact]
        public void Fails()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Person));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(3);
            errors.ElementAt(0).Message.Should().Be("Key 'Name' is missing.");
            errors.ElementAt(0).Path.Should().BeEmpty();
            errors.ElementAt(1).Message.Should().Be("Key 'Kind' is missing.");
            errors.ElementAt(1).Path.Should().BeEmpty();
            errors.ElementAt(2).Message.Should().Be("Key 'Age' is missing.");
            errors.ElementAt(2).Path.Should().BeEmpty();
        }

        [Fact]
        public void PassesCaseInsensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "Henry",
                    "age": 10,
                    "kind": "firstkind"
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Person));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void FailsCaseSensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "Henry",
                    "age": 10,
                    "kind": "firstkind"
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Person));
            var validator = schema.GetValidator(false);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(3);
            errors.ElementAt(0).Message.Should().Be("Key 'Name' is missing.");
            errors.ElementAt(0).Path.Should().BeEmpty();
            errors.ElementAt(1).Message.Should().Be("Key 'Kind' is missing.");
            errors.ElementAt(1).Path.Should().BeEmpty();
            errors.ElementAt(2).Message.Should().Be("Key 'Age' is missing.");
            errors.ElementAt(2).Path.Should().BeEmpty();
        }

        [Fact]
        public void FailsCaseSensitiveEnums()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "Henry",
                    "age": 10,
                    "kind": "firstkind"
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Person), ValidationSchemaOptions.Empty with
            {
                EnumIgnoreCase = false
            });
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Not one of (\"FirstKind\", \"SecondKind\").");
            errors.ElementAt(0).Path.Should().BeEquivalentTo(new string[] { "Kind" });
        }

        [Fact]
        public void FailsExtraKeys()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "Henry",
                    "age": 10,
                    "kind": "firstkind",
                    "extra": null
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Person), ValidationSchemaOptions.Empty with
            {
                AllowExtras = false
            });
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Key 'extra' is not valid.");
            errors.ElementAt(0).Path.Should().BeEmpty();
        }

        [Fact]
        public void FailsRequiredOption()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "Henry",
                    "age": 10,
                    "kind": "firstkind",
                }
                """);
            var schema = ValidationSchema.FromType(
                typeof(Person),
                ValidationSchemaOptions
                    .Empty
                    .AddRequired<Person>(x => x.IsAdmin));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Key 'IsAdmin' is missing.");
            errors.ElementAt(0).Path.Should().BeEmpty();
        }

        [Fact]
        public void PassesOptionalOption()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "Henry",
                    "age": 10,
                }
                """);
            var schema = ValidationSchema.FromType(
                typeof(Person), ValidationSchemaOptions
                    .Empty
                    .AddOptional<Person>(x => x.Kind));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }
    }
}
