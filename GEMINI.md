# Gemini Code Assistant Guide for Lithium

This guide provides instructions for Gemini, our AI code assistant, on how to effectively work with the Lithium project.

## About This Project

Lithium is a game client and server developed in C#. The project is structured into several components, including a core library, an Entity-Component-System (ECS) framework, a client, and a server. It uses a plugin-based architecture to allow for extensibility.

## Project Structure

The solution is organized into the following projects:

| Project                             | Description                                                                                               |
| ----------------------------------- | --------------------------------------------------------------------------------------------------------- |
| `Lithium.Client`                    | The game client.                                                                                          |
| `Lithium.Core`                      | Core functionalities and utilities shared across the solution.                                            |
| `Lithium.Core.ECS`                  | The Entity-Component-System (ECS) framework.                                                              |
| `Lithium.Core.ECS.Tests`            | Unit tests for the ECS framework.                                                                         |
| `Lithium.Server`                    | The game server, built with ASP.NET Core. It manages game logic, networking, and plugins.                 |
| `Lithium.Server.Core`               | Core functionalities for the server.                                                                      |

## Getting Started

### Prerequisites

*   [.NET 10.0 SDK](https://dotnet.microsoft.com/download) (Note: net10.0 seems to be a placeholder or a future version. Please verify the exact version required.)
*   [Docker](https://www.docker.com/products/docker-desktop) (Optional, for containerized deployment)

### Building the Project

To build the entire solution, run the following command from the root directory:

```bash
dotnet build
```

### Running the Server

#### Using dotnet CLI

To run the server directly, execute the following command:

```bash
dotnet run --project Lithium.Server/Lithium.Server.csproj
```

#### Using Docker

You can also run the server in a Docker container.

1.  Build the Docker image:
    ```bash
    docker-compose build
    ```
2.  Run the server:
    ```bash
    docker-compose up
    ```

The server will be available at `http://localhost:5000`.

### Running the Client

To run the client, execute the following command:

```bash
dotnet run --project Lithium.Client/Lithium.Client.csproj
```

## Configuration

The server can be configured through the following files in the `Lithium.Server` directory:
*   `appsettings.json`: Contains settings for logging, Sentry, and other ASP.NET Core configurations.
*   `config.json`: For game-specific configurations.

## Plugins

The server supports a plugin architecture.
*   Plugins are `.dll` files loaded at runtime.
*   A plugin is a class that implements `Lithium.Server.Core.IComponent`.
*   The `PluginManager` loads plugins from the `Build/Plugins` directory. Note: The current implementation has a hardcoded path, which might need to be adjusted.

## Development

### Running Tests

To run the unit tests for the project, use the following command:

```bash
dotnet test
```

### Coding Conventions

Please adhere to the existing coding style and conventions in the project. Analyze the surrounding code before making changes.

## CI/CD

This project uses GitHub Actions for continuous integration. The workflow is defined in `.github/workflows/discord.yml`, which sends notifications to a Discord channel on push and pull request events.

## Useful Commands

| Command                                                      | Description                                         |
| ------------------------------------------------------------ | --------------------------------------------------- |
| `dotnet build`                                               | Builds the entire solution.                         |
| `dotnet run --project Lithium.Server/Lithium.Server.csproj`  | Runs the game server.                               |
| `dotnet run --project Lithium.Client/Lithium.Client.csproj`  | Runs the game client.                               |
| `dotnet test`                                                | Runs all tests in the solution.                     |
| `docker-compose build`                                       | Builds the Docker image for the server.             |
| `docker-compose up`                                          | Starts the server in a Docker container.            |
