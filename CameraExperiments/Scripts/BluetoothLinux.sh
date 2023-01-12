#!/bin/bash

#set -x

sudo timeout 10 hcitool lescan

{
	printf "connect\n\n"
	sleep 10
	printf "primary %s\n\n" "$2"
	sleep 3
	printf "char-write-req 10 %s\n\n" "$3"
	sleep 3
	printf "disconnect\n\n"
	sleep 3
	printf "quit\n\n"
} | sudo gatttool -t random -b $1 -I