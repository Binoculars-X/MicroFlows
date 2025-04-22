docker build -t microflows-adminui -f ./admin/MicroFlows.AdminUI/Dockerfile .
#Start-Sleep -Seconds 2

docker-compose -f ./demo/docker-compose.yml up -d
