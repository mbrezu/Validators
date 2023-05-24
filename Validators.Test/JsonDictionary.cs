using FluentAssertions;
using Newtonsoft.Json.Linq;
using static Validators.NewtonsoftJson.Json;

namespace Validators.Test
{
    public class JsonDictionary
    {
        [Fact]
        public void EmptyDictionaryPasses()
        {
            // Arrange
            var target = JObject.Parse("""
                {}
                """);
            var validator = IsDictionaryOf(IsNull);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void DictionaryFails()
        {
            // Arrange
            var target = JArray.Parse("""
                []
                """);
            var validator = IsDictionaryOf(IsNull);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not an object.");
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void DictionaryPasses()
        {
            // Arrange
            var target = JObject.Parse("""
                { "a": 1, "b": 2, "c": 3 }
                """);
            var validator = IsDictionaryOf(IsNumber);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void StringDictionaryFails()
        {
            // Arrange
            var target = JObject.Parse("""
                { "a": "foo", "b": "bar", "c": 3 }
                """);
            var validator = IsDictionaryOf(IsString);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not a string.");
            errors.First().ReversePath.Should().BeEquivalentTo(new string[] { "c" });
        }

        [Fact]
        public void StringDictionaryMultipleFailures()
        {
            // Arrange
            var target = JObject.Parse("""
                { "a": "foo", "b": true, "c": 3 }
                """);
            var validator = IsDictionaryOf(IsString);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(2);
            errors.ElementAt(0).Message.Should().Be("Not a string.");
            errors.ElementAt(0).ReversePath.Should().BeEquivalentTo(new string[] { "b" });
            errors.ElementAt(1).Message.Should().Be("Not a string.");
            errors.ElementAt(1).ReversePath.Should().BeEquivalentTo(new string[] { "c" });
        }

        [Fact]
        public void DictionaryMinCountPasses()
        {
            // Arrange
            var target = JObject.Parse("""
                { "a": 1, "b": 2, "c": 3 }
                """);
            var validator = IsDictionaryOf(IsNumber, 3);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void DictionaryMinCountFails()
        {
            // Arrange
            var target = JObject.Parse("""
                { "a": 1, "b": 2, "c": 3 }
                """);
            var validator = IsDictionaryOf(IsNumber, 5);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Object property count is 3, but should be at least 5.");
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void DictionaryMaxCountPasses()
        {
            // Arrange
            var target = JObject.Parse("""
                { "a": 1, "b": 2, "c": 3 }
                """);
            var validator = IsDictionaryOf(IsNumber, null, 3);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void DictionaryMaxCountFails()
        {
            // Arrange
            var target = JObject.Parse("""
                { "a": 1, "b": 2, "c": 3 }
                """);
            var validator = IsDictionaryOf(IsNumber, null, 2);

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Object property count is 3, but should be at most 2.");
            errors.First().ReversePath.Should().BeEmpty();
        }
    }
}
