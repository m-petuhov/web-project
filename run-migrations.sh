#!/bin/bash
./liquibase --changeLogFile=$changeLogFile \
--username=$username \
--password=$password \
--url=jdbc:postgresql://$host:$port/$dbname \
--driver=org.postgresql.Driver \
--classpath=./postgresql-42.2.5.jar \
--logLevel=$loglevel \
--contexts="dev" \
update