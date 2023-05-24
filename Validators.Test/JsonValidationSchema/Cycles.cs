using FluentAssertions;
using Newtonsoft.Json.Linq;
using Validators.Json;

namespace Validators.Test.JsonValidationSchema
{
    public class Cycles
    {
        [Fact]
        public void SelfCyclePasses()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "test",
                    "size": 1,
                    "parent": {
                        "name": "other",
                        "size": 2,
                        "parent": {
                            "name": "foo",
                            "size": 3
                        }
                    }
                }
                """);
            var schema = ValidationSchema.FromType(typeof(SelfCycle));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void SelfCycleFails()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "test",
                    "size": 1,
                    "parent": {
                        "name": "foo",
                        "size": 2,
                        "parent": {
                            "name": 20,
                            "size": 3
                        }
                    }
                }
                """);
            var schema = ValidationSchema.FromType(typeof(SelfCycle));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Not a string.");
            errors.ElementAt(0).ReversePath.Should().BeEquivalentTo(new string[] { "Name", "Parent", "Parent" });
        }

        [Fact]
        public void MutualCyclePasses()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "test",
                    "other2": {
                        "size": 2,
                        "other1": {
                            "name": "foo",
                            "other2": {
                                "size": 3
                            }
                        }
                    }
                }
                """);
            var schema = ValidationSchema.FromType(typeof(MutualCycle1));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void MutualCycleFails()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "name": "test",
                    "other2": {
                        "size": 2,
                        "other1": {
                            "name": "foo",
                            "other2": {
                                "sizeo": 3
                            }
                        }
                    }
                }
                """);
            var schema = ValidationSchema.FromType(typeof(MutualCycle1));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Key 'Size' is missing.");
            errors.ElementAt(0).ReversePath.Should().BeEquivalentTo(new string[] { "Other2", "Other1", "Other2" });
        }
    }
}
