# C# Mistakes Explained

> Museum stats: **23** exhibits in **11** halls, latest addition - **#0023**.

### 🗂 Collections

| | | | |
|--:|---|---|---|
| 0001 | [Modifying a collection while iterating](src/collections/0001-modify-while-enumerating/) | 🟢 | never modify a collection while iterating it |
| 0004 | [Mutating a dictionary key](src/collections/0004-dictionary-key-mutation/) | 🟡 | never mutate an object that serves as a dictionary key |

### 🔢 Numbers

| | | | |
|--:|---|---|---|
| 0002 | [Calculating money with double](src/numbers/0002-doubles-for-money/) | 🟢 | never use `double` for money |

### ⚡ Async & Threading

| | | | |
|--:|---|---|---|
| 0003 | [Incrementing a shared counter from parallel threads](src/async/0003-race-on-shared-counter/) | 🟢 | never mutate shared state without synchronization |
| 0007 | [async void and the uncatchable exception](src/async/0007-async-void/) | 🟡 | never write `async void` outside event handlers |
| 0016 | [A cancellation token nobody reads](src/async/0016-token-tourism/) | 🟡 | never accept a token you don't pass down or check |
| 0018 | [Tasks mistaken for results](src/async/0018-tasks-are-not-results/) | 🟡 | never mistake a collection of tasks for a collection of results |
| 0019 | [The fire-and-forgotten task](src/async/0019-forgotten-task/) | 🟡 | never drop a Task - await it or hand it to someone who will |
| 0021 | [await Task.WhenAll shows only the first fault](src/async/0021-whenall-hides-exceptions/) | 🔴 | never trust await Task.WhenAll to report more than one failure |

### 💥 Exceptions

| | | | |
|--:|---|---|---|
| 0005 | [Rethrowing with throw ex](src/exceptions/0005-throw-ex-stack-amnesia/) | 🟡 | never rethrow with `throw ex` - use bare `throw` |
| 0015 | [A catch-all that swallows cancellation](src/exceptions/0015-cancellation-eaten-by-catch/) | 🟡 | never let a catch-all eat OperationCanceledException |
| 0017 | [A finally block that throws](src/exceptions/0017-finally-that-lied/) | 🔴 | never let a finally block throw |

### 🔗 LINQ & Lambdas

| | | | |
|--:|---|---|---|
| 0006 | [A closure capturing the loop variable](src/linq/0006-closure-over-loop-variable/) | 🟢 | never close over a loop variable - capture a copy |
| 0009 | [Enumerating a LINQ query twice](src/linq/0009-multiple-enumeration/) | 🟡 | never enumerate a LINQ query twice - materialize it once |
| 0013 | [Distinct on a class without value equality](src/linq/0013-distinct-that-didnt/) | 🟡 | never dedupe objects that don't define equality |

### 🗄 ORM

| | | | |
|--:|---|---|---|
| 0008 | [The N+1 query problem](src/orm/0008-n-plus-one/) | 🟡 | never query the database inside a loop |

### 🔔 Events

| | | | |
|--:|---|---|---|
| 0010 | [A static event that never lets go](src/events/0010-immortal-subscriber/) | 🔴 | never subscribe to a long-lived event without unsubscribing |
| 0023 | [Unsubscribing from an event with a lambda](src/events/0023-unremovable-lambda/) | 🟡 | never unsubscribe with a lambda - name the handler |

### 📦 Value Types

| | | | |
|--:|---|---|---|
| 0011 | [A mutable struct behind a readonly field](src/value-types/0011-defensive-copy-ambush/) | 🔴 | never write a mutable struct |

### 💉 DI Lifetimes

| | | | |
|--:|---|---|---|
| 0014 | [Transient disposables resolved from the root container](src/di-lifetimes/0014-container-hoarder/) | 🔴 | never resolve transient disposables from the root container |
| 0022 | [A singleton that captured a scoped dependency](src/di-lifetimes/0022-the-captive-scoped/) | 🔴 | never inject a scoped service into a singleton |

### 📄 Serialization

| | | | |
|--:|---|---|---|
| 0012 | [A JSON payload that maps to nothing](src/serialization/0012-zero-priced-order/) | 🟡 | never deserialize without pinning the naming contract |

### 📅 Datetime

| | | | |
|--:|---|---|---|
| 0020 | [A billing day that shrinks](src/datetime/0020-shrinking-billing-day/) | 🟡 | never compute the next date from the previous one - keep the anchor |

# To Be Continued

More halls under construction: **strings & memory**, **security**.
