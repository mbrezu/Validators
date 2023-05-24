using FluentAssertions;
using Newtonsoft.Json.Linq;
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
            errors.First().ReversePath.Should().BeEmpty();
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
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void HasKeyPassesCaseInsensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true
                }
                """);
            var validator = HasKey("A");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void HasKeyPassesCaseSensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true
                }
                """);
            var validator = HasKey(false, "a");

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
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void HasKeyFailsCaseSensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "b": true
                }
                """);
            var validator = HasKey(false, "B");

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Doesn't have key 'B'.");
            errors.First().ReversePath.Should().BeEmpty();
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
            errors.First().ReversePath.Should().BeEmpty();
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
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void DiveIntoPassesCaseInsensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true,
                    "b": false
                }
                """);
            var validator = DiveInto("A", IsBoolean);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        // This is a little weird. :-)
        //
        // The test passes because:
        // * the comparison is case sensitive,
        // * there is no 'A' key in the object and
        // * diving in succeeds when there is no key with the given name.
        [Fact]
        public void DiveIntoPassesCaseSensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": null,
                }
                """);
            var validator = DiveInto(false, "A", IsBoolean);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void DiveIntoFailsCaseInsensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true,
                    "b": false
                }
                """);
            var validator = DiveInto("A", IsNull);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not 'null'.");
            errors.First().ReversePath.Should().BeEquivalentTo(new string[] { "A" });
        }

        [Fact]
        public void DiveIntoFailsCaseSensitive()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": true,
                    "b": false
                }
                """);
            var validator = DiveInto(false, "a", IsNull);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not 'null'.");
            errors.First().ReversePath.Should().BeEquivalentTo(new string[] { "a" });
        }
    }
}
