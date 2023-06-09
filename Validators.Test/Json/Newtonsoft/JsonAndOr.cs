﻿using FluentAssertions;
using Newtonsoft.Json.Linq;
using static Validators.Json.Newtonsoft.Library;

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
                HasRequiredKeys("a", "b"),
                HasValidKeys("a", "b"),
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
                HasRequiredKeys("a", "b", "c"),
                HasValidKeys("a", "b"),
                DiveInto("a", IsNumber),
                DiveInto("b", IsNumber));

            // Act
            var errors = validator.Validate(target);

            // Assert
            errors.Should().HaveCount(2);
            errors.ElementAt(0).Message.Should().Be("Key 'c' is missing.");
            errors.ElementAt(0).ReversePath.Should().BeEmpty();
            errors.ElementAt(1).Message.Should().Be("Not a number.");
            errors.ElementAt(1).ReversePath.Should().BeEquivalentTo(new string[] { "b" });
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
            errors.ElementAt(0).ReversePath.Should().BeEquivalentTo(new string[] { "3" });
        }
    }
}
