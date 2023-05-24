using FluentAssertions;
using Newtonsoft.Json.Linq;
using ValidationSchema = Validators.Json.ValidationSchema<Validators.Json.Newtonsoft.Library>;

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
