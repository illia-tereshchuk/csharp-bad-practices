// Exhibit #0006: a closure capturing the loop variable

// Five lottery winners, five congratulation callbacks.
string[] winners = ["Olena", "Ivan", "Petro", "Maria", "Bohdan"];

var congratulations = new List<Action>();

for (int i = 0; i < winners.Length; i++)
{
    congratulations.Add(
        () => Console.WriteLine($"Congrats, {winners[i]}! Your prize is on the way."));
}

Console.WriteLine($"Callbacks prepared: {congratulations.Count}");
Console.WriteLine("The ceremony begins:");

foreach (var congratulate in congratulations)
{
    congratulate(); // 💥 every callback reads i as it is NOW — and now it's 5
}
