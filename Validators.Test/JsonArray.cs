using FluentAssertions;
using Newtonsoft.Json.Linq;
using static Validators.NewtonsoftJson.Json;

namespace Validators.Test
{
    public class JsonArray
    {
        [Fact]
        public void EmptyArrayPasses()
        {
            // Arrange
            var target = JArray.Parse("""
                []
                """);
            var validator = IsArrayOf(IsNull);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void ArrayFails()
        {
            // Arrange
            var target = JObject.Parse("""
                {}
                """);
            var validator = IsArrayOf(IsNull);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not an array.");
            errors.First().Path.Should().BeEmpty();
        }
        
        [Fact]
        public void ArrayPasses()
        {
            // Arrange
            var target = JArray.Parse("""
                [1, 2, 3]
                """);
            var validator = IsArrayOf(IsNumber);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void StringArrayFails()
        {
            // Arrange
            var target = JArray.Parse("""
                ["a", "b", 3]
                """);
            var validator = IsArrayOf(IsString);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not a string.");
            errors.First().Path.Should().BeEquivalentTo(new string[] { "2" });
        }

        [Fact]
        public void StringArrayMultipleFailures()
        {
            // Arrange
            var target = JArray.Parse("""
                ["a", true, 3]
                """);
            var validator = IsArrayOf(IsString);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(2);
            errors.ElementAt(0).Message.Should().Be("Not a string.");
            errors.ElementAt(0).Path.Should().BeEquivalentTo(new string[] { "1" });
            errors.ElementAt(1).Message.Should().Be("Not a string.");
            errors.ElementAt(1).Path.Should().BeEquivalentTo(new string[] { "2" });
        }
    }
}
