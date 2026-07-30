namespace BankingSystem.Domain.Common;

/// <summary>
/// Pure financial math used by loan/investment simulators and contracts.
/// Kept dependency-free and deterministic so it can be unit tested easily.
/// </summary>
public static class FinancialCalculator
{
    /// <summary>Fixed monthly installment using the Price (French amortization) system.</summary>
    public static decimal MonthlyInstallment(decimal principal, decimal annualRatePercent, int months)
    {
        if (months <= 0) return 0m;

        var i = (double)(annualRatePercent / 100m) / 12.0;
        if (i <= 0)
            return decimal.Round(principal / months, 2);

        var p = (double)principal;
        var factor = i / (1 - Math.Pow(1 + i, -months));
        return decimal.Round((decimal)(p * factor), 2);
    }

    /// <summary>Total amount paid over the whole loan (installment * months).</summary>
    public static decimal LoanTotal(decimal principal, decimal annualRatePercent, int months)
        => decimal.Round(MonthlyInstallment(principal, annualRatePercent, months) * months, 2);

    /// <summary>Compound future value of a single deposit after N months.</summary>
    public static decimal FutureValue(decimal principal, decimal annualRatePercent, int months)
    {
        if (months <= 0) return principal;

        var i = (double)(annualRatePercent / 100m) / 12.0;
        return decimal.Round((decimal)((double)principal * Math.Pow(1 + i, months)), 2);
    }

    /// <summary>Whole months elapsed between two instants (never negative).</summary>
    public static int MonthsBetween(DateTime start, DateTime end)
    {
        var months = ((end.Year - start.Year) * 12) + end.Month - start.Month;
        return months < 0 ? 0 : months;
    }

    // ---- Financiamento (SAC - amortização constante) ----

    /// <summary>Constant monthly amortization (principal / months).</summary>
    public static decimal SacAmortization(decimal principal, int months)
        => months <= 0 ? 0m : decimal.Round(principal / months, 2);

    /// <summary>First (largest) installment under the SAC system.</summary>
    public static decimal SacFirstInstallment(decimal principal, decimal annualRatePercent, int months)
    {
        if (months <= 0) return 0m;
        var i = annualRatePercent / 100m / 12m;
        var amortization = principal / months;
        return decimal.Round(amortization + principal * i, 2);
    }

    /// <summary>Last (smallest) installment under the SAC system.</summary>
    public static decimal SacLastInstallment(decimal principal, decimal annualRatePercent, int months)
    {
        if (months <= 0) return 0m;
        var i = annualRatePercent / 100m / 12m;
        var amortization = principal / months;
        // On the final month only one amortization slice still accrues interest.
        return decimal.Round(amortization + amortization * i, 2);
    }

    /// <summary>Total paid over a SAC financing (sum of a linear installment series).</summary>
    public static decimal SacTotal(decimal principal, decimal annualRatePercent, int months)
    {
        if (months <= 0) return 0m;
        var i = annualRatePercent / 100m / 12m;
        var amortization = principal / months;
        decimal total = 0m;
        for (var k = 0; k < months; k++)
        {
            var outstanding = principal - amortization * k;
            total += amortization + outstanding * i;
        }
        return decimal.Round(total, 2);
    }

    // ---- Quitação antecipada (valor presente das parcelas restantes) ----

    /// <summary>
    /// Present value of the remaining installments discounted at the monthly rate.
    /// Represents a fair early-payoff amount.
    /// </summary>
    public static decimal PresentValueOfInstallments(decimal installment, decimal annualRatePercent, int remainingMonths)
    {
        if (remainingMonths <= 0) return 0m;

        var i = (double)(annualRatePercent / 100m) / 12.0;
        if (i <= 0)
            return decimal.Round(installment * remainingMonths, 2);

        var pv = 0.0;
        var inst = (double)installment;
        for (var k = 1; k <= remainingMonths; k++)
            pv += inst / Math.Pow(1 + i, k);

        return decimal.Round((decimal)pv, 2);
    }
}
