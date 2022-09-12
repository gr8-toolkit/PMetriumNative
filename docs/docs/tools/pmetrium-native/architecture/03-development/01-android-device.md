---
title: Android Device Diagram
sidebar_position: 1
---

```mermaid
    flowchart LR
    id0[Functional tests] --> id1[Workstation]
    id1 <--> |ADB| id2[Android device]
    id2 --- id3[Rooted/Non-rooted]
    
    style id0 stroke-width:3px
    style id1 stroke-width:3px
    style id2 stroke-width:3px
    style id3 stroke-dasharray: 10 5, stroke-width:2px

```

### Android device

This device should be **physical android phone, not an emulator**. Specifications of the
exact mobile phones, for now, are out of scope.

### Rooted/Non-rooted

We reccomend to work with rooted devices to provide full access to hardware metrics gathering, but it's OK to work with a regular android phone without root. PMetrium Native is able to automatically determine if the phone has root.

:::important
If your device is not rooted you will be able to collect almost all metrics, listed **[here](./05-pmetrium-metrics-android.md)**. <br/>
**The not rooted device won't be able to provide network metrics.**
:::

### Workstation     

The Workstation has already been covered in **[Workstation Diagram](./00-workstation.md)**.