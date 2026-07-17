// Exhibit #0005: the fix

// A payment dies somewhere deep. Whoever is on call gets... this trace.
try
{
    ProcessPayment();
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("The trace the on-call engineer sees at 3 AM:");
    Console.WriteLine(ex.StackTrace);
    Console.WriteLine();

    if (ex.StackTrace?.Contains(nameof(ValidateCard)) != true)
    {
        throw new InvalidOperationException(
            $"The trace lost the crime scene: {nameof(ValidateCard)} is not in it");
    }

    Console.WriteLine($"Trace still points at {nameof(ValidateCard)} - debuggable.");
}

void ProcessPayment()
{
    try
    {
        ChargeCard();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[log] payment failed: {ex.Message}");
        throw; // rethrow the exception as it is, trace intact
    }
}

void ChargeCard() => ValidateCard();

void ValidateCard() => throw new ArgumentException("Card number failed the Luhn check");
