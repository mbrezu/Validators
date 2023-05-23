using FluentAssertions;
using Newtonsoft.Json.Linq;
using Validators.NewtonsoftJson;

namespace Validators.Test.JsonValidationSchema
{
    public class NestedOne
    {
        [Fact]
        public void Passes()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "test",
                    "value": 1,
                    "people": [
                        {
                            "name": "Henry",
                            "age": 20,
                            "kind": "firstkind",
                            "extra": null
                        },
                        {
                            "name": "John",
                            "age": 25,
                            "kind": "secondkind",
                        }
                    ]
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Property));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void FailsWrongType()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "test",
                    "value": 1,
                    "people": [true]
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Property));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Not an object.");
            errors.ElementAt(0).Path.Should().BeEquivalentTo(new string[] { "0", "People" });
        }

        [Fact]
        public void FailsWrongStructure()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "test",
                    "value": 1,
                    "people": [{}]
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Property));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(3);
            errors.ElementAt(0).Message.Should().Be("Key 'Name' is missing.");
            errors.ElementAt(0).Path.Should().BeEquivalentTo(new string[] { "0", "People" });
            errors.ElementAt(1).Message.Should().Be("Key 'Kind' is missing.");
            errors.ElementAt(1).Path.Should().BeEquivalentTo(new string[] { "0", "People" });
            errors.ElementAt(2).Message.Should().Be("Key 'Age' is missing.");
            errors.ElementAt(2).Path.Should().BeEquivalentTo(new string[] { "0", "People" });
        }

        [Fact]
        public void FailsExtraFields()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "test",
                    "value": 1,
                    "people": [{
                        "name": "Henry",
                        "age": 20,
                        "kind": "firstkind",
                        "extra": null
                    }]
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Property), ValidationSchemaOptions.Empty with
            {
                AllowExtras = false
            });
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Key 'extra' is not valid.");
            errors.ElementAt(0).Path.Should().BeEquivalentTo(new string[] { "0", "People" });
        }
    }
}
