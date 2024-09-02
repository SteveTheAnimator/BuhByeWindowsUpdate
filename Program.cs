using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Do you want to enable or disable Windows Updates? (Type 'enable' or 'disable')");
            string? userChoice = Console.ReadLine()?.ToLower();

            if (userChoice == "disable")
            {
                DisableWindowsUpdates(args);
                Console.WriteLine("Windows Update has been disabled.");
            }
            else if (userChoice == "enable")
            {
                EnableWindowsUpdates();
                Console.WriteLine("Windows Update has been enabled.");
            }
            else
            {
                Console.WriteLine("Invalid choice. Please type 'enable' or 'disable'.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void DisableUpdateServices()
    {
        var services = new[] { "wuauserv", "UsoSvc", "uhssvc", "WaaSMedicSvc" };
        foreach (var service in services)
        {
            ExecuteCommand($"net stop {service}");
            ExecuteCommand($"sc config {service} start= disabled");
            ExecuteCommand($"sc failure {service} reset= 0 actions= \"\"");
        }
    }

    static void BruteForceRenameServices()
    {
        var services = new[] { "WaaSMedicSvc", "wuaueng" };
        foreach (var service in services)
        {
            var dllPath = Path.Combine(Environment.SystemDirectory, $"{service}.dll");

            ExecuteCommand($"takeown /f \"{dllPath}\"");
            ExecuteCommand($"icacls \"{dllPath}\" /grant *S-1-1-0:F");
            File.Move(dllPath, dllPath.Replace(".dll", "_BAK.dll"));
            ExecuteCommand($"icacls \"{dllPath.Replace(".dll", "_BAK.dll")}\" /setowner \"NT SERVICE\\TrustedInstaller\"");
            ExecuteCommand($"icacls \"{dllPath.Replace(".dll", "_BAK.dll")}\" /remove *S-1-1-0");
        }
    }

    static void UpdateRegistry()
    {
        ExecuteCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\WaaSMedicSvc\" /v Start /t REG_DWORD /d 4 /f");
        ExecuteCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\WaaSMedicSvc\" /v FailureActions /t REG_BINARY /d 000000000000000000000000030000001400000000000000c0d4010000000000e09304000000000000000000 /f");
        ExecuteCommand("reg add \"HKLM\\Software\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU\" /v NoAutoUpdate /t REG_DWORD /d 1 /f");
    }

    static void DeleteDownloadedUpdates()
    {
        var updatePath = @"c:\windows\softwaredistribution";
        ExecuteCommand($"erase /f /s /q {updatePath}\\*.*");
        ExecuteCommand($"rmdir /s /q {updatePath}");
    }

    static void DisableScheduledTasks()
    {
        var taskPaths = new[]
        {
            @"\Microsoft\Windows\InstallService\*",
            @"\Microsoft\Windows\UpdateOrchestrator\*",
            @"\Microsoft\Windows\UpdateAssistant\*",
            @"\Microsoft\Windows\WaaSMedic\*",
            @"\Microsoft\Windows\WindowsUpdate\*",
            @"\Microsoft\WindowsUpdate\*"
        };

        foreach (var taskPath in taskPaths)
        {
            ExecuteCommand($"powershell -command \"Get-ScheduledTask -TaskPath '{taskPath}' | Disable-ScheduledTask\"");
        }
    }

    static void DisableWindowsUpdates(string[] args)
    {
        DisableService("wuauserv");
        DisableService("WaaSMedicSvc");
        DisableService("UsoSvc");

        ExecuteCommand("reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU\" /v NoAutoUpdate /t REG_DWORD /d 1 /f");

        DisableUpdateServices();
        BruteForceRenameServices();
        UpdateRegistry();
        DeleteDownloadedUpdates();
        DisableScheduledTasks();
    }

    static void EnableWindowsUpdates()
    {
        EnableService("wuauserv");
        EnableService("WaaSMedicSvc");
        EnableService("UsoSvc");

        EnableUpdateServices();
        RestoreRenamedServices();
        UpdateRegistrySecondly();
        EnableScheduledTasks();

        ExecuteCommand("reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU\" /v NoAutoUpdate /t REG_DWORD /d 0 /f");
    }

    static void DisableService(string serviceName)
    {
        if (IsServiceRunning(serviceName))
        {
            ExecuteCommand($"sc stop {serviceName}");
        }
        else
        {
            Console.WriteLine($"{serviceName} is already stopped!");
        }
        ExecuteCommand($"sc config {serviceName} start= disabled");
    }

    static void EnableService(string serviceName)
    {
        ExecuteCommand($"sc config {serviceName} start= auto");
        ExecuteCommand($"sc start {serviceName}");
    }

    static bool IsServiceRunning(string serviceName)
    {
        return ExecuteCommand($"sc query {serviceName}") == 0;
    }

    static bool TaskExists(string taskName)
    {
        return ExecuteCommand($"schtasks /query /TN {taskName}") == 0;
    }
    static void EnableUpdateServices()
    {
        ExecuteCommand("sc config wuauserv start= auto");
        ExecuteCommand("sc config UsoSvc start= auto");
        ExecuteCommand("sc config uhssvc start= delayed-auto");
    }

    static void RestoreRenamedServices()
    {
        var services = new[] { "WaaSMedicSvc", "wuaueng" };
        foreach (var service in services)
        {
            var dllPathBak = Path.Combine(Environment.SystemDirectory, $"{service}_BAK.dll");
            var dllPath = Path.Combine(Environment.SystemDirectory, $"{service}.dll");

            ExecuteCommand($"takeown /f \"{dllPathBak}\"");
            ExecuteCommand($"icacls \"{dllPathBak}\" /grant *S-1-1-0:F");
            File.Move(dllPathBak, dllPath);
            ExecuteCommand($"icacls \"{dllPath}\" /setowner \"NT SERVICE\\TrustedInstaller\"");
            ExecuteCommand($"icacls \"{dllPath}\" /remove *S-1-1-0");
        }
    }

    static void UpdateRegistrySecondly()
    {
        ExecuteCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\WaaSMedicSvc\" /v Start /t REG_DWORD /d 3 /f");
        ExecuteCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\WaaSMedicSvc\" /v FailureActions /t REG_BINARY /d 840300000000000000000000030000001400000001000000c0d4010001000000e09304000000000000000000 /f");
        ExecuteCommand("reg delete \"HKLM\\Software\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU\" /v \"NoAutoUpdate\" /f");
    }

    static void EnableScheduledTasks()
    {
        var taskPaths = new[]
        {
            @"\Microsoft\Windows\InstallService\*",
            @"\Microsoft\Windows\UpdateOrchestrator\*",
            @"\Microsoft\Windows\UpdateAssistant\*",
            @"\Microsoft\Windows\WaaSMedic\*",
            @"\Microsoft\Windows\WindowsUpdate\*",
            @"\Microsoft\WindowsUpdate\*"
        };

        foreach (var taskPath in taskPaths)
        {
            ExecuteCommand($"powershell -command \"Get-ScheduledTask -TaskPath '{taskPath}' | Enable-ScheduledTask\"");
        }
    }

    static int ExecuteCommand(string command)
    {
        ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
        {
            CreateNoWindow = false,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using (Process process = Process.Start(processInfo))
        {
            process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}
