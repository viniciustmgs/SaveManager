# SaveManager

A simple desktop app for managing game save files, mainly for challenge running. Its functionalities 
are heavily based on the SousSpeedruns-Save-Organizer, I just built it as a fun side project, with
the difference that both single-file saves and whole-folder saves are supported. Global Hotkeys are 
only implemented for Windows, I still haven't made them work on Linux.

Built with .NET 10 and Avalonia 12 for Windows and Linux;
The macOS build has not been run on real hardware yet, so it has not been published.


## Requirements

### Windows

- **Normal build** — Needs the **[.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)**
  installed.
- **Self-contained build** — No requirements. The executable comes bundled with the .NET runtime. 
  For anyone who doesn't want to install the .NET runtime on their system.

### Linux

- The **[.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)**

## Running on Linux

The build is a single executable with no installer. Make it runnable once, 

```sh
chmod +x SaveManager
```
Then launch it:

```sh
./SaveManager
```
Or just double click on it, once it's runnable.

## Building from source

Requires the **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**.
<br>Clone the repository:

```sh
git clone https://github.com/viniciustmgs/SaveManager
```

### Building

Inside the repository root:

```sh
dotnet build SaveManager.slnx
```

### Running the tests

```sh
dotnet test SaveManager.slnx
```

### Publishing

Windows:

```powershell
.\publish.ps1                # both Windows builds
.\publish.ps1 -Only win-fd   # just normal build
.\publish.ps1 -Only win-sc   # just the self-contained build
```

Linux:

```sh
bash ./publish.sh
```

After publishing, the executable file lands in `publish/`.