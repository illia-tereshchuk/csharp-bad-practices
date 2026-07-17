// Exhibit #0006: the fix

// Five lottery winners, five congratulation callbacks.
string[] winners = ["Olena", "Ivan", "Petro", "Maria", "Bohdan"];

var congratulations = new List<Action>();

for (int i = 0; i < winners.Length; i++)
{
    var winner = winners[i]; // fresh variable each iteration — this is what gets captured
    congratulations.Add(
        () => Console.WriteLine($"Congrats, {winner}! Your prize is on the way."));
}

Console.WriteLine($"Callbacks prepared: {congratulations.Count}");
Console.WriteLine("The ceremony begins:");

foreach (var congratulate in congratulations)
{
    congratulate();
}

Console.WriteLine("Five winners, five prizes, zero lawsuits.");
