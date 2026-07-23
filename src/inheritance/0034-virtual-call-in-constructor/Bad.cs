// Exhibit #0034: calling a virtual method from a base constructor

// Each plan caches its surcharge rate up front; subclasses decide how it's computed.
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
    // The base constructor "primes" the cached rate by asking the subclass.
    protected BillingPlan()
    {
        SurchargeRate = ComputeSurchargeRate(); // 💥 virtual call before the derived ctor body runs
    }

    public decimal SurchargeRate { get; }

    protected abstract decimal ComputeSurchargeRate();
}

class RegionalPlan : BillingPlan
{
    private readonly decimal _regionMultiplier;

    public RegionalPlan(decimal regionMultiplier)
    {
        // This runs AFTER the base constructor has already called the override,
        // so the override read _regionMultiplier while it was still 0.
        _regionMultiplier = regionMultiplier;
    }

    protected override decimal ComputeSurchargeRate() => 0.10m * _regionMultiplier;
}
