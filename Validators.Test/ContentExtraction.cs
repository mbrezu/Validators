using FluentAssertions;
using Newtonsoft.Json.Linq;
using Validators.Json.Newtonsoft;
using static Validators.Json.Newtonsoft.Json;

namespace Validators.Test
{
    public class ContentExtraction
    {
        [Fact]
        public void ExtractContent()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "a": 1,
                    "b": true,
                    "c": [1, 2, 3, "fail"]
                }
                """);
            var validator = And(
                IsObject,
                HasRequiredKeys("a", "b", "c"),
                HasValidKeys("a", "b", "c"),
                DiveInto("c", IsArrayOf(IsNumber)));

            // Act
            var errors = validator.Validate(target);
            var content = errors.ElementAt(0).ExtractInvalidContent(target);

            // Assert
            content.Should().NotBeNull();
            content!.Type.Should().Be(JTokenType.String);
            content!.Value<string>()!.Should().Be("fail");
        }
    }
}
