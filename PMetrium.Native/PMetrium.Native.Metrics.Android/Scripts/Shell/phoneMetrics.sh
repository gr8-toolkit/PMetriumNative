#!/bin/sh

dumpsys batterystats --reset

cd /sdcard/metrics

for i in "$@"; do
  case $i in
  --app=*)
    APP_NAME="${i#*=}"
    shift
    ;;
  --cpuTotal=*)
    CPU_TOTAL="${i#*=}"
    shift
    ;;
  --cpuApp=*)
    CPU_APP="${i#*=}"
    shift
    ;;
  --ramTotal=*)
    RAM_TOTAL="${i#*=}"
    shift
    ;;
  --ramApp=*)
    RAM_APP="${i#*=}"
    shift
    ;;
  --networkTotal=*)
    NETWORK_TOTAL="${i#*=}"
    shift
    ;;
  --networkApp=*)
    NETWORK_APP="${i#*=}"
    shift
    ;;
  --batteryApp=*)
    BATTERY_APP="${i#*=}"
    shift
    ;;
  --framesApp=*)
    FRAMES_APP="${i#*=}"
    shift
    ;;
  -* | --*)
    echo "Unknown option $i"
    exit 1
    ;;
  *) ;;

  esac
done

echo "-> Parameters inside the script: \n --app=$APP_NAME \n --cpuTotal=$CPU_TOTAL \n --cpuApp=$CPU_APP \n --ramTotal=$RAM_TOTAL \n --ramApp=$RAM_APP \n --networkTotal=$NETWORK_TOTAL \n --networkApp=$NETWORK_APP \n --batteryApp=$BATTERY_APP \n --framesApp=$FRAMES_APP"

TEST_STATUS_FILE="test_status.txt"
SHELL_PID_FILE="shell_pid.txt"

CPU_TOTAL_FILE="cpu_total.txt"
CPU_USAGE_TOTAL_FILE="cpu_usage_total.txt"
CPU_USAGE_APP_FILE="cpu_usage_app.txt"
RAM_TOTAL_FILE="ram_total.txt"
RAM_USAGE_TOTAL_FILE="ram_usage_total.txt"
RAM_USAGE_APP_FILE="ram_usage_app.txt"
NETWORK_USAGE_TEMP_FILE="network_usage_temp.txt"
NETWORK_USAGE_TOTAL_FILE="network_usage_total.txt"
NETWORK_USAGE_APP_FILE="network_usage_app.txt"
FRAMES_APP_FILE="frames_app.txt"
BATTERY_APP_FILE="battery_app.txt"

echo 'started' >$TEST_STATUS_FILE
echo '' >$CPU_TOTAL_FILE
echo '' >$CPU_USAGE_TOTAL_FILE
echo '' >$CPU_USAGE_APP_FILE
echo '' >$RAM_TOTAL_FILE
echo '' >$RAM_USAGE_TOTAL_FILE
echo '' >$RAM_USAGE_APP_FILE
echo '' >$NETWORK_USAGE_TEMP_FILE
echo '' >$NETWORK_USAGE_TOTAL_FILE
echo '' >$NETWORK_USAGE_APP_FILE
echo '' >$FRAMES_APP_FILE
echo '' >$BATTERY_APP_FILE

top -n 1 -b -p $$ | grep -i %cpu | grep -i %idle | awk '{ print $1 }' >$CPU_TOTAL_FILE
free | grep -i mem | awk '{ print $2 }' >$RAM_TOTAL_FILE

if [ $APP_NAME != "system" ]; then
  USER_ID=$(dumpsys package $APP_NAME | grep userId | head -n 1 | cut -b 12-17)
fi

echo "-> READY_TO_TRACK_METRICS"

#--------------------------------- SYSTEM STATISTICS ------------------------------------------------------

# =============> Get CPU usage statistic for the whole system

if $CPU_TOTAL; then
  top -d 1 -b -p $$ | grep --line-buffered -i '%cpu' | grep --line-buffered -i '.*%idle' | awk '{ system("print -n $(date +%s); print " "_"$5) }' >>$CPU_USAGE_TOTAL_FILE &
fi

# =============> Get RAM usage statistic for the whole system

if $RAM_TOTAL; then
  while [ $(cat $TEST_STATUS_FILE) = "started" ]; do
    cat /proc/meminfo | grep Available | awk '{ system("print -n $(date +%s); print " "_"$2) }' >>$RAM_USAGE_TOTAL_FILE
    sleep 0.92
  done &
fi
RAM_USAGE_TOTAL_PID=$!

# =============> Get NETWORK usage statistic for the whole system and the APP

dumpsys batterystats --reset  

if $NETWORK_TOTAL && [ $APP_NAME != "system" ]; then
  while [ $(cat $TEST_STATUS_FILE) = "started" ]; do
    export TIMESTAMP=$(date +%s)
    dumpsys batterystats --checkin | grep -E ",gn,|$USER_ID,l,nt" | awk -F',' '{ system("print -n $TIMESTAMP; print " "_"$5"_"$6"_"$7"_"$8"_"$4) }' >>$NETWORK_USAGE_TEMP_FILE
    sleep 0.850
  done &
fi

if $NETWORK_TOTAL && [ $APP_NAME == "system" ]; then
  while [ $(cat $TEST_STATUS_FILE) = "started" ]; do
    export TIMESTAMP=$(date +%s)
    dumpsys batterystats --checkin | grep -E ",gn," | awk -F',' '{ system("print -n $TIMESTAMP; print " "_"$5"_"$6"_"$7"_"$8"_"$4) }' >>$NETWORK_USAGE_TEMP_FILE
    sleep 0.850
  done &
fi

NETWORK_USAGE_PID=$!

#--------------------------------- APP STATISTICS ---------------------------------------------------------

if [ $APP_NAME != "system" ] ; then 
  echo "-> Wait for the application"
  
  #timeout for 2 minutes
  COUNTER=0
  while (($COUNTER < 600)) && [ -z $(pidof $APP_NAME) ] && [ $(cat $TEST_STATUS_FILE) = "started" ]; do
    sleep 0.2
    COUNTER=$(($COUNTER + 1)) 
  done
  
  if [ $COUNTER = "600" ]; then
    echo "finished" >$TEST_STATUS_FILE
    wait "$RAM_USAGE_TOTAL_PID"
    kill -9 $(pgrep -P $$) 3 &>/dev/null || true
    echo "-> EXIT after timeout - application is not found"
    exit
  fi
  
  APP_PID=$(pidof $APP_NAME)
  echo "-> $APP_NAME PID = $APP_PID"
fi

# =============> Get CPU usage statistic for the app

if $CPU_APP && [ $APP_NAME != "system" ]; then
  top -d 1 -b -p $APP_PID | grep --line-buffered -i $APP_NAME | awk '{ system("print -n $(date +%s); print " "_"$9) }' >>$CPU_USAGE_APP_FILE &
fi

# =============> Get RAM usage statistic for the app (PSS memory and Private memory)

if $RAM_APP && [ $APP_NAME != "system" ]; then
  while [ $(cat $TEST_STATUS_FILE) = "started" ]; do
    dumpsys meminfo -c $APP_NAME | grep --line-buffered TOTAL | grep -v PSS | awk '{ system("print -n $(date +%s); print " "_"$2"_"$3) }' >>$RAM_USAGE_APP_FILE
    sleep 0.7
  done &
fi
RAM_USAGE_APP_PID=$!

# =============> Frames for the app

if $FRAMES_APP && [ $APP_NAME != "system" ]; then
  while [ $(cat $TEST_STATUS_FILE) = "started" ] && [ -n "$(pidof $APP_NAME)" ]; do
    dumpsys gfxinfo $APP_NAME | grep --line-buffered -C 3 "50th percentile" >>$FRAMES_APP_FILE
    echo "$(date +%s)\n" >>$FRAMES_APP_FILE
    sleep 0.875
  done &
fi
FRAMES_APP_PID=$!

# =============> BATTERY usage for the app

if $BATTERY_APP && [ $APP_NAME != "system" ]; then
  while [ $(cat $TEST_STATUS_FILE) = "started" ]; do
    dumpsys batterystats --checkin $APP_NAME | grep "$USER_ID,l,pwi,uid" | awk -F',' '{ system("print -n $(date +%s); print " "_"$6) }' >>$BATTERY_APP_FILE
    sleep 0.850
  done &
fi
BATTERY_APP_PID=$!

# =============> Wait and kill all background tasks

while [ $(cat $TEST_STATUS_FILE) = "started" ]; do
  sleep 1
done

wait "$RAM_USAGE_TOTAL_PID"
wait "$RAM_USAGE_APP_PID"
wait "$NETWORK_USAGE_PID"
wait "$FRAMES_APP_PID"
wait "$BATTERY_APP_PID"

if $NETWORK_TOTAL; then
  grep "gn" $NETWORK_USAGE_TEMP_FILE | awk -F'_' '{ system("print " $1"_"$2"_"$3"_"$4"_"$5) }' >$NETWORK_USAGE_TOTAL_FILE
fi

if $NETWORK_APP && [ $APP_NAME != "system" ]; then
  grep "nt" $NETWORK_USAGE_TEMP_FILE | awk -F'_' '{ system("print " $1"_"$2"_"$3"_"$4"_"$5) }' >$NETWORK_USAGE_APP_FILE
fi

SHELL_PID=$(cat $SHELL_PID_FILE)
PIDS_TO_KILL=$(
  ps -s $SHELL_PID -o pid | grep -v -i PID | grep -v $SHELL_PID | grep -v $$ | xargs echo -n
  echo ""
)

echo "-> PIDs to kill: $PIDS_TO_KILL"

kill -9 $PIDS_TO_KILL &>/dev/null || true

echo "-> EXIT from the script - SUCCESS"
