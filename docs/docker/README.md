# Ajuntament Docker Compose Project (aj)
#
# This project defines the required infrastructure for local development:
# - MySQL 5.6 (with persistent volume, user, and root access)
# - Minio object storage (with persistent volume and credentials)
#
# To start the stack:
#   cd docs/docker
#   docker-compose up -d
#
# To stop:
#   docker-compose down
#
# Data is persisted in the 'mysql_data' and 'minio_data' Docker volumes.
#
# MySQL connection string (for appsettings):
#   Server=127.0.0.1;Database=gestorsubvencions;User=user;Password=password;
#
# NOTE: root/root does NOT work against the existing 'mysql_data' volume
# (it was initialised with a different root password). Use user/password
# for schema scripts too - 'user' has ALL PRIVILEGES on gestorsubvencions:
#   docker exec -i aj-mysql mysql -uuser -ppassword gestorsubvencions < script.sql
#
# Minio endpoint (for appsettings):
#   Endpoint: http://localhost:9000
#   AccessKey: admin
#   SecretKey: adminpassword123
#   Bucket: patrimoni
#
# If you need to restore or backup data, use standard MySQL and Minio tools.
