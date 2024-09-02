# BuhByeWindowsUpdate

A Console C# Program to enable and disable windows updates!

## Features

1. **Disable Windows Updates**
   - Stops and disables critical Windows Update services.
   - Renames service DLL files to prevent them from being loaded.
   - Updates the Windows registry to prevent automatic updates.
   - Deletes downloaded updates from the system.
   - Disables scheduled tasks related to Windows Updates.

2. **Enable Windows Updates**
   - Starts and enables critical Windows Update services.
   - Restores any renamed service DLL files.
   - Updates the Windows registry to allow automatic updates.
   - Re-enables scheduled tasks related to Windows Updates.

## How to Use

### Running the Program

**Open Command Prompt as Administrator**
   - To execute the commands that modify services and system settings, you need administrative privileges.

### User Interaction
When you run the program, it will prompt you with a choice:
```cs
Do you want to enable or disable Windows Updates? (Type 'enable' or 'disable')
```
   
- **Type `disable`**: This will disable Windows Updates. The program will:
  - Stop and disable update services.
  - Rename related DLL files.
  - Update the registry to prevent automatic updates.
  - Delete downloaded updates.
  - Disable related scheduled tasks.

- **Type `enable`**: This will enable Windows Updates. The program will:
  - Start and enable update services.
  - Restore renamed DLL files.
  - Update the registry to allow automatic updates.
  - Re-enable related scheduled tasks.

If you type anything other than `enable` or `disable`, the program will prompt you to provide a valid choice.

### Error Handling

The program is designed to catch and display errors. If something goes wrong during execution, it will output the error message to help with troubleshooting.

## Detailed Functionality

### Disabling Windows Updates

- **DisableUpdateServices()**: Stops and disables Windows Update services and sets their start mode to disabled.
- **BruteForceRenameServices()**: Renames critical update-related DLL files to prevent them from being used.
- **UpdateRegistry()**: Updates the Windows registry to prevent automatic updates.
- **DeleteDownloadedUpdates()**: Deletes downloaded updates from the system.
- **DisableScheduledTasks()**: Disables scheduled tasks related to Windows Updates.

### Enabling Windows Updates

- **EnableUpdateServices()**: Starts and enables Windows Update services and sets their start mode to auto.
- **RestoreRenamedServices()**: Restores any renamed DLL files.
- **UpdateRegistrySecondly()**: Updates the Windows registry to allow automatic updates.
- **EnableScheduledTasks()**: Re-enables scheduled tasks related to Windows Updates.

## Notes

- Ensure that you have administrative privileges when running the program.
- Use this tool with caution as it modifies system settings and services.
