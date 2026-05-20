using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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

                    // Auto-refresh branch list when entering Daily Work step
                    if (value == 8 && DailyWorkingDirIsReady)
                        _ = RefreshBranchListAsync();
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

        private double _copyProgress;
        public double CopyProgress { get => _copyProgress; set => SetProperty(ref _copyProgress, value); }

        private bool _isCopying;
        public bool IsCopying { get => _isCopying; set => SetProperty(ref _isCopying, value); }

        public bool AllPreservedSelected
        {
            get => Context.PreservedSubDirs.Count > 0 && Context.PreservedSubDirs.All(d => d.IsPreserved);
            set
            {
                foreach (var item in Context.PreservedSubDirs)
                    item.IsPreserved = value;
                OnPropertyChanged(nameof(AllPreservedSelected));
            }
        }

        private string _gitVersion;
        public string GitVersion { get => _gitVersion; set => SetProperty(ref _gitVersion, value); }

        // === Project Management ===
        private string _currentProjectId;
        public string CurrentProjectId { get => _currentProjectId; set => SetProperty(ref _currentProjectId, value); }

        private string _currentProjectName = "Untitled Project";
        public string CurrentProjectName { get => _currentProjectName; set { SetProperty(ref _currentProjectName, value); OnPropertyChanged(nameof(WindowTitle)); } }

        private bool _isDirty;
        public bool IsDirty { get => _isDirty; set { SetProperty(ref _isDirty, value); OnPropertyChanged(nameof(WindowTitle)); } }

        public string PlatformName => Context?.RemotePlatform ?? "GitLab";

        public string WindowTitle => IsDirty
            ? $"{CurrentProjectName}* — ClearCase -> {PlatformName}"
            : $"{CurrentProjectName} — ClearCase -> {PlatformName}";

        private ObservableCollection<ProjectInfo> _savedProjects = new ObservableCollection<ProjectInfo>();
        public ObservableCollection<ProjectInfo> SavedProjects { get => _savedProjects; set => SetProperty(ref _savedProjects, value); }

        // === Output Console ===
        private readonly StringBuilder _consoleBuffer = new StringBuilder();
        private string _consoleOutput = "";
        public string ConsoleOutput { get => _consoleOutput; set => SetProperty(ref _consoleOutput, value); }

        // === README ===
        private string _readmeContent = "";
        public string ReadmeContent { get => _readmeContent; set => SetProperty(ref _readmeContent, value); }

        // === Tags ===
        private string _tagName = "v1.0.0";
        public string TagName { get => _tagName; set => SetProperty(ref _tagName, value); }

        private string _tagMessage = "";
        public string TagMessage { get => _tagMessage; set => SetProperty(ref _tagMessage, value); }

        // === Daily Workflow ===
        private string _dailyWorkingPath = "";
        public string DailyWorkingPath
        {
            get => _dailyWorkingPath;
            set
            {
                if (SetProperty(ref _dailyWorkingPath, value))
                {
                    OnPropertyChanged(nameof(DailyWorkingDirStatus));
                    OnPropertyChanged(nameof(DailyWorkingDirIsReady));
                }
            }
        }

        public string DailyWorkingDirStatus
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DailyWorkingPath))
                    return $"Not set — choose a working directory or clone from {PlatformName}";
                if (!Directory.Exists(DailyWorkingPath))
                    return "Directory does not exist";
                if (!Directory.Exists(Path.Combine(DailyWorkingPath, ".git")))
                    return "Not a Git repository — use Clone to set it up";
                return "Ready";
            }
        }

        public bool DailyWorkingDirIsReady
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DailyWorkingPath)) return false;
                if (!Directory.Exists(DailyWorkingPath)) return false;
                if (!Directory.Exists(Path.Combine(DailyWorkingPath, ".git"))) return false;
                return true;
            }
        }

        private string _dailyCommitMessage = "";
        public string DailyCommitMessage { get => _dailyCommitMessage; set => SetProperty(ref _dailyCommitMessage, value); }

        private string _newBranchName = "";
        public string NewBranchName { get => _newBranchName; set => SetProperty(ref _newBranchName, value); }

        private string _switchBranchName = "";
        public string SwitchBranchName { get => _switchBranchName; set => SetProperty(ref _switchBranchName, value); }

        private string _deleteBranchName = "";
        public string DeleteBranchName { get => _deleteBranchName; set => SetProperty(ref _deleteBranchName, value); }

        private ObservableCollection<string> _availableBranches = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableBranches { get => _availableBranches; set => SetProperty(ref _availableBranches, value); }

        private string _currentBranchName = "";
        public string CurrentBranchName { get => _currentBranchName; set => SetProperty(ref _currentBranchName, value); }

        private string _stashMessage = "";
        public string StashMessage { get => _stashMessage; set => SetProperty(ref _stashMessage, value); }

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
        // Step 3 - Preserve
        public AsyncRelayCommand ScanPreservableCommand { get; private set; }
        // Step 4
        public AsyncRelayCommand PlaceGitignoreCommand { get; private set; }
        public AsyncRelayCommand PreviewGitignoreCommand { get; private set; }
        // Step 6
        public AsyncRelayCommand RunMigrationCommand { get; private set; }
        public AsyncRelayCommand DryRunCheckCommand { get; private set; }
        // Step 7
        public AsyncRelayCommand RunVerifyCommand { get; private set; }
        public AsyncRelayCommand BrowseVerifyCommand { get; private set; }
        public AsyncRelayCommand VerifyFixGitignoreCommand { get; private set; }
        public AsyncRelayCommand VerifyFixCommitPushCommand { get; private set; }
        public AsyncRelayCommand VerifyScanTrackedIgnoredCommand { get; private set; }
        public AsyncRelayCommand VerifyRemoveCachedCommand { get; private set; }

        private string _verifyFixCommitMessage = "fix: update gitignore and add missing files";
        public string VerifyFixCommitMessage
        {
            get => _verifyFixCommitMessage;
            set => SetProperty(ref _verifyFixCommitMessage, value);
        }

        public ObservableCollection<TrackedIgnoredFile> TrackedIgnoredFiles { get; }
            = new ObservableCollection<TrackedIgnoredFile>();

        public bool AllTrackedIgnoredSelected
        {
            get => TrackedIgnoredFiles.Count > 0 && TrackedIgnoredFiles.All(f => f.IsSelected);
            set
            {
                foreach (var f in TrackedIgnoredFiles) f.IsSelected = value;
                OnPropertyChanged(nameof(AllTrackedIgnoredSelected));
            }
        }
        public RelayCommand GenerateReadmeCommand { get; private set; }
        public RelayCommand SaveReadmeCommand { get; private set; }
        public RelayCommand LoadReadmeFileCommand { get; private set; }

        // Tags
        public AsyncRelayCommand CreateTagCommand { get; private set; }
        public AsyncRelayCommand ListTagsCommand { get; private set; }
        public AsyncRelayCommand PushTagsCommand { get; private set; }

        // Daily Workflow - Setup
        public AsyncRelayCommand BrowseDailyWorkingCommand { get; private set; }
        public AsyncRelayCommand CloneToDailyCommand { get; private set; }
        // Daily Workflow - Operations
        public AsyncRelayCommand DailyPullCommand { get; private set; }
        public AsyncRelayCommand DailyPushCommand { get; private set; }
        public AsyncRelayCommand DailyStatusCommand { get; private set; }
        public AsyncRelayCommand DailyDiffCommand { get; private set; }
        public AsyncRelayCommand DailyLogCommand { get; private set; }
        public AsyncRelayCommand DailyCommitCommand { get; private set; }
        public AsyncRelayCommand DailyCommitPushCommand { get; private set; }
        public AsyncRelayCommand DailyCreateBranchCommand { get; private set; }
        public AsyncRelayCommand DailyListBranchesCommand { get; private set; }
        public AsyncRelayCommand DailySwitchMainCommand { get; private set; }
        public AsyncRelayCommand DailyGraphCommand { get; private set; }
        public AsyncRelayCommand DailySwitchBranchCommand { get; private set; }
        public AsyncRelayCommand DailyDeleteBranchCommand { get; private set; }
        public AsyncRelayCommand DailyFetchCommand { get; private set; }
        public AsyncRelayCommand DailyDiscardCommand { get; private set; }
        public AsyncRelayCommand DailyUnstageCommand { get; private set; }
        public AsyncRelayCommand DailyUndoCommitCommand { get; private set; }
        public AsyncRelayCommand DailyAmendCommitCommand { get; private set; }
        public AsyncRelayCommand DailyStashCommand { get; private set; }
        public AsyncRelayCommand DailyStashPopCommand { get; private set; }
        public AsyncRelayCommand DailyStashListCommand { get; private set; }
        public AsyncRelayCommand RefreshBranchListCommand { get; private set; }

        // Help
        public RelayCommand ShowHelpCommand { get; private set; }

        // Project management
        public RelayCommand NewProjectCommand { get; private set; }
        public RelayCommand SaveProjectCommand { get; private set; }
        public RelayCommand LoadProjectCommand { get; private set; }
        public RelayCommand DeleteProjectCommand { get; private set; }

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
            RefreshProjectList();
            _ = CheckGitAsync();

            // Dirty tracking
            Context.PropertyChanged += (s, e) =>
            {
                if (!_suppressDirty) IsDirty = true;
                if (e.PropertyName == nameof(MigrationContext.RemotePlatform))
                {
                    OnPropertyChanged(nameof(PlatformName));
                    OnPropertyChanged(nameof(WindowTitle));
                }
            };
        }

        private bool _suppressDirty;

        private void InitializeSteps()
        {
            Steps.Add(new StepInfo { Number = 1, Title = "Git Config", Icon = "+" });
            Steps.Add(new StepInfo { Number = 2, Title = "Prepare", Icon = ">" });
            Steps.Add(new StepInfo { Number = 3, Title = "Clean", Icon = "X" });
            Steps.Add(new StepInfo { Number = 4, Title = "Gitignore", Icon = "." });
            Steps.Add(new StepInfo { Number = 5, Title = "Remote", Icon = "G" });
            Steps.Add(new StepInfo { Number = 6, Title = "Migrate", Icon = "!" });
            Steps.Add(new StepInfo { Number = 7, Title = "Verify", Icon = "?" });
            Steps.Add(new StepInfo { Number = 8, Title = "Checklist", Icon = "#" });
            Steps.Add(new StepInfo { Number = 9, Title = "Daily Work", Icon = ">" });
        }

        private void InitializeCommands()
        {
            NextStepCommand = new RelayCommand(() =>
            {
                if (CanGoForward)
                {
                    AutoCompleteCurrentStep();
                    CurrentStepIndex++;
                }
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

            ClearConsoleCommand = new RelayCommand(() => { _consoleBuffer.Clear(); ConsoleOutput = ""; });

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
            ScanPreservableCommand = new AsyncRelayCommand(ScanPreservableAsync, () => !IsBusy);

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
            VerifyFixGitignoreCommand = new AsyncRelayCommand(VerifyFixGitignoreAsync, () => !IsBusy);
            VerifyFixCommitPushCommand = new AsyncRelayCommand(VerifyFixCommitPushAsync, () => !IsBusy);
            VerifyScanTrackedIgnoredCommand = new AsyncRelayCommand(VerifyScanTrackedIgnoredAsync, () => !IsBusy);
            VerifyRemoveCachedCommand = new AsyncRelayCommand(VerifyRemoveCachedAsync, () => !IsBusy);
            GenerateReadmeCommand = new RelayCommand(GenerateReadme);
            SaveReadmeCommand = new RelayCommand(SaveReadme);
            LoadReadmeFileCommand = new RelayCommand(LoadReadmeFile);

            // Tags
            CreateTagCommand = new AsyncRelayCommand(CreateTagAsync, () => !IsBusy);
            ListTagsCommand = new AsyncRelayCommand(ListTagsAsync, () => !IsBusy);
            PushTagsCommand = new AsyncRelayCommand(PushTagsAsync, () => !IsBusy);

            // Daily Workflow - Setup
            BrowseDailyWorkingCommand = new AsyncRelayCommand(() => { BrowseFolder(p => DailyWorkingPath = p); return Task.CompletedTask; });
            CloneToDailyCommand = new AsyncRelayCommand(CloneToDailyAsync, () => !IsBusy);
            // Daily Workflow - Operations
            DailyPullCommand = new AsyncRelayCommand(DailyPullAsync, () => !IsBusy);
            DailyPushCommand = new AsyncRelayCommand(DailyPushAsync, () => !IsBusy);
            DailyStatusCommand = new AsyncRelayCommand(DailyStatusAsync, () => !IsBusy);
            DailyDiffCommand = new AsyncRelayCommand(DailyDiffAsync, () => !IsBusy);
            DailyLogCommand = new AsyncRelayCommand(DailyLogAsync, () => !IsBusy);
            DailyCommitCommand = new AsyncRelayCommand(DailyCommitAsync, () => !IsBusy);
            DailyCommitPushCommand = new AsyncRelayCommand(DailyCommitPushAsync, () => !IsBusy);
            DailyCreateBranchCommand = new AsyncRelayCommand(DailyCreateBranchAsync, () => !IsBusy);
            DailyListBranchesCommand = new AsyncRelayCommand(DailyListBranchesAsync, () => !IsBusy);
            DailySwitchMainCommand = new AsyncRelayCommand(DailySwitchMainAsync, () => !IsBusy);
            DailyGraphCommand = new AsyncRelayCommand(DailyGraphAsync, () => !IsBusy);
            DailySwitchBranchCommand = new AsyncRelayCommand(DailySwitchBranchAsync, () => !IsBusy);
            DailyDeleteBranchCommand = new AsyncRelayCommand(DailyDeleteBranchAsync, () => !IsBusy);
            DailyFetchCommand = new AsyncRelayCommand(DailyFetchAsync, () => !IsBusy);
            DailyDiscardCommand = new AsyncRelayCommand(DailyDiscardAsync, () => !IsBusy);
            DailyUnstageCommand = new AsyncRelayCommand(DailyUnstageAsync, () => !IsBusy);
            DailyUndoCommitCommand = new AsyncRelayCommand(DailyUndoCommitAsync, () => !IsBusy);
            DailyAmendCommitCommand = new AsyncRelayCommand(DailyAmendCommitAsync, () => !IsBusy);
            DailyStashCommand = new AsyncRelayCommand(DailyStashAsync, () => !IsBusy);
            DailyStashPopCommand = new AsyncRelayCommand(DailyStashPopAsync, () => !IsBusy);
            DailyStashListCommand = new AsyncRelayCommand(DailyStashListAsync, () => !IsBusy);
            RefreshBranchListCommand = new AsyncRelayCommand(RefreshBranchListAsync, () => !IsBusy);

            // Help
            ShowHelpCommand = new RelayCommand(() =>
            {
                // Map current step to help chapter: 0=intro, 1-9=steps, 10=branches, 11=tags, 12=glossary
                var chapterIndex = _currentStepIndex + 1; // step 0 → chapter 1 (step1)
                Views.StyledDialog.ShowHelpViewer(chapterIndex);
            });

            // Project management
            NewProjectCommand = new RelayCommand(NewProject);
            SaveProjectCommand = new RelayCommand(SaveProject);
            LoadProjectCommand = new RelayCommand(p =>
            {
                if (p is string projectId)
                    LoadProject(projectId);
            });
            DeleteProjectCommand = new RelayCommand(p =>
            {
                if (p is string projectId)
                    DeleteProject(projectId);
            });
        }

        /// <summary>
        /// Mark manual steps as completed when navigating away, if their required data is filled.
        /// </summary>
        private void AutoCompleteCurrentStep()
        {
            var step = CurrentStep;
            if (step == null || step.Status == "Completed") return;

            switch (_currentStepIndex)
            {
                case 0: // Git Config — complete if name+email filled
                    if (!string.IsNullOrWhiteSpace(Context.UserName) && !string.IsNullOrWhiteSpace(Context.UserEmail))
                        step.Status = "Completed";
                    break;
                case 4: // GitLab — complete if remote URL filled
                    if (!string.IsNullOrWhiteSpace(Context.RemoteUrl))
                    {
                        step.Status = "Completed";
                        AutoCheckItem("pre", "Remote credentials configured");
                        AutoCheckItem("pre", "Empty remote project created");
                    }
                    break;
                case 7: // Checklist — complete if all items checked
                    if (Context.ChecklistItems != null && Context.ChecklistItems.Count > 0 &&
                        Context.ChecklistItems.All(c => c.IsDone))
                        step.Status = "Completed";
                    break;
            }
        }

        // === Checklist Auto-Check ===

        /// <summary>
        /// Automatically marks a checklist item as done by matching phase and text.
        /// </summary>
        private void AutoCheckItem(string phase, string text)
        {
            if (Context.ChecklistItems == null) return;
            var item = Context.ChecklistItems.FirstOrDefault(c =>
                c.Phase == phase && c.Text == text && !c.IsDone);
            if (item != null)
            {
                item.IsDone = true;
                if (!_suppressDirty) IsDirty = true;
            }
        }

        /// <summary>
        /// Re-evaluates all detectable checklist items based on current project state.
        /// Called after loading a project and after startup.
        /// </summary>
        private void RefreshChecklistFromState()
        {
            // Pre-migration checks
            if (GitVersion != null && !GitVersion.Contains("not found"))
                AutoCheckItem("pre", "Git for Windows installed");

            if (Steps[0].Status == "Completed")
                AutoCheckItem("pre", "Global git config applied");

            if (!string.IsNullOrWhiteSpace(Context.VsVersion))
                AutoCheckItem("pre", "VS version identified");

            if (!string.IsNullOrWhiteSpace(Context.RemoteUrl))
            {
                AutoCheckItem("pre", "Remote credentials configured");
                AutoCheckItem("pre", "Empty remote project created");
            }

            // During-migration checks
            if (Steps[1].Status == "Completed")
                AutoCheckItem("during", "Code copied to staging");

            if (Context.ClearCaseFilesRemoved > 0 || Steps[2].Status == "Completed")
                AutoCheckItem("during", "ClearCase artifacts cleaned");

            if (Context.BuildArtifactsRemoved > 0 || Steps[2].Status == "Completed")
                AutoCheckItem("during", "Build artifacts cleaned");

            if (!string.IsNullOrWhiteSpace(Context.ProjectSizeAfter))
            {
                // Parse size — if it contains "MB" or "KB" it's under 1GB
                var sizeText = Context.ProjectSizeAfter.ToUpperInvariant();
                if (sizeText.Contains("KB") || sizeText.Contains("MB") ||
                    (sizeText.Contains("GB") && double.TryParse(
                        sizeText.Replace("GB", "").Trim(), out double gb) && gb < 1.0))
                    AutoCheckItem("during", "Project size under 1GB");
            }

            if (Steps[3].Status == "Completed")
                AutoCheckItem("during", ".gitignore placed at root");

            if (Steps[5].Status == "Completed")
            {
                AutoCheckItem("during", "First commit done");
                AutoCheckItem("during", "Push to remote succeeded");
            }

            // Post-migration checks
            if (Steps[6].Status == "Completed")
                AutoCheckItem("post", "Fresh clone done");
        }

        // === Command Implementations ===

        private async Task CheckGitAsync()
        {
            GitVersion = await Git.GetVersionAsync() ?? "Git not found!";
            _suppressDirty = true;
            RefreshChecklistFromState();
            _suppressDirty = false;
            IsDirty = false;
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
                AutoCheckItem("pre", "Global git config applied");
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
            IsCopying = true;
            CopyProgress = 0;
            BusyMessage = "Counting files...";
            _cts = new CancellationTokenSource();

            try
            {
                AppendOutput($"Copying from: {Context.SourcePath}");
                AppendOutput($"         to: {Context.StagingPath}");

                var sizeBefore = FileCleanupService.GetDirectorySize(Context.SourcePath);
                Context.ProjectSizeBefore = FileCleanupService.FormatSize(sizeBefore);

                var totalFiles = await Task.Run(() => Directory.GetFiles(Context.SourcePath, "*", SearchOption.AllDirectories).Length);
                BusyMessage = $"Copying {totalFiles} files...";

                var progress = new Progress<int>(copied =>
                {
                    CopyProgress = totalFiles > 0 ? (double)copied / totalFiles * 100 : 0;
                    BusyMessage = $"Copying... {copied}/{totalFiles} files";
                });

                await Task.Run(() => CopyDirectoryWithProgress(Context.SourcePath, Context.StagingPath, progress), _cts.Token);

                // Auto-detect .sln
                var slnPath = SlnParser.FindSln(Context.StagingPath);
                if (slnPath != null)
                {
                    Context.SlnFilePath = slnPath;
                    AppendOutput($"Parsing solution: {slnPath}");
                    var info = SlnParser.Parse(slnPath);
                    if (info != null)
                    {
                        Context.VsVersion = info.DetectedVS;
                        Context.GitignoreTemplate = info.RecommendedGitignore;
                        AppendOutput($"VisualStudioVersion header: {info.VisualStudioVersion ?? "n/a"}");
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
                AutoCheckItem("during", "Code copied to staging");
                if (!string.IsNullOrWhiteSpace(Context.VsVersion))
                    AutoCheckItem("pre", "VS version identified");
                CopyProgress = 100;
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
                IsCopying = false;
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
                AutoCheckItem("during", "ClearCase artifacts cleaned");
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

        private async Task ScanPreservableAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                AppendOutput("ERROR: Staging path is invalid. Complete Step 2 first.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Scanning for preservable subdirectories...";

            try
            {
                var items = await Task.Run(() => Cleanup.ScanPreservableDirs(path));

                Context.PreservedSubDirs.Clear();
                foreach (var item in items)
                    Context.PreservedSubDirs.Add(item);

                if (items.Count == 0)
                {
                    AppendOutput("No data subdirectories found inside build output folders.");
                }
                else
                {
                    AppendOutput($"Found {items.Count} subdirectories inside build folders:");
                    foreach (var item in items)
                        AppendOutput($"  [{item.ParentBuildDir}] {item.RelativePath}");
                    AppendOutput("Check the ones you want to KEEP. They will be preserved during cleanup and added as .gitignore exceptions.");
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
                // Collect preserved relative paths
                var preservePaths = new System.Collections.Generic.List<string>();
                foreach (var item in Context.PreservedSubDirs)
                {
                    if (item.IsPreserved)
                        preservePaths.Add(item.RelativePath);
                }
                if (preservePaths.Count > 0)
                    AppendOutput($"Preserving {preservePaths.Count} subdirectories...");

                var result = await Cleanup.CleanBuildArtifactsAsync(path, preservePaths, _cts.Token);
                Context.BuildArtifactsRemoved = result.FilesRemoved + result.DirectoriesRemoved;

                var sizeAfter = FileCleanupService.GetDirectorySize(path);
                Context.ProjectSizeAfter = FileCleanupService.FormatSize(sizeAfter);

                AppendOutput($"Freed: {FileCleanupService.FormatSize(result.BytesFreed)}");
                AppendOutput($"Project size: {Context.ProjectSizeAfter}");

                AutoCheckItem("during", "Build artifacts cleaned");
                if (sizeAfter < 1_073_741_824) // < 1GB
                    AutoCheckItem("during", "Project size under 1GB");

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

                // Append preserve exceptions for data subdirectories
                var preservedItems = new System.Collections.Generic.List<Models.PreservedSubDir>();
                foreach (var item in Context.PreservedSubDirs)
                {
                    if (item.IsPreserved)
                        preservedItems.Add(item);
                }

                if (preservedItems.Count > 0)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine();
                    sb.AppendLine("# Preserved data directories inside build output");
                    foreach (var item in preservedItems)
                    {
                        // Convert backslashes to forward slashes for gitignore
                        var gitPath = item.RelativePath.Replace('\\', '/');
                        sb.AppendLine($"!{gitPath}/");
                        sb.AppendLine($"!{gitPath}/**");
                    }
                    File.AppendAllText(gitignorePath, sb.ToString());
                    AppendOutput($"Added {preservedItems.Count} preserve exceptions to .gitignore.");
                }

                AutoCheckItem("during", ".gitignore placed at root");
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
                var gitDir = Path.Combine(path, ".git");
                bool hadGit = Directory.Exists(gitDir);

                // Always start fresh for an accurate dry run
                if (hadGit)
                {
                    AppendOutput("Removing existing .git for a clean dry run...");
                    ForceDeleteDirectory(gitDir);
                }

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
                    if (ignored.Length > 0)
                    {
                        AppendOutput("--- First 30 ignored ---");
                        for (int i = 0; i < Math.Min(30, ignored.Length); i++)
                            AppendOutput($"  {ignored[i]}");
                        if (ignored.Length > 30)
                            AppendOutput($"  ... and {ignored.Length - 30} more");
                    }
                }

                // Clean up — dry run only, don't leave .git behind
                if (Directory.Exists(gitDir))
                {
                    try { ForceDeleteDirectory(gitDir); } catch { }
                    AppendOutput("Dry run complete — .git removed.");
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
                if (success)
                {
                    AutoCheckItem("during", "First commit done");
                    AutoCheckItem("during", "Push to remote succeeded");
                }
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

                    AutoCheckItem("post", "Fresh clone done");
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

        // === Verify Fix (fix staging and re-push without needing Daily Workflow) ===

        private Task VerifyFixGitignoreAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.Combine(path, ".git")))
            {
                AppendOutput("ERROR: Staging folder is not a git repository. Run migration first.");
                return Task.CompletedTask;
            }
            if (string.IsNullOrWhiteSpace(Context.GitignoreTemplate))
            {
                AppendOutput("ERROR: No .gitignore template selected. Go to Step 4.");
                return Task.CompletedTask;
            }
            var template = GitignoreTemplates.GetTemplate(Context.GitignoreTemplate);
            var dest = Path.Combine(path, ".gitignore");
            File.WriteAllText(dest, template);
            AppendOutput($".gitignore updated at: {dest}");
            AppendOutput("Review it, add any exceptions needed, then click 'Stage All + Commit + Push'.");
            return Task.CompletedTask;
        }

        private async Task VerifyFixCommitPushAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.Combine(path, ".git")))
            {
                AppendOutput("ERROR: Staging folder is not a git repository. Run migration first.");
                return;
            }
            if (string.IsNullOrWhiteSpace(VerifyFixCommitMessage))
            {
                AppendOutput("ERROR: Commit message is empty.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Staging fix and pushing...";
            _cts = new CancellationTokenSource();
            try
            {
                AppendOutput($"Working in: {path}");

                var statusResult = await Git.StatusAsync(path, _cts.Token);
                if (statusResult.Success && string.IsNullOrWhiteSpace(statusResult.Output?.Trim().Replace("nothing to commit", "").Trim()))
                {
                    AppendOutput("Nothing to commit — working tree is clean.");
                    return;
                }

                var addResult = await Git.AddAllAsync(path, _cts.Token);
                if (!addResult.Success) { AppendOutput($"Stage failed: {addResult.Error}"); return; }

                var commitResult = await Git.CommitAsync(path, VerifyFixCommitMessage, _cts.Token);
                if (!commitResult.Success) { AppendOutput($"Commit failed: {commitResult.Error}"); return; }

                var branch = Context.DefaultBranch ?? "main";
                var pushResult = await Git.PushAsync(path, "origin", branch, true, _cts.Token);
                if (pushResult.Success)
                {
                    AppendOutput("Fix pushed successfully. Run 'Clone and Verify' again to re-test.");
                    VerifyFixCommitMessage = "fix: update gitignore and add missing files";
                }
                else
                    AppendOutput($"Push failed: {pushResult.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task VerifyScanTrackedIgnoredAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.Combine(path, ".git")))
            {
                AppendOutput("ERROR: Staging folder is not a git repository. Run migration first.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Scanning for accidentally tracked files...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.GetTrackedIgnoredFilesAsync(path, _cts.Token);
                TrackedIgnoredFiles.Clear();

                if (!result.Success)
                {
                    AppendOutput($"Scan failed: {result.Error}");
                    return;
                }

                var files = (result.Output ?? "")
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(f => f.Trim())
                    .Where(f => !string.IsNullOrEmpty(f))
                    .OrderBy(f => f)
                    .ToList();

                if (files.Count == 0)
                {
                    AppendOutput("No accidentally tracked files found. Repo is clean.");
                    return;
                }

                foreach (var f in files)
                    TrackedIgnoredFiles.Add(new TrackedIgnoredFile { FilePath = f, IsSelected = true });

                OnPropertyChanged(nameof(AllTrackedIgnoredSelected));
                AppendOutput($"Found {files.Count} tracked file(s) covered by .gitignore. Review and click 'Remove Selected from Git'.");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task VerifyRemoveCachedAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.Combine(path, ".git")))
            {
                AppendOutput("ERROR: Staging folder is not a git repository.");
                return;
            }

            var selected = TrackedIgnoredFiles.Where(f => f.IsSelected).Select(f => f.FilePath).ToList();
            if (selected.Count == 0)
            {
                AppendOutput("No files selected.");
                return;
            }

            IsBusy = true;
            BusyMessage = $"Removing {selected.Count} file(s) from git tracking...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.RemoveCachedAsync(path, selected, _cts.Token);
                if (result.Success)
                {
                    AppendOutput($"Removed {selected.Count} file(s) from git tracking (files kept locally).");
                    AppendOutput("Now click 'Stage All + Commit + Push' to apply the change to the remote.");
                    foreach (var f in TrackedIgnoredFiles.Where(f => f.IsSelected).ToList())
                        TrackedIgnoredFiles.Remove(f);
                    OnPropertyChanged(nameof(AllTrackedIgnoredSelected));
                    VerifyFixCommitMessage = $"fix: remove {selected.Count} accidentally tracked file(s) from git";
                }
                else
                    AppendOutput($"Remove failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        // === Tags ===

        private async Task CreateTagAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.Combine(path, ".git")))
            {
                AppendOutput("ERROR: No git repository found. Run migration first.");
                return;
            }
            if (string.IsNullOrWhiteSpace(TagName))
            {
                AppendOutput("ERROR: Tag name is empty.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Creating tag...";
            _cts = new CancellationTokenSource();

            try
            {
                var message = string.IsNullOrWhiteSpace(TagMessage)
                    ? $"Release {TagName} — migrated from ClearCase"
                    : TagMessage;

                var result = await Git.CreateTagAsync(path, TagName, message, _cts.Token);
                if (result.Success)
                    AppendOutput($"Tag '{TagName}' created successfully.");
                else
                    AppendOutput($"Failed to create tag: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task ListTagsAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.Combine(path, ".git")))
            {
                AppendOutput("ERROR: No git repository found.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Loading tags...";
            _cts = new CancellationTokenSource();

            try
            {
                var result = await Git.ListTagsWithMessagesAsync(path, _cts.Token);
                if (result.Success)
                {
                    IsBusy = false;
                    Views.StyledDialog.ShowTagsViewer(result.Output ?? "");
                    return;
                }
                else
                {
                    AppendOutput($"Failed to list tags: {result.Error}");
                }
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task PushTagsAsync()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Path.Combine(path, ".git")))
            {
                AppendOutput("ERROR: No git repository found.");
                return;
            }

            IsBusy = true;
            BusyMessage = $"Pushing tags to {PlatformName}...";
            _cts = new CancellationTokenSource();

            try
            {
                var result = await Git.PushAllTagsAsync(path, "origin", _cts.Token);
                if (result.Success)
                    AppendOutput($"All tags pushed to {PlatformName}.");
                else
                    AppendOutput($"Push failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        // === README Generation ===

        private string BuildReadmeContent()
        {
            var sb = new System.Text.StringBuilder();
            var projectName = string.IsNullOrWhiteSpace(Context.ProjectName) ? "Project" : Context.ProjectName;
            var date = DateTime.Now.ToString("yyyy-MM-dd");

            sb.AppendLine($"# {projectName}");
            sb.AppendLine();

            // Description
            sb.AppendLine("## Description");
            sb.AppendLine();
            sb.AppendLine($"This project was migrated from **IBM ClearCase** to **{PlatformName}** on {date}.");
            sb.AppendLine();

            // Quick Start
            sb.AppendLine("## Quick Start");
            sb.AppendLine();
            sb.AppendLine("```bash");
            if (!string.IsNullOrWhiteSpace(Context.RemoteUrl))
                sb.AppendLine($"git clone {Context.RemoteUrl}");
            else
                sb.AppendLine("git clone <project-url>");
            sb.AppendLine($"cd {projectName}");
            sb.AppendLine("```");
            sb.AppendLine();

            // Build Instructions
            sb.AppendLine("## Build Instructions");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(Context.VsVersion))
            {
                sb.AppendLine($"- **IDE:** Visual Studio {Context.VsVersion}");
            }
            sb.AppendLine($"- **Framework:** .NET Framework (see solution file for details)");
            if (!string.IsNullOrWhiteSpace(Context.SlnFilePath))
            {
                var slnName = Path.GetFileName(Context.SlnFilePath);
                sb.AppendLine($"- **Solution File:** `{slnName}`");
            }
            sb.AppendLine();
            sb.AppendLine("### Steps");
            sb.AppendLine();
            sb.AppendLine("1. Open the `.sln` file in Visual Studio");
            sb.AppendLine("2. Allow NuGet package restore if prompted");
            sb.AppendLine("3. **Build** > **Rebuild Solution**");
            sb.AppendLine("4. Verify: 0 errors");
            sb.AppendLine("5. Press **F5** to run");
            sb.AppendLine();

            // Project Structure
            sb.AppendLine("## Project Structure");
            sb.AppendLine();
            sb.AppendLine("```");
            sb.AppendLine($"{projectName}/");
            sb.AppendLine("  |-- .gitignore");
            if (!string.IsNullOrWhiteSpace(Context.SlnFilePath))
                sb.AppendLine($"  |-- {Path.GetFileName(Context.SlnFilePath)}");
            sb.AppendLine("  |-- README.md");
            sb.AppendLine("  |-- ...");
            sb.AppendLine("```");
            sb.AppendLine();

            // Git Workflow
            sb.AppendLine("## Git Workflow");
            sb.AppendLine();
            sb.AppendLine("### Daily workflow");
            sb.AppendLine();
            sb.AppendLine("```bash");
            sb.AppendLine("# Get latest changes");
            sb.AppendLine("git pull");
            sb.AppendLine();
            sb.AppendLine("# Make changes, then stage and commit");
            sb.AppendLine("git add .");
            sb.AppendLine("git commit -m \"Brief description of changes\"");
            sb.AppendLine();
            sb.AppendLine($"# Push to {PlatformName}");
            sb.AppendLine("git push");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("### Working with branches");
            sb.AppendLine();
            sb.AppendLine("```bash");
            sb.AppendLine("# Create a feature branch");
            sb.AppendLine("git checkout -b feature/my-feature");
            sb.AppendLine();
            sb.AppendLine("# ... make changes, commit ...");
            sb.AppendLine();
            sb.AppendLine("# Push branch and create merge request");
            sb.AppendLine("git push -u origin feature/my-feature");
            sb.AppendLine("```");
            sb.AppendLine();

            // Migration Info
            sb.AppendLine("## Migration Info");
            sb.AppendLine();
            sb.AppendLine("| Detail | Value |");
            sb.AppendLine("|--------|-------|");
            sb.AppendLine($"| Migration Date | {date} |");
            sb.AppendLine($"| Source | IBM ClearCase |");
            sb.AppendLine($"| Target | {PlatformName} |");
            if (!string.IsNullOrWhiteSpace(Context.VsVersion))
                sb.AppendLine($"| Visual Studio | {Context.VsVersion} |");
            sb.AppendLine($"| Gitignore Template | {Context.GitignoreTemplate} |");
            if (!string.IsNullOrWhiteSpace(Context.DefaultBranch))
                sb.AppendLine($"| Default Branch | {Context.DefaultBranch} |");
            sb.AppendLine();

            // Important Notes
            sb.AppendLine("## Important Notes");
            sb.AppendLine();
            sb.AppendLine("- **Do NOT commit** `bin/`, `obj/`, or build outputs — they are in `.gitignore`");
            sb.AppendLine("- **Do NOT commit** personal settings files (`*.user`, `*.suo`)");
            sb.AppendLine("- If build fails after clone, check that all NuGet packages restore correctly");
            sb.AppendLine("- For any external DLL dependencies, see the `libs/` folder (if present)");
            sb.AppendLine();

            return sb.ToString();
        }

        private void GenerateReadme()
        {
            ReadmeContent = BuildReadmeContent();
            AppendOutput("README content generated. Edit as needed, then click Save.");
        }

        private void LoadReadmeFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Markdown|*.md|Text files|*.txt|All files|*.*",
                Title = "Load README file"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ReadmeContent = File.ReadAllText(dialog.FileName, System.Text.Encoding.UTF8);
                    AppendOutput($"Loaded: {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    AppendOutput($"ERROR: {ex.Message}");
                }
            }
        }

        private void SaveReadme()
        {
            var path = Context.StagingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                AppendOutput("ERROR: Staging path is invalid.");
                return;
            }
            if (string.IsNullOrWhiteSpace(ReadmeContent))
            {
                AppendOutput("ERROR: README content is empty. Generate or load first.");
                return;
            }

            try
            {
                var readmePath = Path.Combine(path, "README.md");
                File.WriteAllText(readmePath, ReadmeContent, System.Text.Encoding.UTF8);
                AppendOutput($"README.md saved at: {readmePath}");
            }
            catch (Exception ex)
            {
                AppendOutput($"ERROR: {ex.Message}");
            }
        }

        // === Daily Workflow ===

        private string GetDailyWorkingDir()
        {
            var path = DailyWorkingPath;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return null;
            if (!Directory.Exists(Path.Combine(path, ".git")))
                return null;
            return path;
        }

        private async Task CloneToDailyAsync()
        {
            if (string.IsNullOrWhiteSpace(Context.RemoteUrl))
            {
                AppendOutput($"ERROR: Remote URL is empty. Complete Step 5 ({PlatformName}) first.");
                return;
            }
            if (string.IsNullOrWhiteSpace(DailyWorkingPath))
            {
                AppendOutput("ERROR: Working directory is not set. Browse to a folder first.");
                return;
            }

            // If the directory already has .git, don't clone over it
            if (Directory.Exists(Path.Combine(DailyWorkingPath, ".git")))
            {
                AppendOutput("This folder is already a Git repository. Ready to use.");
                OnPropertyChanged(nameof(DailyWorkingDirStatus));
                OnPropertyChanged(nameof(DailyWorkingDirIsReady));
                return;
            }

            IsBusy = true;
            BusyMessage = $"Cloning from {PlatformName}...";
            _cts = new CancellationTokenSource();

            try
            {
                // Clone into the selected path
                // If directory exists but is empty, clone directly. Otherwise clone into a subfolder.
                string targetDir;
                if (Directory.Exists(DailyWorkingPath) && Directory.GetFileSystemEntries(DailyWorkingPath).Length > 0)
                {
                    // Not empty — clone into a subfolder with the project name
                    var folderName = Context.ProjectName ?? Path.GetFileNameWithoutExtension(Context.RemoteUrl.TrimEnd('/'));
                    targetDir = Path.Combine(DailyWorkingPath, folderName);
                }
                else
                {
                    targetDir = DailyWorkingPath;
                }

                var result = await Git.CloneAsync(Context.RemoteUrl, targetDir, _cts.Token);
                if (result.Success)
                {
                    _dailyWorkingPath = targetDir;
                    OnPropertyChanged(nameof(DailyWorkingPath));
                    OnPropertyChanged(nameof(DailyWorkingDirStatus));
                    OnPropertyChanged(nameof(DailyWorkingDirIsReady));
                    AppendOutput($"Clone successful to: {targetDir}");
                    await Git.LogAsync(targetDir, 5, _cts.Token);
                    await RefreshBranchListAsync();
                }
                else
                {
                    AppendOutput($"Clone failed: {result.Error}");
                }
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyPullAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }

            IsBusy = true;
            BusyMessage = "Checking for local changes...";
            _cts = new CancellationTokenSource();
            try
            {
                // Check for uncommitted changes first
                var statusResult = await Git.StatusAsync(path, _cts.Token);
                bool hasLocalChanges = statusResult.Success &&
                    (statusResult.Output.Contains("Changes not staged") ||
                     statusResult.Output.Contains("Changes to be committed") ||
                     statusResult.Output.Contains("Untracked files"));

                if (hasLocalChanges)
                {
                    IsBusy = false; // Allow dialog interaction
                    var answer = Views.StyledDialog.ShowConfirm(
                        "Local Changes Detected",
                        "You have uncommitted local changes",
                        "Pull may fail if your changes conflict with the remote version.",
                        new[]
                        {
                            "Yes — Stash changes, Pull, then restore (safe)",
                            "No — Pull anyway (may fail if conflicts exist)",
                            "Cancel — Abort"
                        },
                        Views.StyledDialog.DialogIcon.Warning);
                    IsBusy = true;

                    if (answer == MessageBoxResult.Cancel)
                    {
                        AppendOutput("Pull cancelled.");
                        return;
                    }

                    if (answer == MessageBoxResult.Yes)
                    {
                        // Stash → Pull → Stash Pop
                        BusyMessage = "Stashing local changes...";
                        var stashResult = await Git.StashAsync(path, "Auto-stash before pull", _cts.Token);
                        if (!stashResult.Success)
                        {
                            AppendOutput($"Stash failed: {stashResult.Error}");
                            return;
                        }

                        BusyMessage = "Pulling latest changes...";
                        var pullResult = await Git.PullAsync(path, _cts.Token);
                        if (!pullResult.Success)
                        {
                            AppendOutput($"Pull failed: {pullResult.Error}");
                            AppendOutput("Your changes are still in stash. Use 'Stash Pop' to restore them.");
                            return;
                        }

                        BusyMessage = "Restoring local changes...";
                        var popResult = await Git.StashPopAsync(path, _cts.Token);
                        if (popResult.Success)
                        {
                            AppendOutput("Pull complete. Local changes restored successfully.");
                        }
                        else
                        {
                            AppendOutput("Pull succeeded but stash pop had conflicts:");
                            AppendOutput(popResult.Error);
                            AppendOutput("Resolve conflicts manually, then commit.");
                        }
                        return;
                    }
                    // No — just try pull anyway
                }

                BusyMessage = "Pulling latest changes...";
                var result = await Git.PullAsync(path, _cts.Token);
                AppendOutput(result.Success ? "Pull complete." : $"Pull failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyPushAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }

            IsBusy = true;
            BusyMessage = $"Pushing to {PlatformName}...";
            _cts = new CancellationTokenSource();
            try
            {
                var branch = !string.IsNullOrEmpty(CurrentBranchName) ? CurrentBranchName : Context.DefaultBranch;
                var result = await Git.PushAsync(path, "origin", branch, true, _cts.Token);
                AppendOutput(result.Success ? $"Push complete (branch: {branch})." : $"Push failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyStatusAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }

            IsBusy = true;
            BusyMessage = "Getting status...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.StatusAsync(path, _cts.Token);
                if (result.Success)
                {
                    // Extract branch name
                    var branch = Context.DefaultBranch ?? "main";
                    var lines = result.Output.Split('\n');
                    foreach (var l in lines)
                    {
                        if (l.StartsWith("On branch "))
                        {
                            branch = l.Substring("On branch ".Length).Trim();
                            break;
                        }
                    }

                    IsBusy = false;
                    Views.StyledDialog.ShowStatusViewer(result.Output, branch);
                    return;
                }
                else
                {
                    AppendOutput($"Status failed: {result.Error}");
                }
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyDiffAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }

            IsBusy = true;
            BusyMessage = "Getting diff...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.DiffAsync(path, false, _cts.Token);
                if (result.Success)
                {
                    var output = result.Output?.Trim();
                    if (string.IsNullOrEmpty(output))
                    {
                        // Try staged diff
                        var stagedResult = await Git.DiffAsync(path, true, _cts.Token);
                        output = stagedResult.Success ? stagedResult.Output?.Trim() : "";
                    }

                    if (string.IsNullOrEmpty(output))
                    {
                        AppendOutput("No changes detected (working tree is clean).");
                    }
                    else
                    {
                        IsBusy = false;
                        Views.StyledDialog.ShowDiffViewer("Git Diff — Changes", output);
                        return;
                    }
                }
                else
                {
                    AppendOutput($"Diff failed: {result.Error}");
                }
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyLogAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }

            IsBusy = true;
            BusyMessage = "Loading commit history...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.LogDetailedAsync(path, 30, _cts.Token);
                if (result.Success)
                {
                    IsBusy = false;
                    Views.StyledDialog.ShowLogViewer(result.Output ?? "");
                    return;
                }
                else
                {
                    AppendOutput($"Log failed: {result.Error}");
                }
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyCommitAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }
            if (string.IsNullOrWhiteSpace(DailyCommitMessage))
            {
                AppendOutput("ERROR: Commit message is empty.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Staging and committing...";
            _cts = new CancellationTokenSource();
            try
            {
                var addResult = await Git.AddAllAsync(path, _cts.Token);
                if (!addResult.Success) { AppendOutput($"Stage failed: {addResult.Error}"); return; }

                var commitResult = await Git.CommitAsync(path, DailyCommitMessage, _cts.Token);
                if (commitResult.Success)
                {
                    AppendOutput("Commit successful.");
                    DailyCommitMessage = "";
                }
                else
                    AppendOutput($"Commit failed: {commitResult.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyCommitPushAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }
            if (string.IsNullOrWhiteSpace(DailyCommitMessage))
            {
                AppendOutput("ERROR: Commit message is empty.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Staging, committing, and pushing...";
            _cts = new CancellationTokenSource();
            try
            {
                var addResult = await Git.AddAllAsync(path, _cts.Token);
                if (!addResult.Success) { AppendOutput($"Stage failed: {addResult.Error}"); return; }

                var commitResult = await Git.CommitAsync(path, DailyCommitMessage, _cts.Token);
                if (!commitResult.Success) { AppendOutput($"Commit failed: {commitResult.Error}"); return; }

                AppendOutput("Commit successful. Pushing...");
                DailyCommitMessage = "";

                var branch = !string.IsNullOrEmpty(CurrentBranchName) ? CurrentBranchName : Context.DefaultBranch;
                var pushResult = await Git.PushAsync(path, "origin", branch, true, _cts.Token);
                AppendOutput(pushResult.Success ? $"Push complete (branch: {branch})." : $"Push failed: {pushResult.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyCreateBranchAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }
            if (string.IsNullOrWhiteSpace(NewBranchName))
            {
                AppendOutput("ERROR: Branch name is empty.");
                return;
            }

            IsBusy = true;
            BusyMessage = "Creating branch...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.CheckoutAsync(path, NewBranchName, true, _cts.Token);
                if (result.Success)
                {
                    AppendOutput($"Switched to new branch '{NewBranchName}'.");
                    NewBranchName = "";
                    await RefreshBranchListAsync();
                }
                else
                    AppendOutput($"Failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyListBranchesAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }

            IsBusy = true;
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.BranchListAsync(path, _cts.Token);
                if (result.Success)
                    Views.StyledDialog.ShowBranchesViewer(result.Output ?? "");
                else
                    AppendOutput($"Failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task RefreshBranchListAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) return;

            try
            {
                // Get current branch
                var curResult = await Git.GetCurrentBranchAsync(path);
                if (curResult.Success)
                    CurrentBranchName = curResult.Output?.Trim() ?? "";

                var result = await Git.BranchListAsync(path);
                if (!result.Success) return;

                var branches = new List<string>();
                foreach (var line in (result.Output ?? "").Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var name = line.Trim().TrimStart('*').Trim();
                    // Skip HEAD pointer entries like "remotes/origin/HEAD -> origin/main"
                    if (name.Contains("->")) continue;
                    if (!string.IsNullOrWhiteSpace(name) && !branches.Contains(name))
                        branches.Add(name);
                }

                AvailableBranches.Clear();
                foreach (var b in branches)
                    AvailableBranches.Add(b);
            }
            catch { /* silent — just a refresh */ }
        }

        private async Task DailyGraphAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }

            IsBusy = true;
            BusyMessage = "Loading graph...";
            _cts = new CancellationTokenSource();
            try
            {
                var structuredResult = await Git.LogStructuredAsync(path, 50, _cts.Token);
                var graphResult = await Git.LogGraphAsync(path, 50, _cts.Token);
                if (structuredResult.Success)
                {
                    // Capture path for closures
                    var workDir = path;
                    string checkoutBranch = null;
                    string diffA = null, diffB = null;

                    Views.StyledDialog.ShowGraphViewer(
                        structuredResult.Output ?? "",
                        graphResult.Output ?? "",
                        onCheckout: branch => { checkoutBranch = branch; },
                        onDiff: (a, b) => { diffA = a; diffB = b; }
                    );

                    // Handle actions after dialog closes
                    if (checkoutBranch != null)
                    {
                        var result = await Git.CheckoutAsync(workDir, checkoutBranch, false, _cts.Token);
                        if (result.Success)
                        {
                            AppendOutput($"Switched to '{checkoutBranch}'.");
                            await RefreshBranchListAsync();
                        }
                        else
                            AppendOutput($"Switch failed: {result.Error}");
                    }
                    else if (diffA != null && diffB != null)
                    {
                        BusyMessage = "Computing diff...";
                        var statResult = await Git.DiffCommitsAsync(workDir, diffA, diffB, _cts.Token);
                        var fullResult = await Git.DiffCommitsFullAsync(workDir, diffA, diffB, _cts.Token);
                        Views.StyledDialog.ShowDiffViewer(
                            diffA, diffB,
                            statResult.Success ? statResult.Output : "",
                            fullResult.Success ? fullResult.Output : "");
                    }
                }
                else
                    AppendOutput($"Failed: {structuredResult.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailySwitchMainAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }

            IsBusy = true;
            BusyMessage = "Switching to main...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.CheckoutAsync(path, Context.DefaultBranch, false, _cts.Token);
                AppendOutput(result.Success ? $"Switched to '{Context.DefaultBranch}'." : $"Failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyStashAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }

            IsBusy = true;
            BusyMessage = "Stashing changes...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.StashAsync(path, StashMessage, _cts.Token);
                if (result.Success)
                {
                    AppendOutput("Changes stashed.");
                    StashMessage = "";
                }
                else
                    AppendOutput($"Stash failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyStashPopAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput($"ERROR: No git repository found. Set the Working Directory and Clone from {PlatformName}."); return; }

            IsBusy = true;
            BusyMessage = "Restoring stashed changes...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.StashPopAsync(path, _cts.Token);
                AppendOutput(result.Success ? "Stash restored." : $"Stash pop failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        // === Fetch ===

        private async Task DailyFetchAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput("ERROR: No git repository found."); return; }

            IsBusy = true;
            BusyMessage = "Fetching from remote...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.FetchAsync(path, _cts.Token);
                if (result.Success)
                {
                    AppendOutput("Fetch completed. Use Pull to merge remote changes.");
                    await RefreshBranchListAsync();
                }
                else
                    AppendOutput($"Fetch failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        // === Switch Branch ===

        private async Task DailySwitchBranchAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput("ERROR: No git repository found."); return; }
            if (string.IsNullOrWhiteSpace(SwitchBranchName))
            {
                AppendOutput("ERROR: Enter a branch name to switch to.");
                return;
            }

            IsBusy = true;
            BusyMessage = $"Switching to {SwitchBranchName}...";
            _cts = new CancellationTokenSource();
            try
            {
                // Handle remote branch names — strip "remotes/origin/" prefix for checkout
                var branchToSwitch = SwitchBranchName;
                if (branchToSwitch.StartsWith("remotes/"))
                    branchToSwitch = branchToSwitch.Substring(branchToSwitch.IndexOf('/', 8) + 1);

                var result = await Git.CheckoutAsync(path, branchToSwitch, false, _cts.Token);
                if (result.Success)
                {
                    AppendOutput($"Switched to '{branchToSwitch}'.");
                    SwitchBranchName = "";
                    await RefreshBranchListAsync();
                }
                else
                    AppendOutput($"Failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        // === Delete Branch ===

        private async Task DailyDeleteBranchAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput("ERROR: No git repository found."); return; }
            if (string.IsNullOrWhiteSpace(DeleteBranchName))
            {
                AppendOutput("ERROR: Enter a branch name to delete.");
                return;
            }

            var answer = Views.StyledDialog.ShowConfirm(
                "Delete Branch",
                $"Delete branch '{DeleteBranchName}'?",
                "This will delete the local branch. Remote branch will not be affected.",
                new[] { "Yes — Delete", "Cancel" },
                Views.StyledDialog.DialogIcon.Warning,
                MessageBoxButton.YesNo);

            if (answer != MessageBoxResult.Yes) return;

            IsBusy = true;
            BusyMessage = $"Deleting {DeleteBranchName}...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.DeleteBranchAsync(path, DeleteBranchName, _cts.Token);
                if (result.Success)
                {
                    AppendOutput($"Branch '{DeleteBranchName}' deleted.");
                    DeleteBranchName = "";
                    await RefreshBranchListAsync();
                }
                else
                    AppendOutput($"Failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        // === Undo Operations ===

        private async Task DailyDiscardAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput("ERROR: No git repository found."); return; }

            var answer = Views.StyledDialog.ShowConfirm(
                "Discard All Changes",
                "Discard all uncommitted changes?",
                "This will permanently revert all modified files to the last commit.\nThis action cannot be undone!",
                new[] { "Yes — Discard everything", "Cancel" },
                Views.StyledDialog.DialogIcon.Danger,
                MessageBoxButton.YesNo);

            if (answer != MessageBoxResult.Yes) return;

            IsBusy = true;
            BusyMessage = "Discarding changes...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.DiscardAllChangesAsync(path, _cts.Token);
                AppendOutput(result.Success ? "All changes discarded. Working directory is clean." : $"Failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyUnstageAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput("ERROR: No git repository found."); return; }

            IsBusy = true;
            BusyMessage = "Unstaging files...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.UnstageAllAsync(path, _cts.Token);
                AppendOutput(result.Success ? "All files unstaged. Changes are preserved but not staged for commit." : $"Failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyUndoCommitAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput("ERROR: No git repository found."); return; }

            var answer = Views.StyledDialog.ShowConfirm(
                "Undo Last Commit",
                "Undo the last commit?",
                "The commit will be removed but your files will remain as they are.\nThe changes will be staged and ready to commit again.",
                new[] { "Yes — Undo commit", "Cancel" },
                Views.StyledDialog.DialogIcon.Warning,
                MessageBoxButton.YesNo);

            if (answer != MessageBoxResult.Yes) return;

            IsBusy = true;
            BusyMessage = "Undoing last commit...";
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.UndoLastCommitAsync(path, _cts.Token);
                AppendOutput(result.Success ? "Last commit undone. Changes are staged and ready to commit." : $"Failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private async Task DailyAmendCommitAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput("ERROR: No git repository found."); return; }
            if (string.IsNullOrWhiteSpace(DailyCommitMessage))
            {
                AppendOutput("ERROR: Write the corrected commit message first.");
                return;
            }

            var answer = Views.StyledDialog.ShowConfirm(
                "Amend Last Commit",
                "Amend the last commit?",
                "This replaces the last commit message and adds any newly staged changes.\nDo NOT amend commits that have already been pushed!",
                new[] { "Yes — Amend", "Cancel" },
                Views.StyledDialog.DialogIcon.Warning,
                MessageBoxButton.YesNo);

            if (answer != MessageBoxResult.Yes) return;

            IsBusy = true;
            BusyMessage = "Amending commit...";
            _cts = new CancellationTokenSource();
            try
            {
                await Git.AddAllAsync(path, _cts.Token);
                var result = await Git.AmendCommitAsync(path, DailyCommitMessage, _cts.Token);
                if (result.Success)
                {
                    AppendOutput("Last commit amended successfully.");
                    DailyCommitMessage = "";
                }
                else
                    AppendOutput($"Failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        // === Stash List ===

        private async Task DailyStashListAsync()
        {
            var path = GetDailyWorkingDir();
            if (path == null) { AppendOutput("ERROR: No git repository found."); return; }

            IsBusy = true;
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Git.StashListAsync(path, _cts.Token);
                if (result.Success)
                {
                    var output = result.Output?.Trim();
                    if (string.IsNullOrEmpty(output))
                        AppendOutput("Stash is empty — no saved changes.");
                    else
                        Views.StyledDialog.ShowStashViewer(output);
                }
                else
                    AppendOutput($"Failed: {result.Error}");
            }
            catch (Exception ex) { AppendOutput($"ERROR: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        // === Helpers ===

        public void AppendOutput(string text)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == false)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => AppendOutput(text)));
                return;
            }
            _consoleBuffer.AppendLine(text);
            ConsoleOutput = _consoleBuffer.ToString();
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

        /// <summary>
        /// Delete a directory even if it contains read-only files (e.g. .git objects)
        /// </summary>
        private static void ForceDeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;

            // Clear read-only attributes on all files
            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var attrs = File.GetAttributes(file);
                    if ((attrs & FileAttributes.ReadOnly) != 0)
                        File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
                }
                catch { }
            }

            Directory.Delete(path, true);
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

        private static void CopyDirectoryWithProgress(string source, string dest, IProgress<int> progress)
        {
            int copied = 0;
            CopyDirRecursive(source, dest, progress, ref copied);
        }

        private static void CopyDirRecursive(string source, string dest, IProgress<int> progress, ref int copied)
        {
            Directory.CreateDirectory(dest);

            foreach (var file in Directory.GetFiles(source))
            {
                var destFile = Path.Combine(dest, Path.GetFileName(file));
                File.Copy(file, destFile, true);
                copied++;
                if (copied % 50 == 0)
                    progress.Report(copied);
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                var destDir = Path.Combine(dest, Path.GetFileName(dir));
                CopyDirRecursive(dir, destDir, progress, ref copied);
            }

            progress.Report(copied);
        }

        // === Project Management ===

        public void RefreshProjectList()
        {
            SavedProjects.Clear();
            foreach (var p in ProjectService.ListProjects())
                SavedProjects.Add(p);
        }

        private void NewProject()
        {
            if (IsDirty)
            {
                var result = Views.StyledDialog.ShowConfirm(
                    "Unsaved Changes",
                    "Save current project first?",
                    "You have unsaved changes that will be lost if you continue.",
                    new[] { "Yes — Save and continue", "No — Discard changes", "Cancel — Go back" },
                    Views.StyledDialog.DialogIcon.Question);
                if (result == MessageBoxResult.Cancel) return;
                if (result == MessageBoxResult.Yes) SaveProject();
            }

            ResetToDefaults();
            CurrentProjectId = null;
            CurrentProjectName = "Untitled Project";
            _consoleBuffer.Clear(); ConsoleOutput = "";
            foreach (var step in Steps) step.Status = "Pending";
            CurrentStepIndex = 0;
            IsDirty = false;
            AppendOutput("New project created.");
        }

        private void SaveProject()
        {
            if (CurrentProjectId == null)
            {
                // First save — ask for a name
                var name = PromptProjectName();
                if (name == null) return;
                CurrentProjectId = Guid.NewGuid().ToString("N");
                CurrentProjectName = name;
            }

            try
            {
                var data = CreateSaveData();
                ProjectService.Save(data);
                IsDirty = false;
                RefreshProjectList();
                AppendOutput($"Project saved: {CurrentProjectName}");
            }
            catch (Exception ex)
            {
                AppendOutput($"ERROR saving: {ex.Message}");
            }
        }

        private void LoadProject(string projectId)
        {
            if (IsDirty)
            {
                var result = Views.StyledDialog.ShowConfirm(
                    "Unsaved Changes",
                    "Save current project first?",
                    "You have unsaved changes that will be lost if you load another project.",
                    new[] { "Yes — Save and continue", "No — Discard changes", "Cancel — Go back" },
                    Views.StyledDialog.DialogIcon.Question);
                if (result == MessageBoxResult.Cancel) return;
                if (result == MessageBoxResult.Yes) SaveProject();
            }

            try
            {
                var data = ProjectService.Load(projectId);
                RestoreFromSaveData(data);
                AppendOutput($"Project loaded: {CurrentProjectName}");
            }
            catch (Exception ex)
            {
                AppendOutput($"ERROR loading: {ex.Message}");
            }
        }

        private void DeleteProject(string projectId)
        {
            var info = SavedProjects.FirstOrDefault(p => p.ProjectId == projectId);
            if (info == null) return;

            var result = Views.StyledDialog.ShowConfirm(
                "Confirm Delete",
                $"Delete project '{info.DisplayName}'?",
                "This action cannot be undone. The saved project file will be permanently removed.",
                null,
                Views.StyledDialog.DialogIcon.Danger,
                MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                ProjectService.Delete(projectId);
                if (CurrentProjectId == projectId)
                {
                    CurrentProjectId = null;
                    CurrentProjectName = "Untitled Project";
                }
                RefreshProjectList();
                AppendOutput($"Project deleted: {info.DisplayName}");
            }
            catch (Exception ex)
            {
                AppendOutput($"ERROR deleting: {ex.Message}");
            }
        }

        private string PromptProjectName()
        {
            var defaultName = string.IsNullOrWhiteSpace(Context.ProjectName) ? "My Migration" : Context.ProjectName;
            var bgColor = System.Windows.Media.Color.FromRgb(0x0F, 0x14, 0x20);
            var surfaceColor = System.Windows.Media.Color.FromRgb(0x1A, 0x20, 0x35);
            var borderColor = System.Windows.Media.Color.FromRgb(0x2A, 0x34, 0x54);
            var textColor = System.Windows.Media.Color.FromRgb(0xF0, 0xF3, 0xFF);
            var accentColor = System.Windows.Media.Color.FromRgb(0xFF, 0x6B, 0x35);

            var inputWin = new Window
            {
                Title = "Save Project",
                Width = 450, Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                ResizeMode = ResizeMode.NoResize,
                Background = new System.Windows.Media.SolidColorBrush(bgColor),
                WindowStyle = WindowStyle.ToolWindow
            };

            var sp = new System.Windows.Controls.StackPanel { Margin = new Thickness(24) };

            var lbl = new System.Windows.Controls.TextBlock
            {
                Text = "Project Name:",
                Foreground = new System.Windows.Media.SolidColorBrush(textColor),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var tb = new System.Windows.Controls.TextBox
            {
                Text = defaultName,
                FontSize = 14,
                Padding = new Thickness(12, 10, 12, 10),
                Background = new System.Windows.Media.SolidColorBrush(surfaceColor),
                Foreground = new System.Windows.Media.SolidColorBrush(textColor),
                BorderBrush = new System.Windows.Media.SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                CaretBrush = new System.Windows.Media.SolidColorBrush(accentColor)
            };

            var btnPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 18, 0, 0)
            };

            var btnOk = new System.Windows.Controls.Button
            {
                Content = "Save",
                Width = 100, Height = 36,
                FontSize = 13, FontWeight = FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.White,
                Background = new System.Windows.Media.SolidColorBrush(accentColor),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = true
            };

            var btnCancel = new System.Windows.Controls.Button
            {
                Content = "Cancel",
                Width = 100, Height = 36,
                FontSize = 13, FontWeight = FontWeights.SemiBold,
                Foreground = new System.Windows.Media.SolidColorBrush(textColor),
                Background = new System.Windows.Media.SolidColorBrush(surfaceColor),
                BorderBrush = new System.Windows.Media.SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                IsCancel = true
            };

            string result = null;
            btnOk.Click += (s, e) => { result = tb.Text; inputWin.Close(); };
            btnCancel.Click += (s, e) => inputWin.Close();

            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);
            sp.Children.Add(lbl);
            sp.Children.Add(tb);
            sp.Children.Add(btnPanel);
            inputWin.Content = sp;

            inputWin.ContentRendered += (s, e) => { tb.SelectAll(); tb.Focus(); };
            inputWin.ShowDialog();

            return string.IsNullOrWhiteSpace(result) ? null : result.Trim();
        }

        private ProjectSaveData CreateSaveData()
        {
            var data = new ProjectSaveData
            {
                ProjectId = CurrentProjectId ?? Guid.NewGuid().ToString("N"),
                DisplayName = CurrentProjectName,
                CreatedUtc = DateTime.UtcNow,
                UserName = Context.UserName,
                UserEmail = Context.UserEmail,
                AutoCrlf = Context.AutoCrlf,
                LongPaths = Context.LongPaths,
                DefaultBranch = Context.DefaultBranch,
                SourcePath = Context.SourcePath,
                StagingPath = Context.StagingPath,
                ProjectName = Context.ProjectName,
                VsVersion = Context.VsVersion,
                SlnFilePath = Context.SlnFilePath,
                GitignoreTemplate = Context.GitignoreTemplate,
                IncludeWebGitignore = Context.IncludeWebGitignore,
                RemotePlatform = Context.RemotePlatform,
                GitLabUrl = Context.GitLabUrl,
                GitLabProjectUrl = Context.GitLabProjectUrl,
                AuthMethod = Context.AuthMethod,
                CommitMessage = Context.CommitMessage,
                RemoteUrl = Context.RemoteUrl,
                VerifyPath = Context.VerifyPath,
                DailyWorkingPath = DailyWorkingPath,
                ClearCaseFilesRemoved = Context.ClearCaseFilesRemoved,
                BuildArtifactsRemoved = Context.BuildArtifactsRemoved,
                ProjectSizeBefore = Context.ProjectSizeBefore,
                ProjectSizeAfter = Context.ProjectSizeAfter,
                TotalFilesForCommit = Context.TotalFilesForCommit,
                CurrentStepIndex = CurrentStepIndex,
            };

            foreach (var step in Steps)
                data.StepStatuses.Add(new StepStatusEntry { Number = step.Number, Status = step.Status });

            foreach (var item in Context.ChecklistItems)
                data.ChecklistEntries.Add(new ChecklistEntry { Phase = item.Phase, Text = item.Text, IsDone = item.IsDone });

            foreach (var item in Context.PreservedSubDirs)
                data.PreservedSubDirs.Add(new PreservedSubDirEntry
                {
                    FullPath = item.FullPath,
                    RelativePath = item.RelativePath,
                    ParentBuildDir = item.ParentBuildDir,
                    Name = item.Name,
                    IsPreserved = item.IsPreserved
                });

            return data;
        }

        private void RestoreFromSaveData(ProjectSaveData data)
        {
            CurrentProjectId = data.ProjectId;
            CurrentProjectName = data.DisplayName;

            Context.UserName = data.UserName ?? "";
            Context.UserEmail = data.UserEmail ?? "";
            Context.AutoCrlf = data.AutoCrlf;
            Context.LongPaths = data.LongPaths;
            Context.DefaultBranch = data.DefaultBranch ?? "main";
            Context.SourcePath = data.SourcePath ?? "";
            Context.StagingPath = data.StagingPath ?? "";
            Context.ProjectName = data.ProjectName ?? "";
            Context.VsVersion = data.VsVersion ?? "";
            Context.SlnFilePath = data.SlnFilePath ?? "";
            Context.GitignoreTemplate = data.GitignoreTemplate ?? "VS2015-2019";
            Context.IncludeWebGitignore = data.IncludeWebGitignore;
            Context.RemotePlatform = data.RemotePlatform ?? "GitLab";
            Context.GitLabUrl = data.GitLabUrl ?? "";
            Context.GitLabProjectUrl = data.GitLabProjectUrl ?? "";
            Context.AuthMethod = data.AuthMethod ?? "HTTPS";
            Context.CommitMessage = data.CommitMessage ?? "";
            Context.RemoteUrl = data.RemoteUrl ?? "";
            Context.VerifyPath = data.VerifyPath ?? "";
            DailyWorkingPath = data.DailyWorkingPath ?? "";
            Context.ClearCaseFilesRemoved = data.ClearCaseFilesRemoved;
            Context.BuildArtifactsRemoved = data.BuildArtifactsRemoved;
            Context.ProjectSizeBefore = data.ProjectSizeBefore ?? "";
            Context.ProjectSizeAfter = data.ProjectSizeAfter ?? "";
            Context.TotalFilesForCommit = data.TotalFilesForCommit;

            foreach (var entry in data.StepStatuses)
            {
                var step = Steps.FirstOrDefault(s => s.Number == entry.Number);
                if (step != null) step.Status = entry.Status;
            }

            foreach (var entry in data.ChecklistEntries)
            {
                var item = Context.ChecklistItems.FirstOrDefault(c => c.Phase == entry.Phase && c.Text == entry.Text);
                if (item != null) item.IsDone = entry.IsDone;
            }

            Context.PreservedSubDirs.Clear();
            foreach (var entry in data.PreservedSubDirs)
            {
                Context.PreservedSubDirs.Add(new PreservedSubDir
                {
                    FullPath = entry.FullPath,
                    RelativePath = entry.RelativePath,
                    ParentBuildDir = entry.ParentBuildDir,
                    Name = entry.Name,
                    IsPreserved = entry.IsPreserved
                });
            }

            CurrentStepIndex = data.CurrentStepIndex;
            _consoleBuffer.Clear(); ConsoleOutput = "";

            // Re-evaluate auto-checkable items based on restored state
            _suppressDirty = true;
            RefreshChecklistFromState();
            _suppressDirty = false;
            IsDirty = false;
        }

        private void ResetToDefaults()
        {
            Context.UserName = "";
            Context.UserEmail = "";
            Context.AutoCrlf = true;
            Context.LongPaths = true;
            Context.DefaultBranch = "main";
            Context.SourcePath = "";
            Context.StagingPath = "";
            Context.ProjectName = "";
            Context.VsVersion = "";
            Context.SlnFilePath = "";
            Context.GitignoreTemplate = "VS2015-2019";
            Context.IncludeWebGitignore = false;
            Context.RemotePlatform = "GitLab";
            Context.GitLabUrl = "";
            Context.GitLabProjectUrl = "";
            Context.AuthMethod = "HTTPS";
            Context.PersonalAccessToken = "";
            Context.CommitMessage = "";
            Context.RemoteUrl = "";
            Context.VerifyPath = "";
            DailyWorkingPath = "";
            Context.ClearCaseFilesRemoved = 0;
            Context.BuildArtifactsRemoved = 0;
            Context.ProjectSizeBefore = "";
            Context.ProjectSizeAfter = "";
            Context.TotalFilesForCommit = 0;
            Context.PreservedSubDirs.Clear();
            Context.ChecklistItems = null; // forces re-creation via lazy getter
        }

        public void OnWindowClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!IsDirty) return;

            var result = Views.StyledDialog.ShowConfirm(
                "Unsaved Changes",
                "Save project before closing?",
                "You have unsaved changes that will be lost.",
                new[] { "Yes — Save and close", "No — Close without saving", "Cancel — Go back" },
                Views.StyledDialog.DialogIcon.Question);
            if (result == MessageBoxResult.Cancel) { e.Cancel = true; return; }
            if (result == MessageBoxResult.Yes) SaveProject();
        }
    }
}
