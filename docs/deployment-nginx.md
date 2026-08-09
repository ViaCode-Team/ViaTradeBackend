# Production deployment with Nginx

The configuration in `deploy/nginx` is for this API: it exposes only `/api/v1/`, limits request bodies to 1 MiB, throttles anonymous authentication endpoints, and proxies requests to Kestrel on `127.0.0.1:8080`. Swagger is intentionally unavailable in Production because the application enables it only in Development.

## Prerequisites

* Ubuntu/Debian server with Nginx, .NET 10 runtime, and Certbot.
* DNS `A`/`AAAA` record for the API domain.
* MySQL and Redis reachable by the backend process.
* A dedicated `viatrade` system user.

## Application configuration

1. Publish the project into `/opt/viatrade/backend`:

   ```bash
   dotnet publish ViaTradeBackend/ViaTradeBackend.csproj -c Release -o /opt/viatrade/backend
   ```

2. Copy `deploy/via-trade-backend.env.example` to `/etc/viatrade/backend.env`, set real values, then restrict access:

   ```bash
   sudo install -d -m 750 -o root -g viatrade /etc/viatrade
   sudo install -m 640 -o root -g viatrade deploy/via-trade-backend.env.example /etc/viatrade/backend.env
   ```

   `Jwt__Secret`, `ServiceSecurity__Password`, and database credentials must be unique production secrets. Do not commit this file.

3. Make sure the service user can read the published files and the analyzer data directory. If its data is stored elsewhere, update both `AnalyzerData__SourcePath` and `ReadWritePaths` in the systemd unit.

4. Install and start the service:

   ```bash
   sudo install -m 644 deploy/systemd/via-trade-backend.service /etc/systemd/system/via-trade-backend.service
   sudo systemctl daemon-reload
   sudo systemctl enable --now via-trade-backend
   sudo systemctl status via-trade-backend
   ```

Kestrel is bound to loopback only. Do not open port 8080 in the firewall.

## Nginx and TLS

1. In all files under `deploy/nginx`, replace every `api.example.com` with the actual API domain.

2. Create the directory used by the ACME challenge, install the temporary HTTP-only configuration, and obtain a certificate:

   ```bash
   sudo install -d -m 755 /var/www/certbot
   sudo install -m 644 deploy/nginx/via-trade-bootstrap.conf /etc/nginx/conf.d/via-trade-backend.conf
   sudo nginx -t
   sudo systemctl reload nginx
   sudo certbot certonly --webroot -w /var/www/certbot -d api.example.com
   ```

3. Replace the temporary configuration with the production configuration and validate the complete setup:

   ```bash
   sudo install -d -m 755 /etc/nginx/snippets
   sudo install -m 644 deploy/nginx/via-trade-proxy.conf /etc/nginx/snippets/via-trade-proxy.conf
   sudo install -m 644 deploy/nginx/via-trade-backend.conf /etc/nginx/conf.d/via-trade-backend.conf
   sudo nginx -t
   sudo systemctl reload nginx
   ```

Open only ports 80 and 443. Certbot renewal must reload Nginx after updating a certificate.

## Frontend and internal clients

The application does not configure CORS. A browser frontend on another origin will be rejected; configure explicit allowed origins in ASP.NET Core only when its exact production origin is known. Do not use `Access-Control-Allow-Origin: *` because this API uses secure authentication cookies.

Internal routes under `/api/v1/internal/` remain protected by the application's `TgBot-Service-Password`. If the bot/analyzer has a fixed source IP or private network, additionally restrict this location with Nginx `allow`/`deny` rules.

## Checks

```bash
sudo systemctl is-active via-trade-backend nginx
curl -I http://api.example.com
curl -I https://api.example.com/api/v1/unknown
sudo journalctl -u via-trade-backend -n 100 --no-pager
```

The first command should show a permanent redirect to HTTPS; the HTTPS API check should return the application's 404 response rather than a redirect loop.
