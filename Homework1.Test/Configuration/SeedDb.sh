#!/bin/bash

java -jar ../../liquibase/liquibase.jar  --changeLogFile=../../Homewrok1/src/Database/Migrations/index.xml \
--username=mydb \
--password=mydb \
--url=jdbc:postgresql://localhost:5433/mydb \
--driver=org.postgresql.Driver \
--classpath=../../postgresql-42.2.5.jar \
--contexts="test" \
update