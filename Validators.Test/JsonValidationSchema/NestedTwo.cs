using FluentAssertions;
using Newtonsoft.Json.Linq;
using Validators.NewtonsoftJson;

namespace Validators.Test.JsonValidationSchema
{
    public class NestedTwo
    {
        [Fact]
        public void Passes()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "Lead": {
                        "Name": "John",
                        "Age": 35,
                        "Kind": "firstkind",
                        "DateOfBirth": "aaa"
                    },
                    "Members": [
                        {
                            "Name": "Henry",
                            "Age": 30,
                            "Kind": "secondKind",
                            "DateOfBirth": "yyyy"
                        }
                    ],
                    "Budget": 100,
                    "CreationDate": "xxx",
                    "Properties": {
                        "first": {
                            "Name": "x",
                            "Value": 1
                        },
                        "second": {
                            "Name": "y",
                            "Value": 2,
                            "People": [
                                {
                                    "Name": "Henry",
                                    "Age": 30,
                                    "Kind": "secondKind",
                                    "DateOfBirth": "yyyy"
                                }
                            ]
                        }
                    }
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Team),
                ValidationSchemaOptions
                    .Empty
                    .SetAllowExtras(false)
                    .SetOptional<Property>(x => x.People));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().BeEmpty();
        }
    }
}
