---
title: Intro to PMetrium Native
sidebar_position: 1
---

## Problem

Initially, there was no performance testing of mobile native applications in PariMatch.

The functionality of our application is constantly growing, with more and more features. Assuming this can affect the application's performance, we decided to find a way to test it.

***For PoC we've decided to start with our Android application due to multiple reasons:***
- Android devices are much more common than iOS-based;
- Android is a more open architecture;
- Android does not require MacOS to operate with emulator\real device.

### Challenges

- The biggest challenge was that there are no industry-wide solutions for performance testing of mobile native applications like JMeter or Sitespeed.io.
- Absence of information with a complex approach to conducting such testing.
- There are tools to automate tests on mobile devices but such tools do not provide any functionality for performance testing.

## Requirements
- the ability to work with both Android and iOS devices
- ability to work without manual actions to gather metrics
- easy to integrate into existing functional autotests, no need to rewrite tests
- stability and reliability
- easy metrics access, well structured and visualized reports

## Solution
### Overview

After multiple various options tried, we've come up with the final approach on how we will conduct such testing.

- We developed a cross-platform tool - PMetrium Native
- PMetrium Native works as a web host and has some endpoints 
- PMetrium Native does not require any additional functional test settings
- PMetrium Native can be used together with autotests or with manual interactions
- PMetrium Native can gather both hardware and application metrics
- PMetrium Native can work with real devices and emulators
- PMetrium Native based on shell scripts to interact and gather metrics from the Android device
- PMetrium Native based on xctrace cli tool from xcode to interact and gather metrics from the IOS device
- We use InfluxDB to write metrics and Grafana to visualize them.

### Pros

- The main benefit of such an approach is that we have a pretty decent amount of versatility when it comes to what measure and how.
- Also, due to the code-based solution, it's possible to extend our solution with additional functionality if needed.
- Existed functional autotests are not required to be changed

### Cons

- Since we do not use any ready solution, it might be hard for any new person to understand how our solution works, but we try to keep it as simple as possible :)
- Most of the metrics we gather for Android devices, are being gathered from the shell scripts
- To track some events and their timestamps you have to add some code on the application side, which requires extra work for you
- Shell scripts execution for Android utilizes device resources as well.

### Why PMetrium Native

If you wonder what **PM** means, it's an abbreviation from the first names of ***Pavlo Maikshak*** and ***Mykola Panasiuk*** responsible for the creation of this framework. Also, we would like to mention ***Anton Boyko*** who greatly assisted us 🙂.

## Alternative approaches:
### 1. Apptim

#### Overview

This is a paid solution with GUI available.
It allows to record the tests directly from GUI, use Appium tests, etc. User actions time is being collected by adding custom Apptim timestamps with start and finish.
Possible integration with CI\CD and either Apptim device cloud or your own device farm.
Allows collecting hardware metrics out of the box.

#### Pros

1. Out of the box solution that is ready to be used for performance testing.
2. Multiple integrations available.
3. Hardware metrics are being gathered without any additional setup.
4. Available both for Android and iOS devices.

#### Cons

1. It's a paid solution and if you want to utilize your own device farm, you need to subscribe to the most expensive subscription option.
2. While hardware metrics are being gathered out-of-the-box, it's not clear if they are being gathered from the device itself or from a running application.
3. Apptim support does not answer emails and in their online chat.
4. Inconsistency between metrics taken from MacOS and Windows 10 for the same Android application.

#### Why was rejected

Due to the cons we mentioned above. We've considered Apptim as a serious option to consider but they ignored our questions written in email and their chat, which made us drop this option.
Also, we weren't happy that to utilize our own device farm, we need the most expensive subscription.

### 2. NeoLoad

#### Overview

NeoLoad states on its website that it could use for performance testing of mobile applications.

#### Why was rejected

NeoLoad application download link was blocked for Ukrainian customers and support totally ignored our email.

### 3. AppSpector

The tool for measuring the performance metrics from the application in real time for both IOS and Android. But the reason we gave up with AppSpector was the required integration into the source code of the application. 
