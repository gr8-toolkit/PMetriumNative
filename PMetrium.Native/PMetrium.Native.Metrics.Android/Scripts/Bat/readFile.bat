@echo off

adb -s %1 shell "cat /sdcard/metrics/%2"