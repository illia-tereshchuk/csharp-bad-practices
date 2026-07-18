// Exhibit #0016: a cancellation token nobody reads

using var cts = new CancellationTokenSource();

// The user hits Cancel almost immediately.
cts.CancelAfter(50);

int printed = 0;

await PrintStatements(cts.Token);

Console.WriteLine($"Cancel requested:   {cts.IsCancellationRequested}");
Console.WriteLine($"Statements printed: {printed} of 20");

if (cts.IsCancellationRequested && printed == 20)
{
    throw new InvalidOperationException(
        "the user cancelled at the start - the job delivered everything anyway");
}

Console.WriteLine("Cancellation actually cancelled something.");

// The token is welcomed in every signature...
async Task PrintStatements(CancellationToken token)
{
    for (int i = 1; i <= 20; i++)
    {
        await RenderStatement(i, token);
        printed++;
    }
}

// ...and read in none.
async Task RenderStatement(int number, CancellationToken token)
{
    await Task.Delay(25); // 💥 the delay never heard about the token
}
