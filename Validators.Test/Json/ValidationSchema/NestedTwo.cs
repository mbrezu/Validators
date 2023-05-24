using FluentAssertions;
using Newtonsoft.Json.Linq;
using Validators.Json;
using ValidationSchema = Validators.Json.ValidationSchema<Validators.Json.Newtonsoft.Library>;

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

        [Fact]
        public void FailsMinCountArray()
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
                    ],
                    "Budget": 100,
                    "CreationDate": "xxx",
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Team),
                ValidationSchemaOptions
                    .Empty
                    .SetAllowExtras(false)
                    .SetOptional<Property>(x => x.People)
                    .SetOptional<Team>(x => x.Properties)
                    .SetMinCount<Team>(x => x.Members, 2));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Array count is 0, but should be at least 2.");
            errors.ElementAt(0).ReversePath.Should().BeEquivalentTo(new string[] { "Members" });
        }

        [Fact]
        public void FailsMinCountDictionary()
        {
            // Arrange
            var target = JObject.Parse("""
                {
                    "Budget": 100,
                    "CreationDate": "xxx",
                    "Properties": {}
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Team),
                ValidationSchemaOptions
                    .Empty
                    .SetAllowExtras(false)
                    .SetOptional<Property>(x => x.People)
                    .SetOptional<Team>(x => x.Lead)
                    .SetOptional<Team>(x => x.Members)
                    .SetMinCount<Team>(x => x.Properties, 2));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Object property count is 0, but should be at least 2.");
            errors.ElementAt(0).ReversePath.Should().BeEquivalentTo(new string[] { "Properties" });
        }

        [Fact]
        public void FailsMaxCountArray()
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
                            "Name": "John",
                            "Age": 35,
                            "Kind": "firstkind",
                            "DateOfBirth": "aaa"
                        },
                        {
                            "Name": "John",
                            "Age": 35,
                            "Kind": "firstkind",
                            "DateOfBirth": "aaa"
                        }
                    ],
                    "Budget": 100,
                    "CreationDate": "xxx",
                }
                """);
            var schema = ValidationSchema.FromType(typeof(Team),
                ValidationSchemaOptions
                    .Empty
                    .SetAllowExtras(false)
                    .SetOptional<Property>(x => x.People)
                    .SetOptional<Team>(x => x.Properties)
                    .SetMaxCount<Team>(x => x.Members, 1));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Array count is 2, but should be at most 1.");
            errors.ElementAt(0).ReversePath.Should().BeEquivalentTo(new string[] { "Members" });
        }

        [Fact]
        public void FailsMaxCountDictionary()
        {
            // Arrange
            var target = JObject.Parse("""
                {
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
                    .SetOptional<Property>(x => x.People)
                    .SetOptional<Team>(x => x.Lead)
                    .SetOptional<Team>(x => x.Members)
                    .SetMaxCount<Team>(x => x.Properties, 1));
            var validator = schema.GetValidator();

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(1);
            errors.ElementAt(0).Message.Should().Be("Object property count is 2, but should be at most 1.");
            errors.ElementAt(0).ReversePath.Should().BeEquivalentTo(new string[] { "Properties" });
        }
    }
}
