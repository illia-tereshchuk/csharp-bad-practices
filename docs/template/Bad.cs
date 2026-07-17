// Exhibit #XXXX: name the crime being committed

// 1. Arrange a tiny believable domain - an OrderService beats a Foo.
//    Keep it under ~100 lines total.

// 2. Commit the crime. The run MUST end in visible failure:
//    an exception, an obviously wrong number, a demonstrable hang.
//    Flaky bugs (races) must be provoked in a loop until they fire reliably.

Console.WriteLine("Before: everything looks fine...");

throw new NotImplementedException("Plant the bomb here 💥");
