---
title: Database Diagram
sidebar_position: 2
---

```mermaid
    flowchart RL
    id2[InfluxDBSync C# class]:::csharpStyle --> |http| id1[InfluxDB]
    id4[Grafana] --> |http| id1
    id3[HardwareMetricsHandler C# class]:::csharpStyle --> id2
    id5[ApplicationMetricsHandler C# class]:::csharpStyle --> id2

    style id1 fill:#fcd303, stroke:#fca503, stroke-width:3px
    style id4 fill:#fcd303, stroke:#fca503, stroke-width:3px
    classDef csharpStyle fill:#fcd303, stroke:#fca503, stroke-dasharray: 10 5, stroke-width:2px;
```

### InfluxDB

InfluxDB is a time-series database that is used to store collected metrics in an appropriate
format.

:::important
**We use InfluxDB version 1.8.x not 2.x.x.** <br/>
The reason behind this is that InfluxDB 2.x.x supports only Flux query language and all our 
Grafana dashboards use older SQL-like language.
:::

### InfluxDBSync C# class

This class is used to send parsed metrics to InfluxDB.

### HardwareMetricsHandler C# class

This class is used to parse the obtained raw metrics from the phone into appropriate
InfluxDB format.

### ApplicationMetricsHandler C# class

This class is used to parse application events gathered from the device.

### Grafana

Grafana is used to visualize metrics stored in InfluxDB.
