#!/bin/bash

apt-get update
apt-get install -y iproute2

ip a

# Limit all incoming and outgoing network to 1mbit/s
tc qdisc add dev eth0 root handle 1:0 netem delay 100ms
tc qdisc add dev eth0 parent 1:1 handle 10: tbf rate 14.4kbit buffer 1600 limit 3000

/usr/bin/mongod --replSet rs --bind_ip_all
