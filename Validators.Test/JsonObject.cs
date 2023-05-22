using FluentAssertions;
using Newtonsoft.Json.Linq;
using static Validators.NewtonsoftJson.Json;

namespace Validators.Test
{
    public class JsonObject
    {
        [Fact]
        public void ValidKeysPasses()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true
                }
                """);
            var validator = HasValidKeys("a");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidKeysFails()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true,
                    "b": null
                }
                """);
            var validator = HasValidKeys("a");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Key 'b' is not valid.");
            errors.First().Path.Should().BeEmpty();
        }

        [Fact]
        public void HasKeyPasses()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true
                }
                """);
            var validator = HasKey("a");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void HasKeyFails()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "b": true
                }
                """);
            var validator = HasKey("a");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Doesn't have key 'a'.");
            errors.First().Path.Should().BeEmpty();
        }

        [Fact]
        public void RequiredKeysPasses()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true,
                    "b": false
                }
                """);
            var validator = HasRequiredKeys("a", "b");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void RequiredKeysFails()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true,
                    "b": false
                }
                """);
            var validator = HasRequiredKeys("a", "b", "c");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Key 'c' is missing.");
            errors.First().Path.Should().BeEmpty();
        }

        [Fact]
        public void DiveIntoPasses()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true,
                    "b": false
                }
                """);
            var validator = DiveInto("a", IsBoolean);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void DiveIntoFails()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true,
                    "b": false
                }
                """);
            var validator = DiveInto("a", IsNull);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not 'null'.");
            errors.First().Path.Should().BeEquivalentTo(new string[] { "a" });
        }
    }
}
