# NitroMonitor

**NitroMonitor** is a Linux desktop application designed to provide a centralized and visually appealing interface for monitoring and managing hardware resources, especially on gaming laptops.

The goal of this project is to bring Linux users an experience similar to the hardware control and monitoring software commonly available on gaming laptops running Windows, bringing all essential information and controls together in a single screen.

## Features

- **CPU Monitoring:** Real-time CPU temperature and usage monitoring.
- **GPU Monitoring:** Real-time GPU temperature and usage monitoring.
- **RAM Monitoring:** Displays memory statistics, including:
  - Free memory.
  - Used memory.
  - Shared memory.
  - Buff/cache.
- **Fan Control:** Manage fan speeds manually (0–100%) or switch to automatic mode.
- **Network Monitoring:** Real-time internet upload and download speeds, as well as network latency.

## Screenshots

Here is a screenshot of NitroMonitor's interface:

![NitroMonitor Main Interface](screenshots/main.png)

## Technologies

- **Framework:** Avalonia UI (.NET)
- **Programming Language:** C#
- **Design Pattern:** MVVM (Model-View-ViewModel)

## Requirements

Before running NitroMonitor, make sure you have the following installed:

- Linux operating system.
- [NBFC-Linux (NoteBook FanControl)](https://github.com/nbfc-linux/nbfc-linux).

NBFC-Linux is required for fan management functionality.

**Important:** NitroMonitor must be executed with root privileges using `sudo` for hardware monitoring and fan control functionality to work correctly.

## Installation

You have two options to get NitroMonitor:

### Option 1: Use a precompiled version

You can find a precompiled version of NitroMonitor in the `compiled/` directory of this repository.

Download or extract the appropriate build and run the application using:

```bash
sudo ./NitroMonitor
```

### Option 2: Build from source

You can compile NitroMonitor yourself using the .NET SDK.

1. Clone the repository:

   ```bash
   git clone <REPOSITORY_URL>
   ```

2. Navigate to the project directory:

   ```bash
   cd NitroMonitor
   ```

3. Build the project:

   ```bash
   dotnet build
   ```

4. Run the application with root privileges:

   ```bash
   sudo dotnet run
   ```

For more information about Avalonia and .NET, refer to their official documentation.

## Architecture

NitroMonitor uses the **MVVM (Model-View-ViewModel)** design pattern with Avalonia UI.

Currently, the application interface is centralized in `MainWindow`, rather than being separated into multiple View components.

## License

NitroMonitor is intended to be free to use, modify, and distribute.

If you create your own version or derivative project based on NitroMonitor, giving credit to the original project is greatly appreciated, although it is not mandatory.

**License note:** To make these permissions legally clear, consider publishing the project under the MIT License. It allows commercial and non-commercial use, modification, and redistribution, without requiring attribution to the original author. Attribution can instead be requested as a courtesy.

If you find NitroMonitor useful, consider giving the repository a star on GitHub!
