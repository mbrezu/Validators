using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit.Sdk;
using static Validators.NewtonsoftJson.Json;

namespace Validators.Test
{
    public class JsonObject
    {
        [Fact]
        public void ValidKeysPassesCi()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true
                }
                """);
            var validator = HasValidKeys("A");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidKeysPassesCaseSensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true
                }
                """);
            var validator = HasValidKeys(false, "a");

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
        public void ValidKeysCaseSensitiveFails()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "A": true,
                }
                """);
            var validator = HasValidKeys(false, "a");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Key 'A' is not valid.");
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
        public void RequiredKeysPassesCi()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true,
                    "b": false
                }
                """);
            var validator = HasRequiredKeys("A", "b");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void RequiredKeysPassesCaseSensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true,
                    "b": false
                }
                """);
            var validator = HasRequiredKeys(false, "a", "b");

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
        public void RequiredKeysFailsCaseSensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true,
                    "b": false
                }
                """);
            var validator = HasRequiredKeys(false, "a", "B");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Key 'B' is missing.");
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
