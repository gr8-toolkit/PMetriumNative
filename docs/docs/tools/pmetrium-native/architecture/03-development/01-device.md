---
title: Device Diagram
sidebar_position: 1
---

```mermaid
    flowchart LR
    id0[Functional tests] --> id1[Workstation]
    id1 <--> |ADB/xctrace| id2[Android/IOS device]
    
    style id0 stroke-width:3px
    style id1 stroke-width:3px
    style id2 stroke-width:3px
```

### Android/IOS device

This device should be **physical Android/IOS phone** in order to get real metrics. Of course you can use emulators (only for Android) as well in debug purposes.

### Workstation     

The Workstation has already been covered in **[Workstation Diagram](./00-workstation.md)**.