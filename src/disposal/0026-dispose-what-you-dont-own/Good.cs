#:package Microsoft.Extensions.DependencyInjection@10.*
#:property PublishAot=false

// Exhibit #0026: the fix

using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<PricingApi>(); // one shared client for the whole app

using var root = services.BuildServiceProvider();

// Request A: quote a price. The handler uses the client and leaves it alone.
Console.WriteLine($"Request A -> {Quote(root, "SKU-1001")}");

// Request B: a different customer, moments later.
string? second = null;
try
{
    second = Quote(root, "SKU-2002");
}
catch (ObjectDisposedException)
{
    // swallowed the way a generic error handler would
}

Console.WriteLine($"Request B -> {second ?? "no price available"}");

if (second is null)
{
    throw new InvalidOperationException(
        "request B got no price - request A disposed the shared client it never owned");
}

Console.WriteLine("Both requests priced. The client is still serving.");

static string Quote(IServiceProvider provider, string sku)
{
    var api = provider.GetRequiredService<PricingApi>(); // borrowed - the container disposes it
    return api.GetPrice(sku);
}

class PricingApi : IDisposable
{
    private bool _disposed;

    public string GetPrice(string sku)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return $"{sku}: 149.99";
    }

    public void Dispose() => _disposed = true;
}
