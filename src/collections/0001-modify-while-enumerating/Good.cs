// Exhibit #0001: the fix

var subscribers = new List<Subscriber>
{
    new("olena@example.com", IsActive: true),
    new("ivan@example.com", IsActive: false),
    new("petro@example.com", IsActive: false),
    new("maria@example.com", IsActive: true),
};

Console.WriteLine($"Subscribers before cleanup: {subscribers.Count}");

// The list knows how to remove by condition - no iterating over a live collection
var removed = subscribers.RemoveAll(s => !s.IsActive);

Console.WriteLine($"Inactive subscribers removed: {removed}");
Console.WriteLine($"Subscribers after cleanup: {subscribers.Count}");

record Subscriber(string Email, bool IsActive);
