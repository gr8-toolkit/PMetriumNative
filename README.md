## <img src="Assets/PMetriumNativeLogo.png" width="80" height="80"> PMetrium Native


**PMetriumNative** - a testing tool for measuring performance of mobile native applications as well as the system itself.

Please follow the **[DOCUMENTATION](README.md)** (to be done in a few days) to get detailed information about PMetrium Native. 

Key facts about the instrument:
- PMetrium Native works as web server on the host machine with direct connections to real devices. Therefor PMetrium Native provides you a RESTful API for interactions
- PMetrium Native stores their metrics in InfluxDB
- PMetrium Native visualizes their metrics with Grafana Dashboard, which is connected to InfluxDB
- PMetrium Native v.1.0.0 supports Android platform so far (IOS platform will appear in v.2.0.0)
- PMetrium Native does not require integration into the source code of the native application

The idea is to measure performance of the native application and the system as simple as possible:

```shell
curl -G -d "device=192.168.0.103:5555" -d "app=com.parimatch.ukraine" http://localhost:7777/Start

dotnet test .\src\PMetrium.Native\FunctionalTests  --filter ColdStart

curl -G -d "device=192.168.0.103:5555" http://localhost:7777/Stop
```

And to get the results in Grafana:<br>

<img src="Assets/PMNGrafana_1.jpg">

Package directory contains a single executable file for PMetrium Native for the most common OS platforms and architectures.

**Contributors are welcome and stay tuned!**



 