---
name: adr
description: "Lifecycle: author an Architecture Decision Record from the shared template, assigning the next sequential number, status and date, and updating the index. Use when recording or proposing an architectural or technical decision, documenting why a design choice was made, or capturing the trade-offs and alternatives behind one."
metadata:
  plugin: pc-lifecycle
  author: "Juan García Carmona"
---

One responsibility: record one significant decision as an Architecture Decision Record, so the *why* and the road not taken survive the people who were in the room.

**When to use:** a decision shapes the architecture, a dependency, a cross-cutting pattern, or anything hard to reverse: anything a future contributor would want the reasoning for.

**When not to use:** a routine implementation choice with no lasting trade-off, or a decision already captured in the change's own plan artifact. In doubt, a short record is cheap; prefer writing one over losing the rationale.

## Steps

The template is the single source of the document's shape. Copy it; never hand-roll the sections.

1. **Derive the title.** A short, present-tense phrase naming the decision ("Store sessions in the cache tier", not "Sessions"). It becomes the heading.
   - Done when: the title names a decision, not a topic.
2. **Locate the records directory.** Default `docs/adr/`. Absent → ask where the project keeps decision records rather than creating a directory the team did not ask for.
   - Done when: the directory is known and exists.
3. **Compute the next number.** Take the highest existing four-digit prefix and add one, zero-padded. Numbers only ever increase and are never reused, not even a retired one.
   - Done when: the number is unique and next in sequence.
4. **Slug the filename.** Lowercase the title, hyphenate spaces, drop punctuation: `NNNN-<slug>.md`.
5. **Fill the template** from [`references/template.md`](references/template.md). Complete every section: Context (neutral facts, why a decision is needed), Decision (active voice, "We will …"), Consequences (positive, negative, neutral), Alternatives considered (each with a one-line reason for rejection), References.
   - Done when: no placeholder text remains anywhere in the document.
6. **Set the metadata.** Status `Proposed` and today's date for a new record, unless the human states the decision is already accepted. Fill the tags with the areas it touches.
7. **Write the file**, then **update the index**: a row carrying the number, the title linked to the file, the status and the date, keeping the table ordered by number. No index in the project → say so rather than inventing one.
8. **Report** the created path and the assigned number.

## Status vocabulary

Drawn from a fixed set. Never invent a value.

| Status | Meaning |
| --- | --- |
| `Proposed` | Under discussion; the default for a new record |
| `Accepted` | Agreed and in effect |
| `Rejected` | Considered and declined, kept for the record |
| `Deprecated` | No longer applies, and not replaced |
| `Superseded by ADR-NNNN` | Replaced by a later decision; link it |

Records are immutable in spirit. To reverse a decision, write a new record and mark the old one superseded, never rewrite history. The date records when the status last changed.

## Before finishing, confirm

1. The filename carries a zero-padded, unique, next-in-sequence number.
2. The document has its heading, its status/date/tags block, and all five sections in order, with no leftover placeholders.
3. The status is a valid value and the date is `YYYY-MM-DD`.
4. Alternatives considered names at least the main rejected option, with a reason: this is what separates a decision record from a commit message.
5. The index has a matching row.
