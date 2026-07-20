# Add an exhibit

One evening, one exhibit, one merged pull request. If you have never opened a PR
before, this is a good first one - the work is small, self-contained, and you
end up with your username on the front page next to the rule you added.

## What you need

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Claude Code](https://claude.com/claude-code)
- Push access to this repo - ask the maintainer, it takes him one click

## How it goes

1. Clone the repo and run `claude` inside the folder.
2. Tell it: **"I want to add an exhibit."**

That is the whole setup. From there Claude checks your tooling, shows you a menu
of C# mistakes waiting to be built, drafts the first version with you, runs
everything to prove the bug is real, and walks you to the pull request - command
by command if it is your first.

**Pick the one you like most and feel most sure about.** You will be explaining
it to other developers, so choose a mistake you understand or genuinely want to
understand. That comes through in the writing.

## Two things to know

- **Your bug must actually run.** Every exhibit is a program that fails when you
  run it, next to a fixed version that does not. Claude checks this with you
  before the PR.
- **Every exhibit is reviewed personally.** The maintainer may ask for changes or
  decline an idea. That is normal for a curated collection and says nothing
  about your code.

Questions go in the issue tracker.
