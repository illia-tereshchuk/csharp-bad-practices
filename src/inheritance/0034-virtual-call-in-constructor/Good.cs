// Exhibit #0034: the fix

// Each plan exposes its surcharge rate; subclasses decide how it's computed.
var plan = new RegionalPlan(regionMultiplier: 3m);

Console.WriteLine("Region multiplier: 3");
Console.WriteLine($"Surcharge rate:    {plan.SurchargeRate}");

// The override multiplies the base rate by the region factor: 0.10 * 3 = 0.30 expected.
if (plan.SurchargeRate != 0.30m)
{
    throw new InvalidOperationException(
        $"surcharge rate is {plan.SurchargeRate} - region multiplier 3 should have produced 0.30");
}

Console.WriteLine("Every order in this region gets the right surcharge.");

abstract class BillingPlan
{
    // Nothing overridable runs during construction.
    protected BillingPlan()
    {
    }

    // Computed on demand, after every constructor body has finished.
    public decimal SurchargeRate => ComputeSurchargeRate();

    protected abstract decimal ComputeSurchargeRate();
}

class RegionalPlan : BillingPlan
{
    private readonly decimal _regionMultiplier;

    public RegionalPlan(decimal regionMultiplier)
    {
        // By the time anyone reads SurchargeRate, this assignment has run.
        _regionMultiplier = regionMultiplier;
    }

    protected override decimal ComputeSurchargeRate() => 0.10m * _regionMultiplier;
}
