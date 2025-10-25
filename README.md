# WPF FTP Client

A Windows FTP client developed using Windows Presentation Foundation (WPF).

## Appearance and Functionality

The application is a classic dual-pane file manager.

![FTP Client Main Window](./screenshots/main_window.jpg)

**Key UI Elements:**

*   **Connection Panel (left)**: Used to enter connection details for the FTP server.
*   **Local Files Panel (center)**: Displays the local computer's file system.
*   **Remote Files Panel (right)**: After a successful connection, this panel displays the files and folders on the FTP server.
*   **Toolbars**: Above each file panel, there are buttons for navigation (`<`, `>`, `↑`) and file management (create folder, rename, delete, download/upload).
*   **Log Panel (bottom)**: Shows the connection status, commands sent to the server, and its responses.

The application is built based on the **MVVM (Model-View-ViewModel)** architectural pattern.

## Project Structure

*   `Commands`: Implementation of the Command pattern (e.g., `RelayCommand`) for handling user actions.
*   `Converters`: Data converters for WPF bindings.
*   `Enums`: Enumerations defining types and states within the application.
*   `Helpers`: Helper classes and utilities.
*   `Models`: Classes describing data entities (e.g., file, log message).
*   `Resources`: Application resources, including styles (`.xaml`) and custom controls (`Controls`).
*   `Services`: Services encapsulating business logic (FTP operations, file system access, logging).
*   `ViewModels`: ViewModel classes that manage UI logic and state.
*   `Views`: XAML views (windows and user controls).

## How to Run the Project

### Requirements

*   [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the ".NET Desktop Development" workload installed.

### Launch Instructions

1.   **Clone the repository:**
    ``` git clone https://github.com/Drakollla/FTP_Client.git```

4.  **Open the project in Visual Studio:**
    Open the `FTP_Client.sln` (or `FTP_Client.csproj`) file in Visual Studio 2022.

5.  **Build the project:**
    Press `Ctrl+Shift+B` or select `Build -> Build Solution` from the menu. All necessary dependencies will be restored automatically.

6.  **Run the application:**
    Press `F5` or click the "Start" button on the toolbar to run the application in debug mode.

## Future Plans (TODO)

The project is complete as a learning exercise but has potential for further development. Below are possible improvements and new features.

### Key Features
- [ ] Support for secure protocols **SFTP** and **FTPS**.
- [ ] Implement a queue for background file uploads and downloads.
- [ ] Add a manager to save connection profiles.
- [ ] Implement a directory synchronization feature.

### UI Improvements
- [ ] Support for Drag-and-Drop between panels.
- [ ] Add a context menu for files and folders.
- [ ] Add a light theme option.
