## Zitadel
From
https://zitadel.com/docs/self-hosting/manage/reverseproxy/nginx
```sh
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
```

When the docker compose command exits successfully, go to http://127.0.0.1.sslip.io/ui/console/?login_hint=zitadel-admin@zitadel.127.0.0.1.sslip.io and log in:
* username: zitadel-admin@zitadel.127.0.0.1.sslip.io
* password: Password1!

```sh
# You can now stop the database, ZITADEL and NGINX.
docker compose --file docker-compose-base.yaml --file docker-compose-nginx.yaml down
```