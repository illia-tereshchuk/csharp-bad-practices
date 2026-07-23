// Exhibit #0033: the fix

// OrderStatus lives in a shared contract package both the fulfillment centre
// and this notification service reference. Fulfillment just shipped Returned;
// the switch was updated in the same release that pulled the new package.
var orders = new[]
{
    (Id: 1001, Status: OrderStatus.Placed),
    (Id: 1002, Status: OrderStatus.Shipped),
    (Id: 1003, Status: OrderStatus.Delivered),
    (Id: 1004, Status: OrderStatus.Returned), // fulfillment started using this last week
};

foreach (var order in orders)
{
    Console.WriteLine($"Order {order.Id}: {Notify(order.Status)}");
}

Console.WriteLine("All customers notified.");

string Notify(OrderStatus status) => status switch
{
    OrderStatus.Placed => "Your order has been placed.",
    OrderStatus.Shipped => "Your order is on its way.",
    OrderStatus.Delivered => "Your order has been delivered.",
    OrderStatus.Returned => "Your return has been received.",
    _ => throw new ArgumentOutOfRangeException(nameof(status), status, "unknown order status"),
};

enum OrderStatus { Placed, Shipped, Delivered, Returned }
