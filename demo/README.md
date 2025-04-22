# MicroFlows Demo

**Run adminui in docker**
*run in the solution folder, sometimes app failed because sql is not ready, so restart the script or rerurn demo app in docker desktop*

./adminui.ps1

**To build adminui docker image**
*in solution folder*

docker build -t microflows-adminui -f ./admin/MicroFlows.AdminUI/Dockerfile .

docker images

**To start adminui and sql**
*in solution folder*

docker-compose -f ./demo/docker-compose.yml up -d

**To start SQL Server only**

cd ./demo/sql

docker-compose up -d

