# Docker Setup Instructions

## Prerequisites
Docker and Docker Compose need to be installed on your system.

### Install Docker on Ubuntu/Debian
```bash
# Update package index
sudo apt-get update

# Install required packages
sudo apt-get install -y ca-certificates curl gnupg lsb-release

# Add Docker's official GPG key
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# Set up the repository
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker Engine
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Add your user to the docker group
sudo usermod -aG docker $USER

# You'll need to log out and back in for this to take effect
```

## Starting the Services

Once Docker is installed, start all services with:

```bash
cd /home/jaime/projects/jimyy
docker compose up -d
```

## Verifying Services

Check that all services are running:

```bash
docker compose ps
```

You should see all services with status "Up" and healthy.

## Accessing Management UIs

- **Adminer (PostgreSQL UI)**: http://localhost:8080
  - Server: postgres
  - Username: workflow_user
  - Password: workflow_pass_2024
  - Database: workflow_automation

- **Mongo Express (MongoDB UI)**: http://localhost:8081
  - Username: admin
  - Password: admin

- **Redis Commander (Redis UI)**: http://localhost:8082

## Connection Strings for Application

### PostgreSQL
```
Host=localhost;Port=5432;Database=workflow_automation;Username=workflow_user;Password=workflow_pass_2024
```

### MongoDB
```
mongodb://workflow_user:workflow_pass_2024@localhost:27017/workflow_logs?authSource=admin
```

### Redis
```
localhost:6379,password=workflow_pass_2024
```

## Stopping Services

```bash
docker compose down
```

## Stopping and Removing All Data

```bash
docker compose down -v
```

## Troubleshooting

### Port Already in Use
If you get a "port already in use" error, either stop the conflicting service or modify the port mappings in docker-compose.yml

### Permission Denied
If you get permission errors, make sure your user is in the docker group:
```bash
sudo usermod -aG docker $USER
newgrp docker
```
