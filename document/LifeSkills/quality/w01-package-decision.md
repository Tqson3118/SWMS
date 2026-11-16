# W01 Package Decision Log

**Date:** 2026-09-15  
**Status:** Blocked pending contract decision; no manifest created.

## Canonical content paths

- Lesson: `LifeSkills/notes/module-1/week-01-self-awareness.md`
- Quiz: `LifeSkills/assessments/quizzes/week-01-quiz.md`
- Answer key: `LifeSkills/assessments/answer-keys/week-01-answers.md`
- Flashcards: `LifeSkills/flashcards/week-01.yaml`
- Ledger: `LifeSkills/quality/module-1-source-ledger.md`

## Stable IDs retained

- Questions: `Q01-01` through `Q01-08`
- Cards: `fc-ls-w01-001` through `fc-ls-w01-005`
- Claim IDs: `W01-C01`, `W01-C02`

## Unresolved deck ID contract

Two repository documents conflict:

- R5 flashcard schema and validator require the pattern `deck-{subject}-{unit}`, so the validated deck currently uses `deck-ls-w01`.
- Application content contract §3 shows `roadmap:deck:{subject_slug}:{unit_slug}` for `deck_id`; manifest artifact stable IDs also use `roadmap:deck:...`.

The W01 deck passed the current Python validator with `deck-ls-w01`. It is not safe to change it to the colon-form ID without an explicit contract decision because that would fail the current R5 schema and could change application mapping. It is also not safe to create a manifest while the two representations are unresolved.

## Package gate

`LifeSkills/manifest.yaml` is intentionally absent. Creating it requires an owner decision for the deck ID mapping and a final raw-byte hash after that decision. No importer, database, or web readiness claim is made.
