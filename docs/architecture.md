# evowar 2019 architecture

## Introduction
This document contains information about the general architecture of the components required to run the contest.

## Overview
```mermaid
graph LR

subgraph Any client
	C(Web Browser)
end

subgraph Raspberry
	RPI(Reference System worker)
end

subgraph Azure
	FE[evowar FE]
	BE[evowar BE]
	DB((Database))
	FS((Filesystem))
end

C --> FE
FE --> BE
BE --> DB
BE --> FS
RPI --> BE
```

## Submitting a solution

After submitting a solution, a static analysis is performed on the server, and tests are run on the reference system. 

```mermaid
sequenceDiagram

participant client
participant FE as evowar FE
participant DB as DB & FS
participant BE as evowar BE
participant raspberry

client ->> FE:Submit solution
FE -x BE: Save solution
BE ->> BE: Static analysis
BE ->> DB: Save solution and metadata
BE -->> client: Push analysis results (fast)

loop long polling
	Note over BE, raspberry: Secured connection
	raspberry ->> BE: Request work
	BE ->> DB: Get solution and metadata
	raspberry ->> raspberry: Run tests
	raspberry -x BE: Submit test results
	BE ->> DB: Save results
end
BE -->> client: Push test results (slow)
```
