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

### Flowchart

```mermaid
graph TD

start((Start host)) --> anySubLeft{Any submissions left?}
	anySubLeft -->|yes| copyWD[Copy files to workdir]
		copyWD --> createSandbox[Start worker in sandbox]
		createSandbox --> sendSubData[Send metadata to worker]
		sendSubData --> isTimeout{Is timeout?}
			isTimeout -->|yes| errTimeout[Log timeout error]
				errTimeout --> destroySandbox[Destroy sandbox]
				destroySandbox --> anySubLeft
			isTimeout -->|no| isInputRequested{Is input requested?}
				isInputRequested -->|yes| isInputAvailable{Is input available?}
					isInputAvailable -->|yes| sendInput[Send input to sandbox]
					isInputAvailable -->|no| generateInput[Generate input]
						generateInput --> sendInput
				sendInput --> updateTimeout[Update timeout]
				updateTimeout --> isTimeout
				isInputRequested -->|no| isResultReceived{Is result received?}
					isResultReceived -->|yes| logResults[Log results]
						logResults --> isFinalResults{Is final results received?}
							isFinalResults -->|yes| destroySandbox
							isFinalResults -->|no| updateTimeout
					isResultReceived -->|no| isTimeout
	anySubLeft -->|no| stop((Stop host))
```

### Sequence

```mermaid
sequenceDiagram

participant fs as Filesystem
participant host as Runner host
participant slave as Sandbox worker
participant sub as Submission library

host ->> slave: Start
host -->> slave: Send submission metadata
loop while avg runtime &lt; 100ms
	slave -->> host: Ask for input
	opt no input available
		host ->> host: Generate harder input
		host ->> fs: Save input
	end
	host ->> fs: Load input
	host -->> slave: Provide input
	loop n performance runs
		slave ->> sub: Load to context
		activate sub
		slave ->> sub: Warmup
		Note right of sub: easy input
		slave ->> sub: Measure
		Note right of sub: hard input
		deactivate sub
		slave ->> slave: Validate results
		slave -->> host: Partial results
	end
end
slave -->> host: Total results
host ->> slave: Destroy
```
