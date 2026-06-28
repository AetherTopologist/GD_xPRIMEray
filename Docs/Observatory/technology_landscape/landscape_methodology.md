# Technology Landscape Methodology

Version: v0.1

## Evidence-First Comparison

Every status statement must point to inspectable evidence: a revision document, schema, report, fixture, retained artifact, or authoritative external source. Repository evidence supports claims only within the scope stated by that artifact.

Use this sequence:

1. Define the capability in neutral terms.
2. Cite the current repository evidence.
3. Assign maturity from the ladder below.
4. State unknowns and claim boundaries.
5. Name comparable technology categories only when they clarify interfaces or terminology.
6. Record the next evidence-producing milestone.

Absence of evidence is recorded as unknown, planned, or not demonstrated. It is never converted into a positive or negative product judgment.

## Capability Maturity

Maturity describes the strength and accessibility of capability evidence, not quality or market position. A capability advances only when evidence satisfies the next level.

| Level | Evidence expectation |
|---|---|
| Vision | intent or design direction is documented |
| Prototype | a narrow implementation or artifact demonstrates the basic shape |
| Experimental | repeatable project-local workflows explore behavior and limitations |
| Internal Validation | explicit checks and retained evidence support bounded internal conclusions |
| Public Demo | a curated, reproducible presentation has claim-safe supporting evidence |
| Research Ready | methods, provenance, uncertainty, and results are independently reviewable and repeatable |
| Production Ready | supported interfaces, operational controls, compatibility policy, and maintained release evidence exist |

Maturity applies per capability. The project does not receive one blanket maturity label.

## Claim Boundaries

Landscape entries may describe capability presence, contract coverage, evidence availability, known gaps, and planned milestones. They must not infer:

- numerical performance without a declared benchmark protocol
- renderer, runtime, transport, or physical equivalence without direct evidence
- parity from shared schemas, matching metadata, or similar images
- superiority, ranking, or fitness for an unstated use case
- scientific validation from a public demo or internal artifact
- compatibility with an external technology from conceptual similarity alone

Terms such as `comparable` refer to a meaningful shared comparison basis, not equal results or quality.

## Source Citation Expectations

Repository claims should use relative links to the nearest primary artifact and name the relevant Project Glowing Heart revision when one exists. External technology descriptions should cite official documentation, standards bodies, or primary publications. Secondary summaries may provide orientation but should not carry a technical claim when a primary source is available.

A citation should support the exact sentence or table cell in which it appears. Record versions, dates, fixture IDs, and artifact paths when they affect interpretation. Clearly label static inspection, generated reports, runtime evidence, and inferred conclusions as different evidence classes.

Links are not evidence by themselves: the cited material must contain the stated support.

## Revision History

| Landscape revision | Date | Change | Evidence basis |
|---|---|---|---|
| v0.1 | 2026-06-27 | Created renderer, optics, adapter, research, methodology, and capability views | Project Glowing Heart revisions v0.1 through v1.9 |
| 001 | 2026-06-27 | Expanded renderer landscape with field-guide entries for engines, path tracers, and traversal frameworks | [v1.8 milestone](../../xPRIMEray/project_glowing_heart_v1_8_milestone.md), [v1.9 snapshot contract](../../xPRIMEray/project_glowing_heart_v1_9_shared_snapshot_measurement_contract.md), architecture vision |

Future revisions should append a row rather than rewrite history silently. Status changes in the [Capability Matrix](capability_matrix.md) should identify the revision or artifact that justified the change.
