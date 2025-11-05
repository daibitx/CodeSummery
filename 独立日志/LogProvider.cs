using System.Diagnostics;
using System.Reflection;

namespace CommonUtils
{
    public class LogProvider
    {
        private static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();

        // 日志路径和文件名
        private static readonly string logDirectory = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "log", $"{Assembly.GetExecutingAssembly().GetName().Name}");
        private static string logFileNameBase = $"{Assembly.GetExecutingAssembly().GetName().Name}";
        private static readonly string logFileExtension = ".txt";
        private static string? currentLogFile = null;
        private const long MaxFileSize = 10 * 1024 * 1024;
        private const int MaxLogFiles = 30;
        public static void Info(string message) => WriteLog("INFO", message);
        public static void Error(string message) => WriteLog("ERROR", message);
        public static void Fatal(string message) => WriteLog("FATAL", message);
        public static void Warning(string message) => WriteLog("WARNING", message);

        private static void WriteLog(string level, string message)
        {
            try
            {
                LogWriteLock.EnterWriteLock();

                EnsureLogDirectory();

                if (currentLogFile == null || !File.Exists(currentLogFile) || new FileInfo(currentLogFile).Length > MaxFileSize)
                {
                    currentLogFile = GetNextLogFile();
                    CleanupOldLogs();
                }

                using (var stream = new FileStream(currentLogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} >> {level} >> {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logger] Failed to write log: {ex}");
            }
            finally
            {
                LogWriteLock.ExitWriteLock();
            }
        }

        private static void EnsureLogDirectory()
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
                if (IsLinux()) SetFilePermissions(logDirectory, "777");
            }
        }

        private static string GetNextLogFile()
        {
            EnsureLogDirectory();

            var logFiles = Directory.GetFiles(logDirectory, $"{logFileNameBase}_*{logFileExtension}")
                                    .Select(f => new FileInfo(f))
                                    .OrderByDescending(f =>
                                    {
                                        var name = Path.GetFileNameWithoutExtension(f.Name);
                                        var indexStr = name.Split('_').LastOrDefault();
                                        return int.TryParse(indexStr, out int idx) ? idx : 0;
                                    })
                                    .ToList();

            if (logFiles.Any())
            {
                var latestLog = logFiles.First();
                if (latestLog.Length < MaxFileSize)
                {
                    return latestLog.FullName;
                }

                int nextIndex = ExtractIndex(latestLog.Name) + 1;
                return Path.Combine(logDirectory, $"{logFileNameBase}_{nextIndex}{logFileExtension}");
            }
            return Path.Combine(logDirectory, $"{logFileNameBase}_1{logFileExtension}");
        }

        private static void CleanupOldLogs()
        {
            var logFiles = Directory.GetFiles(logDirectory, $"{logFileNameBase}_*{logFileExtension}")
                                    .Select(f => new FileInfo(f))
                                    .OrderBy(f => f.CreationTime)
                                    .ToList();

            while (logFiles.Count > MaxLogFiles)
            {
                try
                {
                    logFiles.First().Delete();
                    logFiles.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Logger] Failed to delete old log: {ex}");
                }
            }
        }

        private static int ExtractIndex(string fileName)
        {
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var parts = baseName.Split('_');
            return int.TryParse(parts.LastOrDefault(), out int index) ? index : 0;
        }

        private static void SetFilePermissions(string path, string permissions)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"{permissions} \"{path}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"[Logger] chmod failed: {process.StandardError.ReadToEnd()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logger] Failed to set permissions: {ex}");
            }
        }

        private static bool IsLinux() =>
            Environment.OSVersion.Platform == PlatformID.Unix ||
            Environment.OSVersion.Platform == PlatformID.MacOSX;
    }
}
