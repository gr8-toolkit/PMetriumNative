---
title: Metrics
sidebar_position: 5
---

### Android Metrics

| **Metrics type** | **Metrics name** |    **Unit**     | **Description**                                     |
|------------------|------------------|:---------------:|-----------------------------------------------------|
| CPU              | cpuTotal         |        %        | Total system CPU usage (0-100%)                     |
| CPU              | cpuApp           |        %        | Application CPU usage                               |
| RAM              | ramTotal         |   bytes(IEC)    | Total system RAM usage                              |
| RAM              | ramApp           |   bytes(IEC)    | Application RAM usage: PSS, Private                 |
| NETWORK          | networkTotal     |  bytes/sec(SI)  | Total system usage: received and transferred        |
| NETWORK          | networkApp       |  bytes/sec(SI)  | Application network usage: received and transferred |
| BATTERY          | batteryApp       |       mAh       | Application battery usage                           |
| FRAMES           | framesApp        |      count      | Application frames: Total rendered, Total Janky     |

**Some notes:**

- *PSS RAM* - Proportional Set Size of RAM memory allocated for the application, is the amount of RAM actually mapped
  into the process, but weighted by the amount it's shared across processes
- *Private RAM* - Private memory allocated for the application, is composed of pages that are only used by the process
- *Janky frames* - number of frames that have been rendered for more than 16ms

### IOS Metrics

| **Metrics type** | **Metrics name**               |    **Unit**    | **Description**                             |
|------------------|--------------------------------|:--------------:|---------------------------------------------|
| CPU              | CPU - Total                    |       %        | Total system CPU usage (may be > 100%)      |
| CPU              | CPU - App                      |       %        | Application CPU usage (may be > 100%)       |
| RAM              | RAM - Total Used               |   bytes(IEC)   | Total system RAM usage                      |
| RAM              | RAM - App                      |   bytes(IEC)   | Application RAM usage                       |
| DISK             | DISK - Read                    | bytes/sec(IEC) | Data read from disk per second              |
| DISK             | DISK - Write                   | bytes/sec(IEC) | Data written to disk per second             |
| DISK             | DISK - Read ops                |    ops/sec     | Operations read from disk per second        |
| DISK             | DISK - Write ops               |    ops/sec     | Operations written to disk per second       |
| NETWORK          | NETWORK - Received             | bytes/sec(IEC) | Network data received by the system         |
| NETWORK          | NETWORK - Sent                 | bytes/sec(IEC) | Network data sent by the system             |
| NETWORK          | NETWORK - App Received         | bytes/sec(IEC) | Network data received by the application    |
| NETWORK          | NETWORK - App Sent             | bytes/sec(IEC) | Network data sent by the application        |
| NETWORK          | NETWORK - Packets Received     |  packets/sec   | Network packets received by the system      |
| NETWORK          | NETWORK - Packets Sent         |  packets/sec   | Network packets sent by the system          |
| NETWORK          | NETWORK - Packets App Received |  packets/sec   | Network packets received by the application |
| NETWORK          | NETWORK - Packets App Sent     |  packets/sec   | Network packets sent by the application     |
| FPS              | FPS                            |      int       | Frames per second                           |
| GPU              | GPU - Utilization              |       %        | GPU utilization percentage                  |
