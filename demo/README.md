# MicroFlows Demo

**To build adminui image**
*in solution folder*
docker build -t microflows-adminui -f ./admin/MicroFlows.AdminUI/Dockerfile .
docker images

**To start adminui and sql**
*in solution folder*:*
docker-compose -f ./demo/docker-compose.yml up -d


**To start SQL Server**
cd ./demo/sql
docker-compose up -d

