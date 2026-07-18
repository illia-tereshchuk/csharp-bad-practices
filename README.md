# C# Bad Practices

Ideal tutorials teach you SOLID. **Bad** code is remembered better.

> Museum stats: **11** exhibits in **8** halls, latest addition - **#0011**.

### 🗂 Collections

| | | | |
|--:|---|---|---|
| 0001 | [Modifying a collection while iterating](src/collections/0001-modify-while-enumerating/) | 🟢 | Never modify a collection while iterating it |
| 0004 | [Mutating a dictionary key](src/collections/0004-dictionary-key-mutation/) | 🟡 | Never mutate an object that serves as a dictionary key |

### 🔢 Numbers

| | | | |
|--:|---|---|---|
| 0002 | [Calculating money with double](src/numbers/0002-doubles-for-money/) | 🟢 | Never use `double` for money |

### ⚡ Async & Threading

| | | | |
|--:|---|---|---|
| 0003 | [Incrementing a shared counter from parallel threads](src/async/0003-race-on-shared-counter/) | 🟢 | Never mutate shared state without synchronization |
| 0007 | [async void and the uncatchable exception](src/async/0007-async-void/) | 🟡 | Never write `async void` outside event handlers |

### 💥 Exceptions

| | | | |
|--:|---|---|---|
| 0005 | [Rethrowing with throw ex](src/exceptions/0005-throw-ex-stack-amnesia/) | 🟡 | Never rethrow with `throw ex` - use bare `throw` |

### 🔗 LINQ & Lambdas

| | | | |
|--:|---|---|---|
| 0006 | [A closure capturing the loop variable](src/linq/0006-closure-over-loop-variable/) | 🟢 | Never close over a loop variable - capture a copy |
| 0009 | [Enumerating a LINQ query twice](src/linq/0009-multiple-enumeration/) | 🟡 | Never enumerate a LINQ query twice - materialize it once |

### 🗄 ORM

| | | | |
|--:|---|---|---|
| 0008 | [The N+1 query problem](src/orm/0008-n-plus-one/) | 🟡 | Never query the database inside a loop |

### 🔔 Events

| | | | |
|--:|---|---|---|
| 0010 | [A static event that never lets go](src/events/0010-immortal-subscriber/) | 🔴 | Never subscribe to a long-lived event without unsubscribing |

### 📦 Value Types

| | | | |
|--:|---|---|---|
| 0011 | [A mutable struct behind a readonly field](src/value-types/0011-defensive-copy-ambush/) | 🔴 | Never write a mutable struct |

# To Be Continued

More halls under construction: **datetime**,
**strings & memory**, **DI lifetimes**, **security**.
