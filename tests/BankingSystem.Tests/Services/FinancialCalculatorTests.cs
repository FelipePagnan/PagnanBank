using BankingSystem.Domain.Common;
using FluentAssertions;
using Xunit;

namespace BankingSystem.Tests.Services;

public sealed class FinancialCalculatorTests
{
    [Fact]
    public void MonthlyInstallment_WithZeroRate_SplitsPrincipalEvenly()
    {
        var installment = FinancialCalculator.MonthlyInstallment(1200m, 0m, 12);
        installment.Should().Be(100m);
    }

    [Fact]
    public void MonthlyInstallment_WithPositiveRate_IsGreaterThanSimpleSplit()
    {
        var installment = FinancialCalculator.MonthlyInstallment(1200m, 24m, 12);
        installment.Should().BeGreaterThan(100m);
    }

    [Fact]
    public void LoanTotal_IsInstallmentTimesMonths()
    {
        var installment = FinancialCalculator.MonthlyInstallment(5000m, 24m, 10);
        var total = FinancialCalculator.LoanTotal(5000m, 24m, 10);
        total.Should().Be(decimal.Round(installment * 10, 2));
    }

    [Fact]
    public void FutureValue_WithZeroMonths_ReturnsPrincipal()
    {
        FinancialCalculator.FutureValue(1000m, 12m, 0).Should().Be(1000m);
    }

    [Fact]
    public void FutureValue_GrowsWithPositiveRate()
    {
        FinancialCalculator.FutureValue(1000m, 12m, 12).Should().BeGreaterThan(1000m);
    }

    [Fact]
    public void MonthsBetween_NeverNegative()
    {
        var later = new DateTime(2025, 6, 1);
        var earlier = new DateTime(2025, 1, 1);
        FinancialCalculator.MonthsBetween(later, earlier).Should().Be(0);
        FinancialCalculator.MonthsBetween(earlier, later).Should().Be(5);
    }

    [Fact]
    public void Sac_FirstInstallment_IsGreaterThanLast()
    {
        var first = FinancialCalculator.SacFirstInstallment(60000m, 12m, 60);
        var last = FinancialCalculator.SacLastInstallment(60000m, 12m, 60);
        first.Should().BeGreaterThan(last);
    }

    [Fact]
    public void SacTotal_IsGreaterThanPrincipal_WithInterest()
    {
        FinancialCalculator.SacTotal(60000m, 12m, 60).Should().BeGreaterThan(60000m);
    }

    [Fact]
    public void PresentValueOfInstallments_IsLessThanNominal_WithPositiveRate()
    {
        var nominal = 500m * 10;
        var pv = FinancialCalculator.PresentValueOfInstallments(500m, 24m, 10);
        pv.Should().BeLessThan(nominal);
        pv.Should().BeGreaterThan(0m);
    }

    [Fact]
    public void PresentValueOfInstallments_WithZeroRate_EqualsNominal()
    {
        FinancialCalculator.PresentValueOfInstallments(500m, 0m, 10).Should().Be(5000m);
    }
}
