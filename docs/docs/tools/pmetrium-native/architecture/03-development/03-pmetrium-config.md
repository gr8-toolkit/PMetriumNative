---
title: PMetrium Native Config
sidebar_position: 3
---

PMetrium Native Host project contains an **appsettings.json** file that allows you to modify multiple parameters needed for performance test execution.

```json
{
  "LogLevel": "Information",
  "InfluxDB": {
    "Url": "http://localhost:8086",
    "DataBaseName": "PMetriumNative",
    "User": "admin",
    "Password": "admin"
  },
  "WireMockPort": 7777,
  "AppEventsTimeout" : "00:00:00"
}
```

### `LogLevel`

Allows you to set the needed log level.<br/>
We have the following log levels available:
`Warning`, `Debug`, `Error`, `Fatal`, `Verbose`, and `Information`.

:::important
The default log level is set to **Information**.
:::

### `InfluxDB`

- `Url` <br/>
    Provide URL with port to your InfluxDB URL.

- `DataBaseName` <br/>
    Provide a database name that will be used to store collected metrics.

- `User` and `Password` <br/>
    Credentials that will allow to read\write metrics from\to InfluxDB.

:::important
If you will decide to change the database name, please, ensure that it will match with your Grafana dashboard datasource.
:::

### `WireMockPort`

PMetrium Native is based on WireMock as a web host instead of a regural web application, that's why this option in settings is called WireMockPort - the port on which the PMetrium Native Host will be running. There was a reason why we have chosen a WireMock, it allows us to run more smoothly new processes on the Workstation.

### `AppEventsTimeout`

Value responsible for the PMetrium Native to wait after the last functional step was executed.