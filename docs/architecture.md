# evowar 2019 architecture

## Introduction
This document contains information about the general architecture of the components required to run the contest.

# Overview
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

# Web

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

# Raspberry
```mermaid
graph TD

subgraph Azure
	cloud[evowar BE]
end

subgraph Raspberry
	host[Runner.Host]
	subgraph Sandbox
		slave[Runner.Slave]
		subgraph Assembly Load Context
			dll[Submitted library]
		end
	end
end

host -->|authenticated http| cloud
host -->|named pipe| slave
slave --> dll
```

## Run submission checks

```mermaid
sequenceDiagram

participant cloud as evowar BE
participant host as Runner host
participant slave as Sandbox worker
participant sub as Submission library

host ->> cloud: Download submissions
loop for each Submission
	host ->> slave: Create
	activate slave
	Note over host, slave: Named pipe connection

	slave ->> sub: Load to context
	activate sub
	slave -->> sub: Static analysis
	slave -->> host: Static analysis results
	slave ->> sub: Unit tests
	deactivate sub
	slave -->> host: Test results
	deactivate slave
end
host ->> cloud: Upload results
```

## Run performance measurements

```mermaid
sequenceDiagram

participant host as Runner host
participant fs as Filesystem
participant slave as Sandbox worker
participant sub as Submission library

loop while avg runtime &lt; 100ms
	opt no input available
		slave -->> host: Status message
		slave ->> slave: Generate harder input
		slave ->> fs: Save input
		Note over fs: Encrypted files
	end
	slave ->> fs: Load input
	loop n performance runs
		slave ->> sub: Load to context
		activate sub
		slave ->> sub: Warmup
		Note right of sub: easy input
		slave ->> sub: Measure
		Note right of sub: hard input
		deactivate sub
		slave ->> slave: validate results
		slave -->> host: Partial results
	end
end
slave -->> host: Total results
```
