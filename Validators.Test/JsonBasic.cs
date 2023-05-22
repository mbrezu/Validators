using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using static Validators.NewtonsoftJson.Json;

namespace Validators.Test
{
    public class JsonBasic
    {
        [Fact]
        public void TestNumberPasses()
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
        public void TestNumberFails()
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
            errors.First().Path.Should().BeEmpty();
        }

        [Fact]
        public void TestStringPasses()
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
        public void TestStringFails()
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
            errors.First().Path.Should().BeEmpty();
        }

        [Fact]
        public void TestOneOfPasses()
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
        public void TestOneOfFails()
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
            errors.First().Path.Should().BeEmpty();
        }

        [Fact]
        public void TestMatchesRegexPasses()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": "2023-05-22"
                }
                """);
            var validator = MatchesRegex(new Regex(@"\d{4}-\d{2}-\d{2}"));

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().BeEmpty();
        }

        [Fact]
        public void TestMatchesRegexFails()
        {
            // Arrange.
            var target = JObject.Parse("""
                {
                    "a": "a2023-05-22"
                }
                """);
            var validator = MatchesRegex(new Regex(@"^\d{4}-\d{2}-\d{2}$"));

            // Act.
            var errors = validator.Validate(target["a"]!);

            // Assert.
            errors.Should().HaveCount(1);
            errors.First().Message.Should().Be("""Not a match for regex ^\d{4}-\d{2}-\d{2}$.""");
            errors.First().Path.Should().BeEmpty();
        }

        [Fact]
        public void TestBooleanPasses()
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
        public void TestBooleanFails()
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
            errors.First().Path.Should().BeEmpty();
        }

        [Fact]
        public void TestNullPasses()
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
        public void TestNullFails()
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
            errors.First().Path.Should().BeEmpty();
        }

        [Fact]
        public void TestObjectPasses()
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
        public void TestObjectFails()
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
            errors.First().Path.Should().BeEmpty();
        }
    }
}