using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CCToGitlabMgr.Models;
using CCToGitlabMgr.Services;

namespace CCToGitlabMgr.ViewModels
{
    public class StepInfo : BaseViewModel
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }

        private string _status = "Pending";
        public string Status { get => _status; set => SetProperty(ref _status, value); }
    }

    public class MainViewModel : BaseViewModel
    {
        // === Services ===
        public CommandRunner Runner { get; }
        public GitService Git { get; }
        public FileCleanupService Cleanup { get; }

        // === State ===
        public MigrationContext Context { get; } = new MigrationContext();

        private int _currentStepIndex;
        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set
            {
                if (SetProperty(ref _currentStepIndex, value))
                {
                    OnPropertyChanged(nameof(CurrentStep));
                    OnPropertyChanged(nameof(CanGoBack));
                    OnPropertyChanged(nameof(CanGoForward));
                    OnPropertyChanged(nameof(StepProgress));
                }
            }
        }

        public StepInfo CurrentStep => Steps.Count > 0 && _currentStepIndex < Steps.Count
            ? Steps[_currentStepIndex] : null;

        public bool CanGoBack => _currentStepIndex > 0;
        public bool CanGoForward => _currentStepIndex < Steps.Count - 1;
        public string StepProgress => $"{_currentStepIndex + 1} / {Steps.Count}";

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); } }

        private string _busyMessage = "";
        public string BusyMessage { get => _busyMessage; set => SetProperty(ref _busyMessage, value); }

        private string _gitVersion;
        public string GitVersion { get => _gitVersion; set => SetProperty(ref _gitVersion, value); }

        // === Output Console ===
        private string _consoleOutput = "";
        public string ConsoleOutput { get => _consoleOutput; set => SetProperty(ref _consoleOutput, value); }

        // === Steps ===
        public ObservableCollection<StepInfo> Steps { get; } = new ObservableCollection<StepInfo>();

        // === Commands ===
        public RelayCommand NextStepCommand { get; private set; }
        public RelayCommand PrevStepCommand { get; private set; }
        public RelayCommand GoToStepCommand { get; private set; }
        public RelayCommand ClearConsoleCommand { get; private set; }

        // Step 1
        public AsyncRelayCommand ApplyGitConfigCommand { get; private set; }
        // Step 2
        public AsyncRelayCommand BrowseSourceCommand { get; private set; }
        public AsyncRelayCommand BrowseStagingCommand { get; private set; }
        public AsyncRelayCommand CopyToStagingCommand { get; private set; }
        // Step 3
        public AsyncRelayCommand ScanArtifactsCommand { get; private set; }
        public AsyncRelayCommand CleanClearCaseCommand { get; private set; }
        public AsyncRelayCommand CleanBuildCommand { get; private set; }
        public AsyncRelayCommand AuditFilesCommand { get; private set; }
        // Step 4
        public AsyncRelayCommand PlaceGitignoreCommand { get; private set; }
        public AsyncRelayCommand PreviewGitignoreCommand { get; private set; }
        // Step 6
        public AsyncRelayCommand RunMigrationCommand { get; private set; }
        public AsyncRelayCommand DryRunCheckCommand { get; private set; }
        // Step 7
        public AsyncRelayCommand RunVerifyCommand { get; private set; }
        public AsyncRelayCommand BrowseVerifyCommand { get; private set; }

        private CancellationTokenSource _cts;

        public MainViewModel()
        {
            Runner = new CommandRunner();
            Git = new GitService(Runner);
            Cleanup = new FileCleanupService();

            // Wire up output
            Git.Output += AppendOutput;
            Cleanup.Progress += AppendOutput;

            InitializeSteps();
            InitializeCommands();
            _ = CheckGitAsync();
        }

        private void InitializeSteps()
        {
            Steps.Add(new StepInfo { Number = 1, Title = "Git Config", Icon = "+" });
            Steps.Add(new StepInfo { Number = 2, Title = "Prepare", Icon = ">" });
            Steps.Add(new StepInfo { Number = 3, Title = "Clean", Icon = "X" });
            Steps.Add(new StepInfo { Number = 4, Title = "Gitignore", Icon = "." });
            Steps.Add(new StepInfo { Number = 5, Title = "GitLab", Icon = "G" });
            Steps.Add(new StepInfo { Number = 6, Title = "Migrate", Icon = "!" });
            Steps.Add(new StepInfo { Number = 7, Title = "Verify", Icon = "?" });
            Steps.Add(new StepInfo { Number = 8, Title = "Checklist", Icon = "#" });
        }

        private void InitializeCommands()
        {
            NextStepCommand = new RelayCommand(() =>
            {
                if (CanGoForward) CurrentStepIndex++;
            }, () => CanGoForward && !IsBusy);

            PrevStepCommand = new RelayCommand(() =>
            {
                if (CanGoBack) CurrentStepIndex--;
            }, () => CanGoBack && !IsBusy);

            GoToStepCommand = new RelayCommand(p =>
            {
                int idx = -1;
                if (p is int i) idx = i - 1;       // Number is 1-based
                else if (p is string s && int.TryParse(s, out int si)) idx = si - 1;
                if (idx >= 0 && idx < Steps.Count)
                    CurrentStepIndex = idx;
            });

            ClearConsoleCommand = new RelayCommand(() => ConsoleOutput = "");

            // Step 1
            ApplyGitConfigCommand = new AsyncRelayCommand(ApplyGitConfigAsync, () => !IsBusy);

            // Step 2
            BrowseSourceCommand = new AsyncRelayCommand(() => { BrowseFolder(p => Context.SourcePath = p); return Task.CompletedTask; });
            BrowseStagingCommand = new AsyncRelayCommand(() => { BrowseFolder(p => Context.StagingPath = p); return Task.CompletedTask; });
            CopyToStagingCommand = new AsyncRelayCommand(CopyToStagingAsync, () => !IsBusy);

            // Step 3
            ScanArtifactsCommand = new AsyncRelayCommand(ScanArtifactsAsync, () => !IsBusy);
            CleanClearCaseCommand = new AsyncRelayCommand(CleanClearCaseAsync, () => !IsBusy);
            CleanBuildCommand = new AsyncRelayCommand(CleanBuildAsync, () => !IsBusy);
            AuditFilesCommand = new AsyncRelayCommand(AuditFilesAsync, () => !IsBusy);

            // Step 4
            PlaceGitignoreCommand = new AsyncRelayCommand(PlaceGitignoreAsync, () => !IsBusy);
            PreviewGitignoreCommand = new AsyncRelayCommand(() =>
            {
                var template = GitignoreTemplates.GetTemplate(Context.GitignoreTemplate);
                AppendOutput("=== .gitignore Preview ===");
                AppendOutput(template);
                AppendOutput("=== End Preview ===");
                return Task.CompletedTask;
            });

            // Step 6
            RunMigrationCommand = new AsyncRelayCommand(RunMigrationAsync, () => !IsBusy);
            DryRunCheckCommand = new AsyncRelayCommand(DryRunCheckAsync, () => !IsBusy);

            // Step 7
            RunVerifyCommand = new AsyncRelayCommand(RunVerifyAsync, () => !IsBusy);
            BrowseVerifyCommand = new AsyncRelayCommand(() => { BrowseFolder(p => Context.VerifyPath = p); return Task.CompletedTask; });
        }

        // === Command Implementations ===

        private async Task CheckGitAsync()
        {
            GitVersion = await Git.GetVersionAsync() ?? "Git not found!";
        }

        private async Task ApplyGitConfigAsync()
        {
            if (string.IsNullOrWhiteSpace(Context.UserName) || string.IsNullOrWhiteSpace(Context.UserEmail))
            {
                AppendOutput("ERROR: Please fill in User Name and Email.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Applying Git configuration...";
            _cts = new CancellationTokenSource();

            try
            {
                await Git.ApplyRecommendedConfigAsync(Context.UserName, Context.UserEmail, _cts.Token);
                var result = await Git.GetGlobalConfigAsync(_cts.Token);
                Steps[0].Status = "Completed";
                AppendOutput("Git configuration applied successfully.");
            }
            catch (Exception ex)
            {
                AppendOutput($"ERROR: {ex.Message}");
                Steps[0].Status = "Error";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CopyToStagingAsync()
        {
            if (string.IsNullOrWhiteSpace(Context.SourcePath) || !Directory.Exists(Context.SourcePath))
            {
                AppendOutput("ERROR: Source path is invalid.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Context.StagingPath))
            {
                AppendOutput("ERROR: Staging path is empty.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Copying to staging...";
            _cts = new CancellationTokenSource();

            try
            {
                AppendOutput($"Copying from: {Context.SourcePath}");
                AppendOutput($"         to: {Context.StagingPath}");

                var sizeBefore = FileCleanupService.GetDirectorySize(Context.SourcePath);
                Context.ProjectSizeBefore = FileCleanupService.FormatSize(sizeBefore);

                await Task.Run(() => CopyDirectory(Context.SourcePath, Context.StagingPath), _cts.Token);

                // Auto-detect .sln
                var slnPath = SlnParser.FindSln(Context.StagingPath);
                if (slnPath != null)
                {
                    Context.SlnFilePath = slnPath;
                    var info = SlnParser.Parse(slnPath);
                    if (info != null)
                    {
                        Context.VsVersion = info.DetectedVS;
                        Context.GitignoreTemplate = info.RecommendedGitignore;
                        AppendOutput($"Detected: Visual Studio {info.DetectedVS} (Format {info.FormatVersion})");
                        AppendOutput($"Recommended gitignore: {info.RecommendedGitignore}");
                    }
                }
                else
                {
                    AppendOutput("Warning: No .sln file found.");
                }

                if (string.IsNullOrWhiteSpace(Context.ProjectName))
                    Context.ProjectName = Path.GetFileName(Context.StagingPath);

                Steps[1].Status = "Completed";
                AppendOutput("Copy complete.");
            }
            catch (Exception ex)
            {
                AppendOutput($"ERROR: {ex.Message}");
                Steps[1].Status = "Error";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ScanArtifactsAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                AppendOutput("ERROR: Staging path is invalid. Complete Step 2 first.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Scanning...";

            await Task.Run(() =>
            {
                var ccItems = Cleanup.ScanClearCaseArtifacts(path);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AppendOutput($"=== ClearCase artifacts found: {ccItems.Count} ===");
                    foreach (var item in ccItems) AppendOutput($"  {item}");
                });

                var buildItems = Cleanup.ScanBuildArtifacts(path);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AppendOutput($"=== Build artifacts found: {buildItems.Count} ===");
                    foreach (var item in buildItems) AppendOutput($"  {item}");
                });
            });

            IsBusy = false;
        }

        private async Task CleanClearCaseAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                AppendOutput("ERROR: Staging path is invalid.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Cleaning ClearCase artifacts...";
            _cts = new CancellationTokenSource();

            try
            {
                var result = await Cleanup.CleanClearCaseArtifactsAsync(path, _cts.Token);
                Context.ClearCaseFilesRemoved = result.FilesRemoved + result.DirectoriesRemoved;
                AppendOutput($"Freed: {FileCleanupService.FormatSize(result.BytesFreed)}");
                if (result.Errors.Count > 0)
                {
                    AppendOutput("Errors:");
                    foreach (var err in result.Errors) AppendOutput($"  {err}");
                }
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task CleanBuildAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                AppendOutput("ERROR: Staging path is invalid.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Cleaning build artifacts...";
            _cts = new CancellationTokenSource();

            try
            {
                var result = await Cleanup.CleanBuildArtifactsAsync(path, _cts.Token);
                Context.BuildArtifactsRemoved = result.FilesRemoved + result.DirectoriesRemoved;

                var sizeAfter = FileCleanupService.GetDirectorySize(path);
                Context.ProjectSizeAfter = FileCleanupService.FormatSize(sizeAfter);

                AppendOutput($"Freed: {FileCleanupService.FormatSize(result.BytesFreed)}");
                AppendOutput($"Project size: {Context.ProjectSizeAfter}");

                Steps[2].Status = "Completed";
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task AuditFilesAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                AppendOutput("ERROR: Staging path is invalid.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Auditing files...";
            _cts = new CancellationTokenSource();

            try
            {
                var audit = await Cleanup.AuditAsync(path, ct: _cts.Token);
                AppendOutput($"Total: {audit.TotalFiles} files, {audit.TotalSizeFormatted}");

                if (audit.LargeFiles.Count > 0)
                {
                    AppendOutput($"\nLarge files (>10MB): {audit.LargeFiles.Count}");
                    foreach (var f in audit.LargeFiles)
                        AppendOutput($"  {f.SizeFormatted,10}  {f.Path}");
                }

                if (audit.BinaryFiles.Count > 0)
                {
                    AppendOutput($"\nBinary files (dll/exe/pdb): {audit.BinaryFiles.Count}");
                    foreach (var f in audit.BinaryFiles)
                        AppendOutput($"  {f.SizeFormatted,10}  {f.Path}");
                }

                if (audit.TotalSize > 1_073_741_824)
                    AppendOutput("\nWARNING: Project is over 1GB! Consider removing large files.");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private Task PlaceGitignoreAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                AppendOutput("ERROR: Staging path is invalid.");
                return Task.CompletedTask;
            }

            IsBusy = true;
            try
            {
                var template = GitignoreTemplates.GetTemplate(Context.GitignoreTemplate);
                var gitignorePath = Path.Combine(path, ".gitignore");
                File.WriteAllText(gitignorePath, template);
                AppendOutput($".gitignore ({Context.GitignoreTemplate}) placed at: {gitignorePath}");

                if (Context.IncludeWebGitignore)
                {
                    // Append Angular template
                    var webTemplate = GitignoreTemplates.Angular;
                    File.AppendAllText(gitignorePath, "\n\n" + webTemplate);
                    AppendOutput("Angular/Web rules appended.");
                }

                Steps[3].Status = "Completed";
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
            return Task.CompletedTask;
        }

        private async Task DryRunCheckAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                AppendOutput("ERROR: Staging path is invalid.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Checking what will be committed...";
            _cts = new CancellationTokenSource();

            try
            {
                // Init repo temporarily if needed
                var gitDir = Path.Combine(path, ".git");
                bool wasInit = Directory.Exists(gitDir);

                if (!wasInit)
                    await Git.InitAsync(path, _cts.Token);

                var countResult = await Git.CountFilesAsync(path, _cts.Token);
                if (countResult.Success)
                {
                    var lines = countResult.Output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    Context.TotalFilesForCommit = lines.Length;
                    AppendOutput($"Files that will be committed: {lines.Length}");

                    // Show first 30
                    AppendOutput("--- First 30 files ---");
                    for (int i = 0; i < Math.Min(30, lines.Length); i++)
                        AppendOutput($"  {lines[i]}");
                    if (lines.Length > 30)
                        AppendOutput($"  ... and {lines.Length - 30} more");
                }

                // Show ignored files
                var ignoredResult = await Git.ListIgnoredFilesAsync(path, _cts.Token);
                if (ignoredResult.Success)
                {
                    var ignored = ignoredResult.Output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    AppendOutput($"\nIgnored files: {ignored.Length}");
                }

                // Clean up if we created .git
                if (!wasInit && Directory.Exists(gitDir))
                {
                    try { Directory.Delete(gitDir, true); } catch { }
                }
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task RunMigrationAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                AppendOutput("ERROR: Staging path is invalid.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Context.RemoteUrl))
            {
                AppendOutput("ERROR: Remote URL is empty.");
                return;
            }

            var message = string.IsNullOrWhiteSpace(Context.CommitMessage)
                ? $"Initial import from ClearCase\n\n- Visual Studio version: {Context.VsVersion}\n- Migration date: {DateTime.Now:yyyy-MM-dd}\n- Source: ClearCase snapshot (latest)"
                : Context.CommitMessage;

            IsBusy = true;
            BusyMessage = "Running migration...";
            _cts = new CancellationTokenSource();

            try
            {
                var success = await Git.RunMigrationAsync(path, message, Context.RemoteUrl, Context.DefaultBranch, _cts.Token);
                Steps[5].Status = success ? "Completed" : "Error";
            }
            catch (Exception ex)
            {
                AppendOutput($"ERROR: {ex.Message}");
                Steps[5].Status = "Error";
            }
            finally { IsBusy = false; }
        }

        private async Task RunVerifyAsync()
        {
            if (string.IsNullOrWhiteSpace(Context.RemoteUrl))
            {
                AppendOutput("ERROR: Remote URL is empty.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Context.VerifyPath))
            {
                AppendOutput("ERROR: Verify folder is empty.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Running verification clone...";
            _cts = new CancellationTokenSource();

            try
            {
                var targetDir = Path.Combine(Context.VerifyPath, Context.ProjectName ?? "verify-clone");

                if (Directory.Exists(targetDir))
                {
                    AppendOutput($"Warning: {targetDir} already exists. Deleting...");
                    Directory.Delete(targetDir, true);
                }

                var result = await Git.CloneAsync(Context.RemoteUrl, targetDir, _cts.Token);
                if (result.Success)
                {
                    AppendOutput("Clone successful!");
                    await Git.LogAsync(targetDir, 5, _cts.Token);

                    var countResult = await Runner.RunGitAsync("ls-files", targetDir, _cts.Token);
                    if (countResult.Success)
                    {
                        var files = countResult.Output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        AppendOutput($"Files in repo: {files.Length}");
                    }

                    Steps[6].Status = "Completed";
                    AppendOutput("\nVerification complete. Open the .sln in Visual Studio and run Build.");
                }
                else
                {
                    AppendOutput("Clone FAILED.");
                    Steps[6].Status = "Error";
                }
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        // === Helpers ===

        public void AppendOutput(string text)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == false)
            {
                Application.Current.Dispatcher.Invoke(() => AppendOutput(text));
                return;
            }
            ConsoleOutput += text + "\n";
        }

        private void BrowseFolder(Action<string> setter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                setter(dialog.SelectedPath);
            }
        }

        private static void CopyDirectory(string source, string dest)
        {
            Directory.CreateDirectory(dest);

            foreach (var file in Directory.GetFiles(source))
            {
                var destFile = Path.Combine(dest, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                var destDir = Path.Combine(dest, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }
    }
}
