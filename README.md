# LiveCompose

LiveCompose is a lightweight API that allows you to update your Docker Compose services via webhooks. It simplifies the deployment process by enabling you to pull updates and run your services with minimal effort, making it ideal for continuous integration workflows.

## Features

- Update all or specific services in a Docker Compose file via API calls.
- Modify environment variables in the `.env` file using query strings.
- Access real-time service logs.
- Retrieve docker compose services status
- Unique project ID generation for easy management.

## Configuration

LiveCompose has two key environment variables with default values:

- **ASK_LiveCompose__BasePath**: The base path where the application looks for subfolders containing Docker Compose files.  
  **Default**: `/projects`

- **ASK_LiveCompose__Key**: Used to compute the project IDs.  
  **Default**: `1234567890abcdefgh`  
  **Important**: A specific key must be set in production for security purposes.

At startup, LiveCompose will display the project IDs for each folder in the output in the following format:

```
/projects/bookstack => 0a7910c601b98d71168753cac15700ce
/projects/cloudbeaver => f3832c6d95cc3530b57956c6ecf7dfca
```

The project ID remains constant as long as the `ASK_LiveCompose__Key` value stays the same.

## API Endpoints

### Update Services

To update all services or a specific service, use the following endpoints:

- **Update all services**:
    ```
    POST http://localhost:9000/projects/{project_id}/update
    ```

- **Update a specific service**:
    ```
    POST http://localhost:9000/projects/{project_id}/services/{service_name}/update
    ```

Replace `{project_id}` with the unique ID generated for your project and `{service_name}` with the name of the service you want to update. You can pass environment variables to update the `.env` file by prefixing the variable name with `ENV`. For example:

```
POST http://localhost:9000/projects/{project_id}/services/{service_name}/update?ENV_VAR1=value1&ENV_VAR2=value2
```

This request will execute `docker compose pull` and `docker compose up` after updating the specified environment variables in the `.env` file.

### Get Service Logs

To retrieve logs for a specific service, use:

```
GET http://localhost:9000/projects/{project_id}/services/{service_name}/logs
```

To retrieve all service logs, use:

```
GET http://localhost:9000/projects/{project_id}/logs
```

### Get Docker Compose Status

To get the output of `docker-compose ps` for a project, simply use:

```
GET http://localhost:9000/projects/{project_id}/
```

This will return the current status of the services defined in your Docker Compose file.

## Deployment

You can deploy LiveCompose using the Docker image `askbe/livecompose`. Here’s a sample `docker-compose.yml` file to get you started:

```yaml
services:
  livecompose:
    image: askbe/livecompose:0.0.3
    environment:
      - ASK_LiveCompose__BasePath=/projects
      - ASK_LiveCompose__Key=1234567890abcdefgh  # Change this for production
    volumes:
      - /home/flynn/Docker:/projects
      - /var/run/docker.sock:/var/run/docker.sock
    ports:
      - 9000:8080
```

### Steps to Deploy

1. Create a directory for your projects, e.g., `/home/flynn/Docker`.
2. Create the `docker-compose.yml` file with the content above.
3. Run the following command to start the LiveCompose service:

```bash
docker-compose up -d
```

## Example GitLab CI/CD Pipeline

Here’s an example of how you can configure a GitLab CI/CD pipeline to automatically deploy an updated Docker image using LiveCompose:

```yaml
stages:
  - build
  - deploy

build:
  stage: build
  script:
    - echo "Building Docker image..."
    - docker build -t my-image:latest .

deploy:
  stage: deploy
  script:
    - echo "Deploying with LiveCompose..."
    - curl -X POST "http://localhost:9000/projects/0a7910c601b98d71168753cac15700ce/update"
  only:
    - master
```

### Explanation of the Pipeline

- **Build Stage**: This stage builds your Docker image. Replace `my-image:latest` with the appropriate image name for your project.
- **Deploy Stage**: After the image is built, this stage triggers the LiveCompose update for the specified project ID. Make sure to replace `0a7910c601b98d71168753cac15700ce` with your actual project ID.

You can customize the pipeline according to your specific requirements and branching strategies.

## Example Usage

You can easily integrate LiveCompose into your CI/CD pipeline. Here’s how you can use `curl` to update your services, retrieve logs, and check status.

1. **Update Services** with environment variables:

```bash
curl -X POST "http://localhost:9000/projects/0a7910c601b98d71168753cac15700ce/update?ENV_VAR1=value1&ENV_VAR2=value2"
```

2. **Update a Specific Service**:

```bash
curl -X POST "http://localhost:9000/projects/0a7910c601b98d71168753cac15700ce/services/service_name/update"
```

3. **View Logs for a Specific Service** with a timeout:

```bash
curl --max-time 10 "http://localhost:9000/projects/0a7910c601b98d71168753cac15700ce/services/service_name/logs"
```

4. **View All Logs** with a timeout:

```bash
curl --max-time 10 "http://localhost:9000/projects/0a7910c601b98d71168753cac15700ce/logs"
```

5. **Check Docker Compose Status**:

```bash
curl http://localhost:9000/projects/0a7910c601b98d71168753cac15700ce/
```

## Contributing

Contributions are welcome! Please submit a pull request or open an issue to discuss improvements.

## License

This project is licensed under the LGPL-3.0 License or later. See the LICENSE file for details.