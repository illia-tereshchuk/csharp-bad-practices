// Exhibit #0033: a switch expression that only ever warned about missing cases

// OrderStatus lives in a shared contract package both the fulfillment centre
// and this notification service reference. Fulfillment just shipped Returned;
// this switch was written before that release and never got updated.
var orders = new[]
{
    (Id: 1001, Status: OrderStatus.Placed),
    (Id: 1002, Status: OrderStatus.Shipped),
    (Id: 1003, Status: OrderStatus.Delivered),
    (Id: 1004, Status: OrderStatus.Returned), // fulfillment started using this last week
};

foreach (var order in orders)
{
    Console.WriteLine($"Order {order.Id}: {Notify(order.Status)}"); // 💥 throws on Returned
}

Console.WriteLine("All customers notified.");

string Notify(OrderStatus status) => status switch
{
    OrderStatus.Placed => "Your order has been placed.",
    OrderStatus.Shipped => "Your order is on its way.",
    OrderStatus.Delivered => "Your order has been delivered.",
    // CS8509: not exhaustive - a warning, not an error, so this compiles and ships.
};

enum OrderStatus { Placed, Shipped, Delivered, Returned }
