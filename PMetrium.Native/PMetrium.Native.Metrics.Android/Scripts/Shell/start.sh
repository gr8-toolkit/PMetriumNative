#!/usr/bin/env sh

adb -s $1 shell "mkdir /sdcard/metrics &> /dev/null || true"
adb -s $1 shell "echo 'finished' > /sdcard/metrics/test_status.txt"
adb -s $1 shell "touch /sdcard/metrics/shell_pid.txt &> /dev/null || true"
adb -s $1 shell "if [ -s /sdcard/metrics/shell_pid.txt ]; then kill -9 \$(ps -s \$(cat /sdcard/metrics/shell_pid.txt) -o pid | grep -v -i PID | grep -v \$(cat /sdcard/metrics/shell_pid.txt)) &> /dev/null || true; fi"
adb -s $1 shell "dumpsys batterystats --reset; dumpsys battery unplug; dumpsys batterystats --reset"
adb -s $1 push Scripts/Shell/phoneMetrics.sh /sdcard/metrics
adb -s $1 shell "logcat --clear"
adb -s $1 shell "chmod +x /sdcard/metrics/phoneMetrics.sh"
adb -s $1 shell "echo -n 'Shell PID = '; echo \$\$; echo \$\$ > /sdcard/metrics/shell_pid.txt; sh /sdcard/metrics/phoneMetrics.sh $2"