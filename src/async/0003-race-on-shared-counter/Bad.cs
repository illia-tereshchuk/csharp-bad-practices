// Exhibit #0003: incrementing a shared counter from parallel threads

// Two ticket kiosks sell all day; every sale bumps the shared counter.
const int SalesPerKiosk = 100_000;
int ticketsSold = 0;

void RunKiosk()
{
    for (int i = 0; i < SalesPerKiosk; i++)
    {
        ticketsSold++; // one sale, one increment. What could go wrong?
    }
}

var kioskA = Task.Run(RunKiosk);
var kioskB = Task.Run(RunKiosk);
await Task.WhenAll(kioskA, kioskB);

int expected = 2 * SalesPerKiosk;

Console.WriteLine($"Tickets printed:  {expected}");
Console.WriteLine($"Sales registered: {ticketsSold}");

if (ticketsSold != expected)
{
    throw new InvalidOperationException(
        $"Audit failed: {expected - ticketsSold} sales vanished into thin air");
}

Console.WriteLine("Audit passed. Every ticket accounted for.");
