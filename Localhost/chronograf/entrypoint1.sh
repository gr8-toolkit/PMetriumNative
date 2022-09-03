#!/bin/sh
 
bash /entrypoint.sh
chronograf --influxdb-url http://influxdb-pmetrium-native:8086 &

echo "==================> Create Chronograf new connection"

curl localhost:8888/ping
while [ $? -ne 0 ]; do
  echo -n "."
  sleep 2
  curl localhost:8888/ping
done
echo " "

echo "==================> Chronograf - Ready"
wait $!