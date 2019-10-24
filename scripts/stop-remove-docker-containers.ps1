docker ps -a -q | % { docker stop $_ }
docker ps -a -q | % { docker rm $_ }
