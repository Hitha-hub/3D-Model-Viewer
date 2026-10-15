<<<<<<< HEAD
# 3D Equipment Viewer and Manager

A small C# WPF desktop application using Helix Toolkit.

## Requirements
- Windows
- .NET 8 SDK
- VS Code with C# Dev Kit, or Visual Studio 2022
- Internet connection for the first NuGet restore

## Run
Open this folder in VS Code, then run:

```powershell
dotnet restore
dotnet build
dotnet run
```

## Features
- 3D viewport with mouse navigation
- Sample tank and box equipment
- Select equipment from the 3D scene or list
- Add and remove equipment
- Edit equipment name and status
- Status-based colors: Running (blue), Warning (orange), Stopped (red)
- Save and load scene data as JSON
- Basic validation and error dialogs

## Project files
- `MainWindow.xaml`: UI and Helix Toolkit viewport
- `MainWindow.xaml.cs`: scene creation, selection, equipment operations, JSON persistence
- `Equipment.cs`: equipment model
- `App.xaml`: application startup

This project uses the Helix Toolkit NuGet package. It is not the full Helix Toolkit source repository. Understand, test, and extend the code before listing it as your own portfolio work.
=======
# 3D-Model-Viewer
>>>>>>> 66339995c118391a798d2584668e0fd8ccc4236d
