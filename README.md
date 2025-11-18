# Workflow Automation Platform (n8n-like)

A complete workflow automation platform built with .NET 8 and React 18, featuring visual workflow editing, real-time collaboration, AI-powered assistance, and multi-environment management.

## Features

- **Visual Workflow Editor**: Drag-and-drop interface powered by React Flow
- **Node-Based Execution Engine**: Sequential and parallel execution modes
- **200+ Integration Nodes**: APIs, databases, and external services
- **Real-Time Collaboration**: Multi-user editing with SignalR WebSockets
- **AI-Powered Assistant**: Natural language workflow editing with OpenAI/Anthropic
- **Multi-Environment Management**: Testing, Launched, and Production environments
- **Credential Management**: Secure storage with AES-256 encryption
- **Webhook Triggers**: HTTP-triggered workflow execution
- **Scheduled Executions**: Cron-based scheduling with Hangfire
- **Version Control**: Workflow history and version comparison
- **Execution Monitoring**: Real-time logs and execution path visualization
- **CLI Tool**: Terminal-based workflow management
- **REST API**: Comprehensive API for programmatic access

## Technology Stack

### Backend
- **.NET 8**: Modern, high-performance framework
- **PostgreSQL**: Primary data store
- **MongoDB**: Execution logs and large data
- **Redis**: Caching and session management
- **Entity Framework Core**: ORM
- **SignalR**: Real-time communication
- **Hangfire**: Background job scheduling
- **JWT**: Authentication

### Frontend
- **React 18**: Modern UI framework
- **TypeScript**: Type-safe development
- **Jotai**: Atomic state management
- **TanStack Query**: Server state management
- **React Flow**: Workflow canvas
- **Material-UI**: Component library
- **Monaco Editor**: Code editing
- **Axios**: HTTP client

## Project Structure

```
jimyy/
├── backend/                          # .NET Backend
│   ├── WorkflowAutomation.API/       # REST API & SignalR Hubs
│   ├── WorkflowAutomation.Core/      # Domain entities and interfaces
│   ├── WorkflowAutomation.Infrastructure/  # Data access & services
│   ├── WorkflowAutomation.Engine/    # Workflow execution engine
│   └── WorkflowAutomation.CLI/       # Command-line tool
├── frontend/                         # React Frontend
│   ├── src/
│   │   ├── components/               # Reusable components
│   │   ├── pages/                    # Page components
│   │   ├── hooks/                    # Custom React hooks
│   │   ├── services/                 # API services
│   │   ├── atoms/                    # Jotai state atoms
│   │   └── types/                    # TypeScript definitions
├── docs/                             # Documentation
│   ├── devguide/                     # Development guide
│   └── devplan/                      # Development plan
├── docker-compose.yml                # Local development services
└── README.md                         # This file
```

## Getting Started

### Prerequisites

- **.NET 8 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 20+**: [Download here](https://nodejs.org/)
- **Docker & Docker Compose**: [Install here](https://docs.docker.com/get-docker/)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd jimyy
   ```

2. **Start infrastructure services**
   ```bash
   docker compose up -d
   ```

   This starts PostgreSQL, MongoDB, Redis, and their management UIs.

3. **Set up the backend**
   ```bash
   cd backend
   dotnet restore
   dotnet build
   ```

4. **Set up the frontend**
   ```bash
   cd frontend
   npm install
   ```

### Running the Application

1. **Start the backend API**
   ```bash
   cd backend/WorkflowAutomation.API
   dotnet run
   ```

   The API will be available at `http://localhost:5000`

2. **Start the frontend**
   ```bash
   cd frontend
   npm run dev
   ```

   The frontend will be available at `http://localhost:5173`

### Access Management UIs

- **Adminer (PostgreSQL)**: http://localhost:8080
- **Mongo Express**: http://localhost:8081
- **Redis Commander**: http://localhost:8082
- **Hangfire Dashboard**: http://localhost:5000/hangfire

## Configuration

### Backend Configuration

Edit `backend/WorkflowAutomation.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=workflow_automation;Username=workflow_user;Password=workflow_pass_2024",
    "MongoDb": "mongodb://workflow_user:workflow_pass_2024@localhost:27017/workflow_logs?authSource=admin",
    "Redis": "localhost:6379,password=workflow_pass_2024"
  }
}
```

### Frontend Configuration

Edit `frontend/.env`:

```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_SIGNALR_HUB_URL=http://localhost:5000
VITE_ENVIRONMENT=development
```

## Development

### Backend Development

```bash
cd backend
dotnet watch --project WorkflowAutomation.API
```

### Frontend Development

```bash
cd frontend
npm run dev
```

### Running Tests

**Backend:**
```bash
cd backend
dotnet test
```

**Frontend:**
```bash
cd frontend
npm test
```

## Building for Production

### Backend
```bash
cd backend
dotnet publish -c Release -o ./publish
```

### Frontend
```bash
cd frontend
npm run build
```

The production build will be in `frontend/dist/`

## CLI Tool

The CLI tool allows you to manage workflows from the terminal:

```bash
cd backend/WorkflowAutomation.CLI
dotnet run -- login --email user@example.com --password yourpassword
dotnet run -- list-workflows
dotnet run -- execute-workflow --id <workflow-id>
```

## API Documentation

Once the backend is running, access the Swagger documentation at:
- **Swagger UI**: http://localhost:5000/swagger

## Architecture

The platform follows a clean architecture pattern with clear separation of concerns:

- **API Layer**: Controllers, middleware, SignalR hubs
- **Core Layer**: Domain entities, interfaces, DTOs
- **Infrastructure Layer**: Data access, external services
- **Engine Layer**: Workflow execution logic, node executors

For detailed architecture documentation, see `docs/devguide/devguide.md`

## Development Plan

The project follows a structured 61-phase development plan covering:

1. **Phase 1-20**: Core backend and infrastructure
2. **Phase 21-39**: Frontend development
3. **Phase 40-49**: Testing and documentation
4. **Phase 50-61**: Advanced features and production readiness

See `docs/devplan/development-plan.md` for the complete plan.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

[Specify your license here]

## Support

For issues and questions:
- GitHub Issues: [Create an issue]
- Documentation: `docs/` directory
- Development Guide: `docs/devguide/devguide.md`

## Roadmap

- [ ] Phase 1: Project Setup (Current)
- [ ] Phase 2-5: Core entities and data access
- [ ] Phase 6-9: Workflow execution engine
- [ ] Phase 10-15: API and real-time features
- [ ] Phase 16-19: AI integration and CLI
- [ ] Phase 20-39: Complete frontend
- [ ] Phase 40-49: Testing and documentation
- [ ] Phase 50-61: Advanced features and production deployment

See the complete roadmap in `docs/devplan/development-plan.md`

---

Built with ❤️ using .NET 8 and React 18
