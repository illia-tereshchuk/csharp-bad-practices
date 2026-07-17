#:package Microsoft.EntityFrameworkCore.Sqlite@10.*
#:package SQLitePCLRaw.bundle_e_sqlite3@2.*
#:property PublishAot=false

// Exhibit #0008: the fix

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var dbPath = Path.Combine(Path.GetTempPath(), "museum-0008-good.db");
File.Delete(dbPath);

var queryCount = 0;

using var db = new ShopContext(dbPath, () => queryCount++);
db.Database.EnsureCreated();

// Seed: 20 customers, one order each.
for (int i = 1; i <= 20; i++)
{
    db.Orders.Add(new Order
    {
        Total = 100m + i,
        Customer = new Customer { Name = $"Customer {i:D2}" },
    });
}
db.SaveChanges();

queryCount = 0; // the daily report starts here

// The daily report: every order with its customer's name.
var orders = db.Orders.Include(o => o.Customer).ToList(); // 1 query with a JOIN

foreach (var order in orders)
{
    Console.WriteLine($"Order #{order.Id}: {order.Customer.Name} paid {order.Total}");
}

Console.WriteLine();
Console.WriteLine($"Orders in the report: {orders.Count}");
Console.WriteLine($"SQL queries sent:     {queryCount}");

if (queryCount > 2)
{
    throw new InvalidOperationException(
        $"The report should cost about one query, not {queryCount}");
}

Console.WriteLine("The database barely noticed. As it should be.");

class ShopContext(string dbPath, Action onQuery) : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlite($"Data Source={dbPath}")
            .LogTo(_ => onQuery(), [RelationalEventId.CommandExecuting]);
}

class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}
