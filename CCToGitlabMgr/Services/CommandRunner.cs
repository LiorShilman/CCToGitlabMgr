using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCToGitlabMgr.Services
{
    public class CommandResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public bool Success => ExitCode == 0;
    }

    public class CommandRunner
    {
        public event Action<string> OutputReceived;
        public event Action<string> ErrorReceived;

        private static readonly Lazy<string> _gitPath = new Lazy<string>(FindGitPath);

        public static string GitPath => _gitPath.Value;

        private static string FindGitPath()
        {
            // Try common Git for Windows locations
            var candidates = new[]
            {
                @"C:\Program Files\Git\bin\git.exe",
                @"C:\Program Files (x86)\Git\bin\git.exe",
                @"C:\Program Files\Git\cmd\git.exe",
                Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Programs\Git\bin\git.exe"),
                Environment.ExpandEnvironmentVariables(@"%ProgramW6432%\Git\bin\git.exe"),
            };

            foreach (var path in candidates)
            {
                if (System.IO.File.Exists(path))
                    return path;
            }

            // Fallback — assume git is on PATH
            return "git";
        }

        public async Task<CommandResult> RunGitAsync(string arguments, string workingDirectory = null, CancellationToken ct = default)
        {
            return await RunAsync(GitPath, arguments, workingDirectory, ct);
        }

        public async Task<CommandResult> RunAsync(string fileName, string arguments, string workingDirectory = null, CancellationToken ct = default)
        {
            var result = new CommandResult();
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            // Ensure git uses English output for parsing
            psi.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            psi.EnvironmentVariables["LC_ALL"] = "C";

            using (var process = new Process { StartInfo = psi })
            {
                var outputTcs = new TaskCompletionSource<bool>();
                var errorTcs = new TaskCompletionSource<bool>();

                process.OutputDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                    {
                        outputTcs.TrySetResult(true);
                        return;
                    }
                    outputBuilder.AppendLine(e.Data);
                    OutputReceived?.Invoke(e.Data);
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                    {
                        errorTcs.TrySetResult(true);
                        return;
                    }
                    errorBuilder.AppendLine(e.Data);
                    ErrorReceived?.Invoke(e.Data);
                };

                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    using (ct.Register(() => { try { process.Kill(); } catch { } }))
                    {
                        await Task.Run(() => process.WaitForExit(), ct);
                    }

                    // Wait for async output to flush
                    await Task.WhenAll(outputTcs.Task, errorTcs.Task);

                    result.ExitCode = process.ExitCode;
                }
                catch (OperationCanceledException)
                {
                    result.ExitCode = -1;
                    errorBuilder.AppendLine("Operation was cancelled.");
                }
                catch (Exception ex)
                {
                    result.ExitCode = -1;
                    errorBuilder.AppendLine($"Error: {ex.Message}");
                    ErrorReceived?.Invoke($"Error: {ex.Message}");
                }
            }

            result.Output = outputBuilder.ToString();
            result.Error = errorBuilder.ToString();
            return result;
        }

        /// <summary>
        /// Run a bash command via Git Bash
        /// </summary>
        public async Task<CommandResult> RunBashAsync(string script, string workingDirectory = null, CancellationToken ct = default)
        {
            var bashPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(GitPath) ?? "",
                "bash.exe"
            );

            if (!System.IO.File.Exists(bashPath))
            {
                // Try git's usr/bin
                var gitDir = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(GitPath));
                bashPath = System.IO.Path.Combine(gitDir ?? "", "usr", "bin", "bash.exe");
            }

            if (!System.IO.File.Exists(bashPath))
                bashPath = "bash";

            var escapedScript = script.Replace("\"", "\\\"");
            return await RunAsync(bashPath, $"-c \"{escapedScript}\"", workingDirectory, ct);
        }

        /// <summary>
        /// Quick synchronous check if git is available
        /// </summary>
        public static bool IsGitAvailable()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = GitPath,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(5000);
                    return p.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
