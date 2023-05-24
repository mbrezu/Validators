using FluentAssertions;
using Newtonsoft.Json.Linq;
using Validators.Json;

namespace Validators.Test.JsonValidationSchema
{
    public class NullableString
    {
        [Fact]
        public void Passes()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                }
                """);
            var schema = ValidationSchema.FromType(typeof(ContainsNullableString));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }
    }
}
