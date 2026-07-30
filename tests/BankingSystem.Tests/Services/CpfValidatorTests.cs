using BankingSystem.Domain.Common;
using FluentAssertions;
using Xunit;

namespace BankingSystem.Tests.Services;

public sealed class CpfValidatorTests
{
    [Theory]
    [InlineData("529.982.247-25")]  // valid, formatted
    [InlineData("52998224725")]     // valid, digits only
    public void IsValid_WithValidCpf_ReturnsTrue(string cpf)
        => CpfValidator.IsValid(cpf).Should().BeTrue();

    [Theory]
    [InlineData("12345678901")]     // wrong check digits
    [InlineData("11111111111")]     // all identical
    [InlineData("00000000000")]     // all identical
    [InlineData("123456789")]       // too short
    [InlineData("")]                // empty
    [InlineData(null)]              // null
    public void IsValid_WithInvalidCpf_ReturnsFalse(string? cpf)
        => CpfValidator.IsValid(cpf).Should().BeFalse();
}
