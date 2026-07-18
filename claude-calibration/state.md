# State

_Snapshot; `dotnet run tools/next-id.cs` is authoritative for numbering._

- Exhibits: **13** | Halls: **9** | Next free id: **0014**
- Last updated after: #0013 (2026-07-18)

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
| 0010 | events | immortal-subscriber | 🔴 | 6 |
| 0011 | value-types | defensive-copy-ambush | 🔴 | 3 |
| 0012 | serialization | zero-priced-order | 🟡 | 4,5 |
| 0013 | linq | distinct-that-didnt | 🟡 | 2,4 |

## Halls

- **Open (9):** collections, numbers, async, linq, exceptions, orm, events, value-types, serialization
- **Closed, planned:** datetime, strings-memory, di-lifetimes, security

## Level mix

- 🟢 4 | 🟡 7 | 🔴 2  ->  healthy; keep the spread.

## Infra status

- `tools/next-id.cs` - live (counts folders, flags dup numbers, exit 1).
- Roadmap steps 8-10 (TOC generator, CI, polish) - NOT started. See `todo.md`.
