# Lithium CLR

**Lithium CLR** is a C# rewrite of the Hytale server, replacing Java with a high-performance, Common Language Runtime (CLR) designed for scalability, stability, and modern tooling.

## Features

- **Full C# Rewrite**: Complete server reimplementation originally in Java, now in C#.
- **Performance Focused**: Reduced memory allocations and high-speed packet handling.
- **Plugin-Friendly**: Designed to allow extensions and custom plugins.
- **Cross-Platform**: Works on Windows, Linux, and macOS.

## Development Roadmap

Here is the current development roadmap, with progress tracked using checkboxes:

- [x] **Experiment with UDP/QUIC networking**  
  - Testing packet transmission, latency, and reliability.  
- [ ] **Define server architecture**  
  - Decide on module separation, packet handling, threading, and memory management.  
- [ ] **Server interpretation (Java → C#)**  
  - Once the official Hytale Java server is available, analyze and interpret the logic in C#.  
- [ ] **Zero-cost optimization rewrite**  
  - Rewrite core systems for minimal allocations, maximum speed, and low memory footprint.  
- [ ] **Advanced features**  
  - Dashboard for server management (player tracking, logs, plugins).  
  - Plugin system with event handling and safe execution.  
  - High-performance networking and multithreading.  
- [ ] **Testing & stability**  
  - Load testing, memory profiling, and stress tests.  
- [ ] **Documentation & community tools**  
  - API references, plugin guides, and tutorials.

> The roadmap is subject to change as experimentation progresses and the official Java server becomes available.

## Installation

1. Clone the repository:

```bash
git clone https://github.com/lithium-clr/LithiumCLR.git
