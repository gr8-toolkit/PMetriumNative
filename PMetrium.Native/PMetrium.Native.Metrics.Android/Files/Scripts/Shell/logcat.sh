#!/usr/bin/env sh

adb -s $1 shell "logcat -d | grep PERFORMANCE"