// Exhibit #0016: the fix

using var cts = new CancellationTokenSource();

// The user hits Cancel almost immediately.
cts.CancelAfter(50);

int printed = 0;

try
{
    await PrintStatements(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("[worker] cancellation observed - stopping mid-batch");
}

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

// ...and read at the leaf, where the work happens.
async Task RenderStatement(int number, CancellationToken token)
{
    await Task.Delay(25, token); // the delay is now cancellable
}
