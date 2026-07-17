// Exhibit #0002: the fix

// Ten loyalty top-ups of 0.10 each hit the register today.
var payments = Enumerable.Repeat(0.10m, 10).ToArray();

decimal total = payments.Sum();
decimal expected = 1.00m;

Console.WriteLine($"Payments received: {payments.Length} x 0.10");
Console.WriteLine($"Register total:    {total}");
Console.WriteLine($"Expected total:    {expected}");

if (total != expected)
{
    throw new InvalidOperationException(
        $"Audit failed: the register is off by {expected - total}");
}

Console.WriteLine("Audit passed. Everyone goes home on time.");
