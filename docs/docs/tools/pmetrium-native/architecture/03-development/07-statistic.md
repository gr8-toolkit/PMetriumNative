---
title: Measurement statistic
sidebar_position: 7
---

After each successful run PMetrium Native returns a response from `PLATFORM/Stop` endpoint with some statistic taken towards all metrics from this run.

This statistic from the response may be used in CI/CD for validation of the test results and taking some actions accordingly.

## Statistic for Android

Example: 

```json
{
  "events": [
    "[START] CPU load 1672234220013",
    "[END] CPU load 1672234226371",
    "RAM create usage 1672234227710",
    "RAM created 1672234228681",
    "RAM create usage 1672234230826",
    "RAM created 1672234231519",
    "[START] Network activity 1672234233566",
    "[END] Network activity 1672234242269",
    "Show GIF 1672234243763",
    "Hide GIF 1672234246966"
  ],
  "complexEvents": [
    {
      "timestamp": "2022-12-28T13:30:20.013Z",
      "name": "CPU load",
      "latency": 6358
    },
    {
      "timestamp": "2022-12-28T13:30:33.566Z",
      "name": "Network activity",
      "latency": 8703
    }
  ],
  "cpu": {
    "totalCpu_percentage": {
      "avg": 43.05,
      "min": 27.38,
      "max": 69.25,
      "p50": 42.5,
      "p75": 49.75,
      "p80": 53.12,
      "p90": 56.38,
      "p95": 62.5,
      "p99": 69.25
    },
    "applicationCpu_percentage": {
      "avg": 11.87,
      "min": 0,
      "max": 19.5,
      "p50": 14.37,
      "p75": 15.25,
      "p80": 16,
      "p90": 16.5,
      "p95": 18,
      "p99": 19.5
    }
  },
  "ram": {
    "systemRam_bytes": 5840543744,
    "totalUsedRam_bytes": {
      "avg": 3152303232,
      "min": 2933686272,
      "max": 3371954176,
      "p50": 3184119808,
      "p75": 3235876864,
      "p80": 3247439872,
      "p90": 3258236928,
      "p95": 3272880128,
      "p99": 3371954176
    },
    "applicationPSSRam_bytes": {
      "avg": 240061867,
      "min": 48441344,
      "max": 347667456,
      "p50": 234886144,
      "p75": 309626880,
      "p80": 318483456,
      "p90": 324459520,
      "p95": 324681728,
      "p99": 347667456
    },
    "applicationPrivateRam_bytes": {
      "avg": 202574848,
      "min": 20975616,
      "max": 311607296,
      "p50": 197840896,
      "p75": 271716352,
      "p80": 280535040,
      "p90": 287219712,
      "p95": 287424512,
      "p99": 311607296
    }
  },
  "network": {
    "networkSpeed": {
      "wiFiSpeed": {
        "total": {
          "rx_bytes_per_sec": {
            "avg": 0,
            "min": 0,
            "max": 0,
            "p50": 0,
            "p75": 0,
            "p80": 0,
            "p90": 0,
            "p95": 0,
            "p99": 0
          },
          "tx_bytes_per_sec": {
            "avg": 0,
            "min": 0,
            "max": 0,
            "p50": 0,
            "p75": 0,
            "p80": 0,
            "p90": 0,
            "p95": 0,
            "p99": 0
          }
        },
        "application": {
          "rx_bytes_per_sec": {
            "avg": 0,
            "min": 0,
            "max": 0,
            "p50": 0,
            "p75": 0,
            "p80": 0,
            "p90": 0,
            "p95": 0,
            "p99": 0
          },
          "tx_bytes_per_sec": {
            "avg": 0,
            "min": 0,
            "max": 0,
            "p50": 0,
            "p75": 0,
            "p80": 0,
            "p90": 0,
            "p95": 0,
            "p99": 0
          }
        }
      },
      "mobileSpeed": {
        "total": {
          "rx_bytes_per_sec": {
            "avg": 815333,
            "min": 0,
            "max": 5049620,
            "p50": 0,
            "p75": 147946,
            "p80": 2635735,
            "p90": 3366419,
            "p95": 4076446,
            "p99": 5049620
          },
          "tx_bytes_per_sec": {
            "avg": 7634,
            "min": 0,
            "max": 154212,
            "p50": 0,
            "p75": 6653,
            "p80": 8227,
            "p90": 10162,
            "p95": 17286,
            "p99": 154212
          }
        },
        "application": {
          "rx_bytes_per_sec": {
            "avg": 1802096,
            "min": 0,
            "max": 5049620,
            "p50": 147946,
            "p75": 3366419,
            "p80": 3975509,
            "p90": 4076446,
            "p95": 5049620,
            "p99": 5049620
          },
          "tx_bytes_per_sec": {
            "avg": 5774,
            "min": 0,
            "max": 17286,
            "p50": 2723,
            "p75": 9510,
            "p80": 10162,
            "p90": 16287,
            "p95": 17286,
            "p99": 17286
          }
        }
      }
    },
    "networkTotal": {
      "wiFiTotal": {
        "total": {
          "rx_bytes": 0,
          "tx_bytes": 0
        },
        "application": {
          "rx_bytes": 0,
          "tx_bytes": 0
        }
      },
      "mobileTotal": {
        "total": {
          "rx_bytes": 27927045,
          "tx_bytes": 243318
        },
        "application": {
          "rx_bytes": 27881078,
          "tx_bytes": 87489
        }
      }
    }
  },
  "battery": {
    "application_mAh": 2.88
  },
  "frames": {
    "applicationRenderedFrames": 1641,
    "applicationJankyFrames": 25
  }
}
```

## Statistic for IOS

Example:

```json
{
  "events": [
    " [START] CPU load 1672236334428",
    " [END] CPU load 1672236338453",
    " [START] Network request 1672236339360",
    " [END] Network request 1672236340664",
    " Show gif 1672236341849",
    " Hide gif 1672236345943"
  ],
  "complexEvents": [
    {
      "timestamp": "2022-12-28T14:05:34.428Z",
      "name": "CPU load",
      "latency": 4025
    },
    {
      "timestamp": "2022-12-28T14:05:39.36Z",
      "name": "Network request",
      "latency": 1304
    }
  ],
  "system": {
    "cpu": {
      "usedCpu_percentage": {
        "avg": 148.49,
        "min": 0,
        "max": 315.1,
        "p50": 145.76,
        "p75": 197.66,
        "p80": 216.91,
        "p90": 273.33,
        "p95": 274.14,
        "p99": 315.1
      }
    },
    "ram": {
      "usedRam_bytes": {
        "avg": 2252539281,
        "min": 2167128064,
        "max": 2346844160,
        "p50": 2242428928,
        "p75": 2292645888,
        "p80": 2308227072,
        "p90": 2329837568,
        "p95": 2339733504,
        "p99": 2346844160
      }
    },
    "disk": {
      "dataRead_bytes_per_sec": {
        "avg": 1134162,
        "min": 0,
        "max": 5167765,
        "p50": 311422,
        "p75": 1711314,
        "p80": 2136959,
        "p90": 2662274,
        "p95": 3612240,
        "p99": 5167765
      },
      "dataWritten_bytes_per_sec": {
        "avg": 7684206,
        "min": 0,
        "max": 73662098,
        "p50": 294219,
        "p75": 3300814,
        "p80": 4841080,
        "p90": 8734245,
        "p95": 63253449,
        "p99": 73662098
      },
      "readsIn_per_sec": {
        "avg": 46.49,
        "min": 0,
        "max": 267.57,
        "p50": 26.98,
        "p75": 55.6,
        "p80": 56.33,
        "p90": 100.19,
        "p95": 145.07,
        "p99": 267.57
      },
      "writesOut_per_sec": {
        "avg": 57.2,
        "min": 0,
        "max": 375.17,
        "p50": 3.93,
        "p75": 70.42,
        "p80": 101.82,
        "p90": 194.63,
        "p95": 299.73,
        "p99": 375.17
      }
    },
    "systemNetwork": {
      "dataReceived_bytes_per_sec": {
        "avg": 274015,
        "min": 4133,
        "max": 2304963,
        "p50": 147240,
        "p75": 258195,
        "p80": 260818,
        "p90": 323664,
        "p95": 728348,
        "p99": 2304963
      },
      "dataSent_bytes_per_sec": {
        "avg": 152462,
        "min": 4133,
        "max": 322436,
        "p50": 143891,
        "p75": 240779,
        "p80": 253005,
        "p90": 284366,
        "p95": 288432,
        "p99": 322436
      },
      "packetsIn_per_sec": {
        "avg": 890.97,
        "min": 21.36,
        "max": 1993.11,
        "p50": 874.14,
        "p75": 1370.32,
        "p80": 1398.78,
        "p90": 1595.4,
        "p95": 1711.01,
        "p99": 1993.11
      },
      "packetsOut_per_sec": {
        "avg": 832.91,
        "min": 21.36,
        "max": 1589.73,
        "p50": 872.26,
        "p75": 1193.26,
        "p80": 1370.32,
        "p90": 1486.12,
        "p95": 1550.4,
        "p99": 1589.73
      }
    },
    "frames": {
      "fps": {
        "avg": 3.12,
        "min": 0,
        "max": 31,
        "p50": 0,
        "p75": 3,
        "p80": 8,
        "p90": 9,
        "p95": 9,
        "p99": 31
      }
    },
    "gpu": {
      "gpuUtilization_percentage": {
        "avg": 4.25,
        "min": 0,
        "max": 56,
        "p50": 0,
        "p75": 0,
        "p80": 0,
        "p90": 0,
        "p95": 46,
        "p99": 56
      }
    }
  },
  "application": {
    "cpu": {
      "usedCpu_percentage": {
        "avg": 19.43,
        "min": 0.05,
        "max": 54.4,
        "p50": 16.03,
        "p75": 32.09,
        "p80": 32.09,
        "p90": 53.87,
        "p95": 54.4,
        "p99": 54.4
      }
    },
    "ram": {
      "usedRam_bytes": {
        "avg": 113432316,
        "min": 44974784,
        "max": 264422320,
        "p50": 49365696,
        "p75": 112263952,
        "p80": 261063400,
        "p90": 264422320,
        "p95": 264422320,
        "p99": 264422320
      }
    },
    "applicationNetwork": {
      "dataReceived_bytes_per_sec": {
        "avg": 546164,
        "min": 546164,
        "max": 546164,
        "p50": 546164,
        "p75": 546164,
        "p80": 546164,
        "p90": 546164,
        "p95": 546164,
        "p99": 546164
      },
      "dataSent_bytes_per_sec": {
        "avg": 5841,
        "min": 5841,
        "max": 5841,
        "p50": 5841,
        "p75": 5841,
        "p80": 5841,
        "p90": 5841,
        "p95": 5841,
        "p99": 5841
      },
      "packetsIn_per_sec": {
        "avg": 414,
        "min": 414,
        "max": 414,
        "p50": 414,
        "p75": 414,
        "p80": 414,
        "p90": 414,
        "p95": 414,
        "p99": 414
      },
      "packetsOut_per_sec": {
        "avg": 96,
        "min": 96,
        "max": 96,
        "p50": 96,
        "p75": 96,
        "p80": 96,
        "p90": 96,
        "p95": 96,
        "p99": 96
      }
    }
  }
}
```