---
title: Prepare Device
sidebar_position: 1
---

## Android

### Prerequisites

- Workstation has **[Android Debug Bridge (adb)](https://developer.android.com/studio/releases/platform-tools)**
- You have a real Android device
- Your device has activated **[Debug mode](https://developer.android.com/studio/command-line/adb)** (wired or wi-fi)

### Steps
1. Connect your device to the Workstation (wired or wi-fi)
2. Check that your device is visible for your Workstation and take their name/ip: <br/>
	```bash
	> adb devices
	```

	![image](./02-run-localhost/adb-devices.jpg)

	Note: if you do not see your device, please look at the sources in the prerequisites
3. Now your devices is ready to work with PMetrium Native. You can run your first performance test for your native app

### Root device (optional)

- PMetrium Native may work with both Root and NOT Root devices
- PMetrium Native detects automatically whether the device has Root access or not
- Root devices allow PMetrium Native to collect more hardware metrics, see the full **[list of supported metrics](../architecture/03-development/05-pmetrium-metrics-android.md)** 

:::caution
- There are a lot of different Android devices on the market. It's worth mentioning that not all of them are possible to root.
- Please search the instruction on the internet for your specific device
:::

:::tip
There is an option to use an Android emulator, but we strongly recommend using only REAL devices.
:::