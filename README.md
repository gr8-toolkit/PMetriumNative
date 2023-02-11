## <img src="Assets/PMetriumNativeLogo.png" width="80" height="80"> PMetrium Native


**PMetriumNative** - a testing tool for measuring performance of mobile native applications as well as the system itself.

Please follow the **[DOCUMENTATION](https://parimatch-tech.github.io/PMetriumNative/)** to get detailed information about PMetrium Native. 

Key facts about the instrument:
- PMetrium Native works as web server on the host machine with direct connections to real devices. Therefor PMetrium Native provides you a [RESTful API](https://parimatch-tech.github.io/PMetriumNative/tools/pmetrium-native/architecture/development/pmetrium-api) for interactions
- PMetrium Native stores their [metrics](https://parimatch-tech.github.io/PMetriumNative/tools/pmetrium-native/architecture/development/pmetrium-metrics-android) in InfluxDB
- PMetrium Native visualizes their metrics with Grafana Dashboard, which is connected to InfluxDB
- PMetrium Native v.2.1 supports Android and IOS platforms
- PMetrium Native does not require integration into the source code of the native application

The idea is to measure performance of the native application and the system as simple as possible:

```shell
curl -G -d "device=192.168.0.103:5555" -d "applicationName=com.example.pmnative" http://localhost:7777/Android/Start

dotnet test .\src\PMetrium.Native\FunctionalTests  --filter ColdStart

curl -G -d "device=192.168.0.103:5555" http://localhost:7777/Android/Stop
```

Okey, no more words. Let's look at the live Demo:
- *Android:*

<img src="Assets/AndroidDemo.gif">

- *IOS:*

<img src="Assets/IOSDemo.gif">

**PackageRegistry** directory contains a single executable file for PMetrium Native for the most common OS platforms and architectures.

**Contributors are welcome and stay tuned!**



 