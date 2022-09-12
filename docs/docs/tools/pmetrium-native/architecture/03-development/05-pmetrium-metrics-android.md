---
title: PMetrium Native Metrics (Android)
sidebar_position: 5
---

| **Metrics type** | **Metrics name** | **Unit**        | **Root**   | **Description**                                     |
|------------------|------------------|:---------------:|:----------:|-----------------------------------------------------|
| CPU              | cpuTotal         | %               | no         | Total system CPU usage                              |
| CPU              | cpuApp           | %               | no         | Application CPU usage                               |
| RAM              | ramTotal         | bytes(IEC)      | no         | Total system RAM usage                              |
| RAM              | ramApp           | bytes(IEC)      | no         | Application RAM usage: PSS, Private                 |
| Network          | networkApp       | bytes/sec(SI)   | yes        | Application network usage: received and transferred |
| Battery          | batteryApp       | mAh             | no         | Application battery usage                           |
| Frames           | framesApp        | count           | no         | Application frames: Total rendered, Total Janky     |


**Some notes:**
- *PSS RAM* - Proportional Set Size of RAM memory allocated for the application, is the amount of RAM actually mapped into the process, but weighted by the amount it's shared across processes
- *Private RAM* - Private memory allocated for the application, is composed of pages that are only used by the process
- *Janky frames* - number of frames that have been rendered for more than 16ms

:::tip
**Metrics name** from the table above corresponds to the **[PMetrium Native API](./04-pmetrium-api.md#start)** options
:::

:::important
While this page is mostly about hardware metrics there is an option to track also **[Application Events](./06-application-events.md)**
:::