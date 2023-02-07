---
title: Process Diagram
sidebar_position: 0
---

## Functional test Diagram

```mermaid
sequenceDiagram
    autonumber
    participant Functional tests
    participant Workstation
    participant Android/IOS device

    rect rgb(191, 223, 255)
    note right of Functional tests: Functional test flow

    Workstation ->>+ Functional tests : Request to start test
    Functional tests ->>- Workstation : Start test
    Workstation ->> Android/IOS device : Run test
    Android/IOS device ->> Functional tests : Test results

    end
```

## Functional test + PMetrium Native Diagram

```mermaid
sequenceDiagram
    autonumber
    participant Functional tests
    participant Workstation
    participant Android/IOS device

    rect rgb(144,238,144)
    note left of Android/IOS device: Start measurement

    Workstation ->> PMetrium Native : Request mesurement
    PMetrium Native ->>+ Workstation : Start measure
    Workstation ->>- Android/IOS device : measuring

    end

    rect rgb(191, 223, 255)
    note right of Functional tests: Functional test flow

    Workstation ->>+ Functional tests : Request to start test
    Functional tests ->>- Workstation : Start test
    Workstation ->> Android/IOS device : Run test
    Android/IOS device ->> Functional tests : Test result

    end

    rect rgb(255,165,165)
    note left of PMetrium Native: Stop measurement

    Workstation ->> PMetrium Native : Request measurement to stop
    PMetrium Native ->>+ Workstation : Stop measure
    Workstation ->> Android/IOS device : Get raw metrcis  
    Workstation ->>- PMetrium Native : Parse metrics   
    PMetrium Native ->> Database : Save metrics
    Visualization ->> Database : Visualize metrics


    end
```

## Components

### Functional tests

As was mentioned in the Logical diagram, the Functional tests serve as an entry point for our PMetrium Native framework.

### Workstation

Serve as a 'hub' connecting functional test and Android/IOS device that will execute the scenario.
Also, it serves as a 'hub' that collects saved metrics on a device, parses them, and sends them to a database.

### Android/IOS device

Real phone or emulator that is used for functional test execution. 

### PMetrium Native

Our framework is responsible for the performance test execution process. Also, it used to parse obtained raw metrics from the Android/IOS device
and save them in the Database.

### Database

We use a time-series database InfluxDb to save metrics gathered from the device and parsed by our PMetrium Native framework. 

### Visualization

The metrics we gathered should be visualized in a human-friendly format so it could be easy to track trends and observe possible anomalies in terms of the performance of our application.