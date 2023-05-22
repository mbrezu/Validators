using FluentAssertions;
using Newtonsoft.Json.Linq;
using static Validators.NewtonsoftJson.Json;

namespace Validators.Test
{
    public class JsonAndOr
    {
        [Fact]
        public void AndPasses()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": 1,
                    "b": 2
                }
                """);
            var validator = And(
                IsObject,
                RequiredKeys("a", "b"),
                ValidKeys("a", "b"),
                DiveInto("a", IsNumber),
                DiveInto("b", IsNumber));

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void AndFails()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": 1,
                    "b": true
                }
                """);
            var validator = And(
                IsObject,
                RequiredKeys("a", "b", "c"),
                ValidKeys("a", "b"),
                DiveInto("a", IsNumber),
                DiveInto("b", IsNumber));

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(2);
            errors.ElementAt(0).Message.Should().Be("Key 'c' is missing.");
            errors.ElementAt(0).Path.Should().BeEmpty();
            errors.ElementAt(1).Message.Should().Be("Not a number.");
            errors.ElementAt(1).Path.Should().BeEquivalentTo(new string[] { "b" });
        }

        [Fact]
        public void OrPasses()
        {
            // Arrange
            var target = JArray.Parse("""
                [ 1, 2, 'a' ]
                """);
            var validator = IsArrayOf(Or(IsNumber, IsString));

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void OrFails()
        {
            // Arrange
            var target = JArray.Parse("""
                [ 1, 2, 'a', true ]
                """);
            var validator = IsArrayOf(Or(IsNumber, IsString));

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Not one of (number, string).");
            errors.ElementAt(0).Path.Should().BeEquivalentTo(new string[] { "3" });
        }
    }
}
