using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using static Validators.NewtonsoftJson.Json;

namespace Validators.Test
{
    public class JsonBasic
    {
        [Fact]
        public void AnythingPasses()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": 1
                }
                """);
            var validator = IsAnything;

            // Act.
            var errors = validator.Validate(target);

            // Assert.
            errors.Should().BeEmpty();
        }

        [Fact]
        public void NumberPasses()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": 1
                }
                """);
            var validator = IsNumber;

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().BeEmpty();
        }

        [Fact]
        public void DelayedNumberPasses()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": 1
                }
                """);
            var validator = DelayedValidator(() => IsNumber);

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().BeEmpty();
        }

        [Fact]
        public void NumberFails()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": null
                }
                """);
            var validator = IsNumber;

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not a number.");
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void DelayedNumberFails()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": null
                }
                """);
            var validator = DelayedValidator(() => IsNumber);

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not a number.");
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void StringPasses()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": "str"
                }
                """);
            var validator = IsString;

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().BeEmpty();
        }

        [Fact]
        public void StringFails()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": true
                }
                """);
            var validator = IsString;

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not a string.");
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void OneOfPasses()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": "str"
                }
                """);
            var validator = IsOneOf("str", "test");

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().BeEmpty();
        }

        [Fact]
        public void OneOfFails()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": "foo"
                }
                """);
            var validator = IsOneOf("str", "test");

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("""Not one of ("str", "test").""");
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void MatchesRegexPasses()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": "2023-05-22"
                }
                """);
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
            var validator = MatchesRegex(new Regex(@"^\d{4}-\d{2}-\d{2}$"));
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().BeEmpty();
        }

        [Fact]
        public void MatchesRegexFails()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": "a2023-05-22"
                }
                """);
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
            var validator = MatchesRegex(new Regex(@"^\d{4}-\d{2}-\d{2}$"));
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("""Not a match for regex ^\d{4}-\d{2}-\d{2}$.""");
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void CustomValidatorPasses()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": "2023-05-22"
                }
                """);
            var validator = CustomValidator(
                "a date",
                t => DateTime.TryParse((string?)(t.Value<string>() ?? ""), out _));

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().BeEmpty();
        }

        [Fact]
        public void CustomValidatorFails()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": "2023-25-22"
                }
                """);
            var validator = CustomValidator(
                "a date",
                t => DateTime.TryParse((string?)(t.Value<string>() ?? ""), out _));

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("""Not a date.""");
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void BooleanPasses()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": true
                }
                """);
            var validator = IsBoolean;

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().BeEmpty();
        }

        [Fact]
        public void BooleanFails()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": null
                }
                """);
            var validator = IsBoolean;

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not a boolean.");
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void NullPasses()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": null
                }
                """);
            var validator = IsNull;

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().BeEmpty();
        }

        [Fact]
        public void NullFails()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": true
                }
                """);
            var validator = IsNull;

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not 'null'.");
            errors.First().ReversePath.Should().BeEmpty();
        }

        [Fact]
        public void ObjectPasses()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": null
                }
                """);
            var validator = IsObject;

            // Act.
            var errors = validator.Validate(target);

            // Assert.
            errors.Should().BeEmpty();
        }

        [Fact]
        public void ObjectFails()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": null
                }
                """);
            var validator = IsObject;

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("Not an object.");
            errors.First().ReversePath.Should().BeEmpty();
        }
    }
}