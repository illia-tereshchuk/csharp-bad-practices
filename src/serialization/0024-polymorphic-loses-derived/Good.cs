#:property PublishAot=false

// Exhibit #0024: the fix

using System.Text.Json;
using System.Text.Json.Serialization;

// The customer's receipt. Payment is declared as the base type.
var receipt = new Receipt
{
    OrderId = 1042,
    Payment = new CardPayment { Provider = "Visa", Last4 = "4242", Amount = 149.99m },
};

var json = JsonSerializer.Serialize(receipt);

Console.WriteLine("Receipt sent to the customer:");
Console.WriteLine(json);
Console.WriteLine();

if (!json.Contains("4242") || !json.Contains("149.99"))
{
    throw new InvalidOperationException(
        "the receipt shipped without the card or the amount - the derived properties were dropped");
}

Console.WriteLine("Card and amount present. The receipt is complete.");

class Receipt
{
    public int OrderId { get; set; }
    public PaymentMethod Payment { get; set; } = null!;
}

// The hierarchy declares itself, so the serializer writes the runtime shape.
[JsonDerivedType(typeof(CardPayment), "card")]
class PaymentMethod
{
    public string Provider { get; set; } = "";
}

class CardPayment : PaymentMethod
{
    public string Last4 { get; set; } = "";
    public decimal Amount { get; set; }
}
