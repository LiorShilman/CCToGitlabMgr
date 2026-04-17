using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CCToGitlabMgr.Services
{
    public class CleanupResult
    {
        public int FilesRemoved { get; set; }
        public int DirectoriesRemoved { get; set; }
        public long BytesFreed { get; set; }
        public List<string> RemovedItems { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class AuditResult
    {
        public List<FileAuditItem> LargeFiles { get; set; } = new List<FileAuditItem>();
        public List<FileAuditItem> BinaryFiles { get; set; } = new List<FileAuditItem>();
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public string TotalSizeFormatted => FormatSize(TotalSize);

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1_073_741_824) return $"{bytes / 1_073_741_824.0:F1} GB";
            if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:F1} MB";
            if (bytes >= 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes} B";
        }
    }

    public class FileAuditItem
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public string SizeFormatted
        {
            get
            {
                if (Size >= 1_073_741_824) return $"{Size / 1_073_741_824.0:F1} GB";
                if (Size >= 1_048_576) return $"{Size / 1_048_576.0:F1} MB";
                if (Size >= 1024) return $"{Size / 1024.0:F1} KB";
                return $"{Size} B";
            }
        }
    }

    public class FileCleanupService
    {
        public event Action<string> Progress;

        // ClearCase artifact patterns
        private static readonly string[] ClearCaseDirs = { "lost+found", ".cc_data" };
        private static readonly string[] ClearCaseFilePatterns = { "*.keep", "*.keep.*", "*.contrib", "*.contrib.*", "view.dat", ".copyarea.*" };

        // Build artifact directories
        private static readonly string[] BuildDirs = { "bin", "obj", "Debug", "Release", "x64", "x86", ".vs", "packages", "node_modules" };
        private static readonly string[] BuildFilePatterns = { "*.suo", "*.user", "*.userosscache", "*.sln.docstates", "*.DotSettings.user" };

        // Binary file patterns for audit
        private static readonly string[] BinaryExtensions = { ".dll", ".exe", ".pdb", ".ilk", ".obj", ".o", ".so", ".lib" };

        /// <summary>
        /// Scan for ClearCase artifacts without deleting
        /// </summary>
        public List<string> ScanClearCaseArtifacts(string rootPath)
        {
            var items = new List<string>();
            if (!Directory.Exists(rootPath)) return items;

            ScanDirectories(rootPath, ClearCaseDirs, items);
            ScanFiles(rootPath, ClearCaseFilePatterns, items);
            return items;
        }

        /// <summary>
        /// Scan for build artifacts without deleting
        /// </summary>
        public List<string> ScanBuildArtifacts(string rootPath)
        {
            var items = new List<string>();
            if (!Directory.Exists(rootPath)) return items;

            ScanDirectories(rootPath, BuildDirs, items);
            ScanFiles(rootPath, BuildFilePatterns, items);
            return items;
        }

        /// <summary>
        /// Remove ClearCase artifacts
        /// </summary>
        public async Task<CleanupResult> CleanClearCaseArtifactsAsync(string rootPath, CancellationToken ct = default)
        {
            return await Task.Run(() =>
            {
                var result = new CleanupResult();
                Progress?.Invoke("Scanning for ClearCase artifacts...");

                RemoveDirectories(rootPath, ClearCaseDirs, result, ct);
                RemoveFiles(rootPath, ClearCaseFilePatterns, result, ct);

                Progress?.Invoke($"ClearCase cleanup complete: {result.FilesRemoved} files, {result.DirectoriesRemoved} directories removed.");
                return result;
            }, ct);
        }

        /// <summary>
        /// Remove build artifacts
        /// </summary>
        public async Task<CleanupResult> CleanBuildArtifactsAsync(string rootPath, CancellationToken ct = default)
        {
            return await Task.Run(() =>
            {
                var result = new CleanupResult();
                Progress?.Invoke("Scanning for build artifacts...");

                RemoveDirectories(rootPath, BuildDirs, result, ct);
                RemoveFiles(rootPath, BuildFilePatterns, result, ct);

                Progress?.Invoke($"Build cleanup complete: {result.FilesRemoved} files, {result.DirectoriesRemoved} directories removed.");
                return result;
            }, ct);
        }

        /// <summary>
        /// Audit files for large/binary items
        /// </summary>
        public async Task<AuditResult> AuditAsync(string rootPath, long largeFileThreshold = 10 * 1024 * 1024, CancellationToken ct = default)
        {
            return await Task.Run(() =>
            {
                var result = new AuditResult();
                if (!Directory.Exists(rootPath)) return result;

                Progress?.Invoke("Auditing project files...");

                foreach (var file in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
                {
                    ct.ThrowIfCancellationRequested();

                    try
                    {
                        var fi = new FileInfo(file);
                        result.TotalFiles++;
                        result.TotalSize += fi.Length;

                        if (fi.Length > largeFileThreshold)
                        {
                            var relativePath = GetRelativePath(rootPath, file);
                            result.LargeFiles.Add(new FileAuditItem { Path = relativePath, Size = fi.Length });
                        }

                        var ext = fi.Extension.ToLowerInvariant();
                        if (BinaryExtensions.Contains(ext))
                        {
                            var relativePath = GetRelativePath(rootPath, file);
                            result.BinaryFiles.Add(new FileAuditItem { Path = relativePath, Size = fi.Length });
                        }
                    }
                    catch { }
                }

                result.LargeFiles = result.LargeFiles.OrderByDescending(f => f.Size).ToList();
                Progress?.Invoke($"Audit complete: {result.TotalFiles} files, {result.TotalSizeFormatted} total, {result.LargeFiles.Count} large files, {result.BinaryFiles.Count} binary files.");
                return result;
            }, ct);
        }

        /// <summary>
        /// Calculate folder size
        /// </summary>
        public static long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long size = 0;
            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try { size += new FileInfo(file).Length; } catch { }
                }
            }
            catch { }
            return size;
        }

        public static string FormatSize(long bytes)
        {
            if (bytes >= 1_073_741_824) return $"{bytes / 1_073_741_824.0:F1} GB";
            if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:F1} MB";
            if (bytes >= 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes} B";
        }

        // === Private helpers ===

        private void ScanDirectories(string root, string[] dirNames, List<string> results)
        {
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
                {
                    var name = Path.GetFileName(dir);
                    if (dirNames.Any(d => string.Equals(d, name, StringComparison.OrdinalIgnoreCase)))
                        results.Add(dir);
                }
            }
            catch { }
        }

        private void ScanFiles(string root, string[] patterns, List<string> results)
        {
            foreach (var pattern in patterns)
            {
                try
                {
                    foreach (var file in Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories))
                        results.Add(file);
                }
                catch { }
            }
        }

        private void RemoveDirectories(string root, string[] dirNames, CleanupResult result, CancellationToken ct)
        {
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories).ToList())
                {
                    ct.ThrowIfCancellationRequested();
                    var name = Path.GetFileName(dir);
                    if (!dirNames.Any(d => string.Equals(d, name, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    if (!Directory.Exists(dir)) continue;

                    try
                    {
                        var size = GetDirectorySize(dir);
                        Directory.Delete(dir, true);
                        result.DirectoriesRemoved++;
                        result.BytesFreed += size;
                        result.RemovedItems.Add(dir);
                        Progress?.Invoke($"  Removed directory: {GetRelativePath(root, dir)}");
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"{dir}: {ex.Message}");
                    }
                }
            }
            catch { }
        }

        private void RemoveFiles(string root, string[] patterns, CleanupResult result, CancellationToken ct)
        {
            foreach (var pattern in patterns)
            {
                try
                {
                    foreach (var file in Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories).ToList())
                    {
                        ct.ThrowIfCancellationRequested();
                        if (!File.Exists(file)) continue;

                        try
                        {
                            var size = new FileInfo(file).Length;
                            File.Delete(file);
                            result.FilesRemoved++;
                            result.BytesFreed += size;
                            result.RemovedItems.Add(file);
                            Progress?.Invoke($"  Removed: {GetRelativePath(root, file)}");
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"{file}: {ex.Message}");
                        }
                    }
                }
                catch { }
            }
        }

        private static string GetRelativePath(string root, string fullPath)
        {
            if (fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                var rel = fullPath.Substring(root.Length);
                return rel.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            return fullPath;
        }
    }
}
