# Download the configuration files.
export ZITADEL_CONFIG_FILES=https://raw.githubusercontent.com/zitadel/zitadel/main/docs/docs/self-hosting/manage/reverseproxy
wget ${ZITADEL_CONFIG_FILES}/docker-compose.yaml -O docker-compose-base.yaml
wget ${ZITADEL_CONFIG_FILES}/nginx/docker-compose.yaml -O docker-compose-nginx.yaml
wget ${ZITADEL_CONFIG_FILES}/nginx/nginx-disabled-tls.conf -O nginx-disabled-tls.conf

# Run the database, ZITADEL and NGINX.
docker compose --file docker-compose-base.yaml --file docker-compose-nginx.yaml up --detach proxy-disabled-tls

# Test that gRPC and HTTP APIs work. Empty brackets like {} means success.
sleep 3
grpcurl --plaintext 127.0.0.1.sslip.io:80 zitadel.admin.v1.AdminService/Healthz
curl http://127.0.0.1.sslip.io:80/admin/v1/healthz

