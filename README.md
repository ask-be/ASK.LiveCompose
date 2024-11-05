![GitHub Release](https://img.shields.io/github/v/tag/ask-be/ASK.LiveCompose)
![Docker Image Version](https://img.shields.io/docker/v/askbe/livecompose)

# LiveCompose

LiveCompose is a lightweight API that allows you to update your Docker Compose services via webhooks. It simplifies the deployment process by enabling you to pull updates and run your services with minimal effort, making it ideal for continuous integration workflows.

## Features

- Update all or specific services in a Docker Compose file via API calls.
- Modify environment variables in the `.env` file using query strings.
- Access real-time service logs.
- Unique project ID generation for easy management.
- Integrated Rate Limiting 

### Important Security Note

**All API calls must be made over HTTPS to ensure security.** It is essential to set up a reverse proxy (such as Nginx or Traefik) for SSL termination before deploying LiveCompose.
**Additionally, implement IP restrictions in your reverse proxy configuration to control access to the API and enhance security.** Direct HTTP calls should be avoided to protect sensitive data.

## Project Auth-Tokens and Authentication

At startup each project is assigned an Auth-Token built using the ```ASK_LiveCompose__Key``` environment variable (see below). All the projects Auth-Tokens are displayed in console at application startup. 
```
|--------------------------|----------------------------------|
| Project Name            | Project Auth Token               |
|--------------------------|----------------------------------|
| baseline                 | 12345678901234567890123456789011 |
| bevault                  | azertyuiopqsdfghjklmmmwxcvbnazzz |
| bookstack                | bz1234uiopqsdfghjklmm2345332azzz |
| cloudbeaver              | 9dab7bb5f4af64ae8f095f112342a844 |
|--------------------------|----------------------------------|
```
The Auth-Token of the project must be sent as using the ```X-Auth-token``` HTTP Header.
If there is a need to generate new Auth-Tokens, simply update the ```ASK_LiveCompose__Key``` environment variable and restart the application.

## API Endpoints

### Update Services

To update all services or a specific service, use the following endpoints:

- **Update all services**:
    ```
    POST https://yourdomain.com/projects/{project_name}/update
    ```

- **Update a specific service**:
    ```
    POST https://yourdomain.com/projects/{project_name}/services/{service_name}/update
    ```

### Get Service Logs

To retrieve logs for a specific service, use:

```
GET https://yourdomain.com/projects/{project_name}/services/{service_name}/logs
```

To retrieve all service logs, use:

```
GET https://yourdomain.com/projects/{project_name}/logs
```

The following query string parameters can be specified while requesting logs

- ```t=true``` : Boolean value, Add timestamp before each line (default false)
- ```n=xxx```: Number of lines to show from the end of the logs for each container or "All" for everything (default All)
- ```since=xxx```: Show logs since timestamp (e.g. 2013-01-02T13:23:37Z) or relative (e.g. 42m for 42 minutes)

### Get Docker Compose Status

To get the output of `docker-compose ps` for a project, simply use:

```
GET https://yourdomain.com/projects/{project_name}/
```

## Deployment

You can deploy LiveCompose using the Docker image `askbe/livecompose`. Here’s how to run it using a Docker command:

### Important Reverse Proxy Configuration

When deploying a reverse proxy, ensure you configure it to allow traffic only from specific IP addresses that require access to the LiveCompose API. This adds an additional layer of security. 

### Using Docker Command

Run the following command to start the LiveCompose service:

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

If you prefer to use Docker Compose, here’s a sample `docker-compose.yml` file:

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

## Rate Limiting

Rate limiting is enabled on the api by default and can be customized using the following environment variables (default limit is 5req/sec):
- **ASK_LiveCompose__EnableRateLimit**: Boolean, Enable to Disable Rate Limiting (default : true)
- **ASK_LiveCompose__RateLimit**: Number of request allowed by Window (default :5)
- **ASK_LiveCompose__RateDelaySecond**: Window Delay Seconds (default :1)
- **ASK_LiveCompose__RateLimitQueueSize** : Number of requests put in queue before dropping requests (default: 0)

## Example Usage

Here’s how you can use `curl` to update your services, retrieve logs, and check status. Remember to use `https://` for all API calls.

1. **Update Services** with environment variables:

```bash
curl -X POST \
     -H X-Auth-Token:701b0bbeb927cbeb41435c1b5dc39d57\
     "https://yourdomain.com/projects/reverse_proxy/update?ENV_VAR1=value1&ENV_VAR2=value2"
```

2. **Update a Specific Service**:

```bash
curl -X POST \
     -H X-Auth-Token:234b034b927cbeb41435c1b5dc39345\
     "https://yourdomain.com/projects/bookstack/services/app/update"
```

3. **View Logs for a Specific Service**:

```bash
curl --max-time 10 \
     -H X-Auth-Token:234b034b927cbeb41435c1b5dc39345\
     "https://yourdomain.com/projects/bookstack/services/app/logs"
```

4. **View All Logs**:

```bash
curl --max-time 10 \
     -H X-Auth-Token:234b034b927cbeb41435c1b5dc39345\
     "https://yourdomain.com/projects/bookstack/logs"
```

5. **Check Docker Compose Status**:

```bash
curl -H X-Auth-Token:234b034b927cbeb41435c1b5dc39345 "https://yourdomain.com/projects/bookstack/"
```

## Contributing

Contributions are welcome! Please submit a pull request or open an issue to discuss improvements.

## License

This project is licensed under the LGPL-3.0 License or later. See the LICENSE file for details.