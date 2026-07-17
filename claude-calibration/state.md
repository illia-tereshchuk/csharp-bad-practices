# State

_Snapshot; `dotnet run tools/next-id.cs` is authoritative for numbering._

- Exhibits: **9** | Halls: **6** | Next free id: **0010**
- Last updated after: #0009 (2026-07-17)

## Exhibits shipped

| id | hall | slug | level | archetype |
|--:|------|------|:--:|:--:|
| 0001 | collections | modify-while-enumerating | 🟢 | 2 |
| 0002 | numbers | doubles-for-money | 🟢 | 4 |
| 0003 | async | race-on-shared-counter | 🟢 | 6 |
| 0004 | collections | dictionary-key-mutation | 🟡 | 2 |
| 0005 | exceptions | throw-ex-stack-amnesia | 🟡 | 7 |
| 0006 | linq | closure-over-loop-variable | 🟢 | 1 |
| 0007 | async | async-void | 🟡 | 1 |
| 0008 | orm | n-plus-one | 🟡 | - |
| 0009 | linq | multiple-enumeration | 🟡 | 1,3 |

## Halls

- **Open (6):** collections, numbers, async, linq, exceptions, orm
- **Closed, planned:** datetime, strings-memory, di-lifetimes, security
- **Candidate new halls:** value-types, events, serialization

## Level mix

- 🟢 4 | 🟡 5 | 🔴 0  ->  next batches should introduce 🔴 flagships.

## Infra status

- `tools/next-id.cs` - live (counts folders, flags dup numbers, exit 1).
- Roadmap steps 8-10 (TOC generator, CI, polish) - NOT started. See `todo.md`.
