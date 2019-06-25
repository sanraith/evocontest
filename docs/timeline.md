# evowar 2019 timeline

## Introduction
This document contains information about the forecasted timeline of the contest.

## Timeline
```mermaid
gantt
title Development and running of the contest
dateFormat YYYY-MM-DD

section Misc
    now: now, 0d
    Summer vacation: vacation, 2019-08-29, 5d

section Challenge
    Find a good challenge: active, challengeFind, 2019-06-25, 2d
    Solve varying time problem: challengeTime, after challengeFind, 2d
    Alpha test: after challengeTime, 3d

section Web Logic
    Setup repo: done, 2019-06-18, 2d
    Create skeleton: webSkeleton, after now, 1d

    Setup azure server: webAzureSetup, after webSkeleton, 1d
    Setup DB engine: webPickDb, after webAzureSetup, 1d
    Setup login: webLogin, after webSkeleton, 2d
    Testability (login, test data): after webLogin, 1d

    Create EF + FS model: webModel, after webSkeleton, 1d
    
    Create work distributor: webDistributor, after webModel, 2d
    Daily runner job: 1d

    Activate site: crit, webActivate, 2019-09-08, 2h
    Make public repo: after webActivate, 2h

section Web Content
    Admin page: after webModel, 2d
    Submitter page: webSubmitter, after webDistributor, 3d
    Daily standings page: after webSubmitter, 2d
    Personal stats page: 2d

    Wiki challenge: after challengeFind, 1d
    Wiki envirnoment: after subRepo, 1d
    Wiki solution: 1d
    Wiki submission: webWiki, after subPackager, 1d
    Wiki FAQ: 1d

    Final standings page: 1d

section Submission
    Setup repo: subRepo, after now, 1d
    Add sample: subSample, after subRepo, 1d
    Add runner: after subRepo, 2d
    
    Add tests: after subSample, 1d
    Add packager: subPackager, after subSample, 1d
    Add submitter: after subPackager, 1d
    
    Decide requirements: 1d

    Make repo public: crit, after webActivate, 2h

section Raspberry FW
    Create sandbox: rpiSandbox, after now, 1d
    Safely communicate w/ sandbox: rpiSandComm, after rpiSandbox, 1d
    Test data generator: 1d
    Test runner: rpiRunner, after challengeTime, 2d
    Long polling worker: after rpiRunner, 1d
    Contest runner app: 3d

section Contest
    Promo email: 2019-09-05, 1h
    Kickoff meeting: contestKickoff, 2019-09-09, 2h
    Receiving submissions: contestRunning, after contestKickoff, 11d
    Midtime email: 2019-09-16, 1h
    Finals: after contestRunning, 1d
    Summary email: 2019-09-23, 1h
```
