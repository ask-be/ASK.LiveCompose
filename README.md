# LiveCompose

LiveCompose is a lightweight API that allows you to update your Docker Compose services via webhooks. It simplifies the deployment process by enabling you to pull updates and run your services with minimal effort, making it ideal for continuous integration workflows.

## Features

- Update all or specific services in a Docker Compose file via API calls.
- Modify environment variables in the `.env` file using query strings.
- Access real-time service logs.
- Unique project ID generation for easy management.

### Important Security Note

**All API calls must be made over HTTPS to ensure security.** It is essential to set up a reverse proxy (such as Nginx or Traefik) for SSL termination before deploying LiveCompose. **Additionally, implement IP restrictions in your reverse proxy configuration to control access to the API and enhance security.** Direct HTTP calls should be avoided to protect sensitive data.

## API Endpoints

### Update Services

To update all services or a specific service, use the following endpoints (replace `http://` with `https://`):

- **Update all services**:
    ```
    POST https://yourdomain.com/projects/{project_id}/update
    ```

- **Update a specific service**:
    ```
    POST https://yourdomain.com/projects/{project_id}/services/{service_name}/update
    ```

### Get Service Logs

To retrieve logs for a specific service, use:

```
GET https://yourdomain.com/projects/{project_id}/services/{service_name}/logs
```

To retrieve all service logs, use:

```
GET https://yourdomain.com/projects/{project_id}/logs
```

The following query string parameters can be specified while requesting logs

- ```t=true``` : Boolean value, Add timestamp before each line (default false)
- ```n=xxx```: Number of lines to show from the end of the logs for each container or "All" for everything (default All)
- ```since=xxx```: Show logs since timestamp (e.g. 2013-01-02T13:23:37Z) or relative (e.g. 42m for 42 minutes)

### Get Docker Compose Status

To get the output of `docker-compose ps` for a project, simply use:

```
GET https://yourdomain.com/projects/{project_id}/
```

## Deployment

You can deploy LiveCompose using the Docker image `askbe/livecompose`. Here’s how to run it using a Docker command:

### Important Reverse Proxy Configuration

When deploying a reverse proxy, ensure you configure it to allow traffic only from specific IP addresses that require access to the LiveCompose API. This adds an additional layer of security. 

### Using Docker Command

Run the following command to start the LiveCompose service with autorestart:

```bash
docker run -d \
  --name livecompose \
  --restart unless-stopped \
  -e ASK_LiveCompose__BasePath=/projects \
  -e ASK_LiveCompose__Key=1234567890abcdefgh \  # Change this for production
  -v /opt/compose-projects:/projects \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -p 9000:8080 \
  askbe/livecompose:0.0.3
```

### Using Docker Compose (Optional)

If you prefer to use Docker Compose, here’s a sample `docker-compose.yml` file with autorestart configured:

```yaml
services:
  livecompose:
    image: askbe/livecompose:0.0.3
    restart: unless-stopped
    environment:
      - ASK_LiveCompose__BasePath=/projects
      - ASK_LiveCompose__Key=1234567890abcdefgh  # Change this for production
    volumes:
      - /opt/compose-projects:/projects
      - /var/run/docker.sock:/var/run/docker.sock
    ports:
      - 9000:8080
```

To start the service using Docker Compose, run:

```bash
docker-compose up -d
```

This will achieve the same result as the direct Docker command.

## Example Usage

Here’s how you can use `curl` to update your services, retrieve logs, and check status. Remember to use `https://` for all API calls.

1. **Update Services** with environment variables:

```bash
curl -X POST "https://yourdomain.com/projects/0a7910c601b98d71168753cac15700ce/update?ENV_VAR1=value1&ENV_VAR2=value2"
```

2. **Update a Specific Service**:

```bash
curl -X POST "https://yourdomain.com/projects/0a7910c601b98d71168753cac15700ce/services/service_name/update"
```

3. **View Logs for a Specific Service**:

```bash
curl --max-time 10 "https://yourdomain.com/projects/0a7910c601b98d71168753cac15700ce/services/service_name/logs"
```

4. **View All Logs**:

```bash
curl --max-time 10 "https://yourdomain.com/projects/0a7910c601b98d71168753cac15700ce/logs"
```

5. **Check Docker Compose Status**:

```bash
curl "https://yourdomain.com/projects/0a7910c601b98d71168753cac15700ce/"
```

## Contributing

Contributions are welcome! Please submit a pull request or open an issue to discuss improvements.

## License

This project is licensed under the LGPL-3.0 License or later. See the LICENSE file for details.