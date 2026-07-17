// Exhibit #0002: calculating money with double

// Ten loyalty top-ups of 0.10 each hit the register today.
var payments = Enumerable.Repeat(0.10, 10).ToArray();

double total = payments.Sum();
double expected = 1.00;

Console.WriteLine($"Payments received: {payments.Length} x 0.10");
Console.WriteLine($"Register total:    {total:R}");
Console.WriteLine($"Expected total:    {expected:R}");

if (total != expected)
{
    throw new InvalidOperationException(
        $"Audit failed: the register is off by {expected - total:R}");
}

Console.WriteLine("Audit passed. Everyone goes home on time.");
