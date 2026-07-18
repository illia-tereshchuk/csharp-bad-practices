// Exhibit #0013: the fix

// Two subscriber exports merged for the newsletter. Time to dedupe.
var fromWebsite = new List<Subscriber>
{
    new("olena@example.com"),
    new("ivan@example.com"),
};

var fromMobileApp = new List<Subscriber>
{
    new("ivan@example.com"), // Ivan uses both platforms
    new("petro@example.com"),
};

var recipients = fromWebsite.Concat(fromMobileApp).Distinct().ToList();

Console.WriteLine($"Merged entries: {fromWebsite.Count + fromMobileApp.Count}");
Console.WriteLine($"After Distinct: {recipients.Count}");

foreach (var recipient in recipients)
{
    Console.WriteLine($"  sending to {recipient.Email}");
}

var uniqueEmails = recipients.Select(r => r.Email).Distinct().Count();

if (recipients.Count != uniqueEmails)
{
    throw new InvalidOperationException(
        $"{uniqueEmails} unique emails, {recipients.Count} recipients - someone gets spammed twice");
}

Console.WriteLine("Every subscriber gets exactly one email.");

record Subscriber(string Email);
