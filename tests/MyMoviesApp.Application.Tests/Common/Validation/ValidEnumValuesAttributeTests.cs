using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using MyMoviesApp.Application.Common.Validation;
using MyMoviesApp.Domain.Enums;
using Xunit;

namespace MyMoviesApp.Application.Tests.Common.Validation;

public class ValidEnumValuesAttributeTests
{
    private ValidationResult? Validate(object? value, string memberName = "TestProp")
    {
        var attribute = new ValidEnumValuesAttribute();
        var context = new ValidationContext(new object()) { MemberName = memberName };
        return attribute.GetValidationResult(value, context);
    }

    // ── null / empty pass ─────────────────────────────────────────────────────

    [Fact]
    public void Should_ReturnSuccess_ForNullValue()
    {
        Validate(null).Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void Should_ReturnSuccess_ForEmptyCollection()
    {
        Validate(new HashSet<Format>()).Should().Be(ValidationResult.Success);
    }

    // ── valid enum collections ────────────────────────────────────────────────

    [Fact]
    public void Should_ReturnSuccess_WhenAllFormatsAreValid()
    {
        var formats = new HashSet<Format> { Format.Dvd, Format.BluRay, Format.BluRay4K };
        Validate(formats).Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void Should_ReturnSuccess_WhenAllDigitalRetailersAreValid()
    {
        var retailers = new HashSet<DigitalRetailer> { DigitalRetailer.AppleTv, DigitalRetailer.MoviesAnywhere };
        Validate(retailers).Should().Be(ValidationResult.Success);
    }

    // ── invalid enum collections ──────────────────────────────────────────────

    [Fact]
    public void Should_ReturnError_WhenFormatCollectionContainsUndefinedValue()
    {
        // Format has values 0, 1, 2 — 99 is not defined
        var formats = new HashSet<Format> { Format.Dvd, (Format)99 };
        var result = Validate(formats);

        result.Should().NotBe(ValidationResult.Success);
        result!.ErrorMessage.Should().Contain("99");
    }

    [Fact]
    public void Should_ReturnError_WhenDigitalRetailerCollectionContainsUndefinedValue()
    {
        var retailers = new HashSet<DigitalRetailer> { (DigitalRetailer)999 };
        var result = Validate(retailers);

        result.Should().NotBe(ValidationResult.Success);
        result!.ErrorMessage.Should().Contain("999");
    }

    [Fact]
    public void Should_ReturnError_WhenCollectionHasMixOfValidAndInvalidValues()
    {
        var formats = new HashSet<Format> { Format.BluRay, (Format)42 };
        var result = Validate(formats);

        result.Should().NotBe(ValidationResult.Success);
        result!.ErrorMessage.Should().Contain("42");
    }

    // ── single enum value ─────────────────────────────────────────────────────

    [Fact]
    public void Should_ReturnSuccess_ForValidSingleEnumValue()
    {
        Validate(Format.BluRay4K).Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void Should_ReturnError_ForInvalidSingleEnumValue()
    {
        var result = Validate((Format)999);

        result.Should().NotBe(ValidationResult.Success);
        result!.ErrorMessage.Should().Contain("999");
    }

    // ── non-enum types pass through ───────────────────────────────────────────

    [Fact]
    public void Should_ReturnSuccess_ForNonEnumValue()
    {
        Validate("just a string").Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void Should_ReturnSuccess_ForListOfStrings()
    {
        Validate(new List<string> { "a", "b" }).Should().Be(ValidationResult.Success);
    }
}

