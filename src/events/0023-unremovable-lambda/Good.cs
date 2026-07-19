// Exhibit #0023: the fix

var feed = new PriceFeed();
var notifications = new List<string>();

// The handler gets a name, so it can be found again later.
Action<decimal> onPriceChanged = price => notifications.Add($"alert: {price:C}");

// The customer switches the price alert ON.
feed.PriceChanged += onPriceChanged;
Console.WriteLine($"Alert enabled.   Subscribers: {feed.SubscriberCount}");

feed.Publish(101m); // this one the customer asked for

// The customer switches the price alert OFF.
feed.PriceChanged -= onPriceChanged; // the same delegate instance - actually removed
Console.WriteLine($"Alert cancelled. Subscribers: {feed.SubscriberCount}");

int beforeCancel = notifications.Count;
feed.Publish(102m); // the customer should hear nothing
int afterCancel = notifications.Count;

Console.WriteLine($"Notifications after cancelling: {afterCancel - beforeCancel}");
foreach (var n in notifications)
    Console.WriteLine($"  {n}");

if (afterCancel > beforeCancel)
{
    throw new InvalidOperationException(
        $"the alert was cancelled and still fired {afterCancel - beforeCancel} time(s) - the -= removed nothing");
}

Console.WriteLine("Cancelled means cancelled.");

class PriceFeed
{
    public event Action<decimal>? PriceChanged;

    public int SubscriberCount => PriceChanged?.GetInvocationList().Length ?? 0;

    public void Publish(decimal price) => PriceChanged?.Invoke(price);
}
