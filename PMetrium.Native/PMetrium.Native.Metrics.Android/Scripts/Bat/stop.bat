@echo off

adb -s %1 shell "cd sdcard/metrics; echo 'finished' > test_status.txt"
adb -s %1 shell "dumpsys battery reset"