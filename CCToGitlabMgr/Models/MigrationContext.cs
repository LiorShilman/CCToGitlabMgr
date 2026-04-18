using System.Collections.Generic;
using System.Collections.ObjectModel;
using CCToGitlabMgr.ViewModels;

namespace CCToGitlabMgr.Models
{
    public class PreservedSubDir : BaseViewModel
    {
        /// <summary>Full path of the subdirectory</summary>
        public string FullPath { get; set; }

        /// <summary>Path relative to staging root (e.g. MyApp\bin\Debug\Data)</summary>
        public string RelativePath { get; set; }

        /// <summary>Name of the parent build folder (e.g. Debug, Release, bin)</summary>
        public string ParentBuildDir { get; set; }

        /// <summary>Just the folder name (e.g. Data)</summary>
        public string Name { get; set; }

        private bool _isPreserved = true;
        /// <summary>If true, this folder will be kept and added as gitignore exception</summary>
        public bool IsPreserved { get => _isPreserved; set => SetProperty(ref _isPreserved, value); }
    }


    public class MigrationContext : BaseViewModel
    {
        // == Step 1: Git Config ==
        private string _userName = "";
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

        private string _userEmail = "";
        public string UserEmail { get => _userEmail; set => SetProperty(ref _userEmail, value); }

        private bool _autocrlf = true;
        public bool AutoCrlf { get => _autocrlf; set => SetProperty(ref _autocrlf, value); }

        private bool _longPaths = true;
        public bool LongPaths { get => _longPaths; set => SetProperty(ref _longPaths, value); }

        private string _defaultBranch = "main";
        public string DefaultBranch { get => _defaultBranch; set => SetProperty(ref _defaultBranch, value); }

        // == Step 2: Project Paths ==
        private string _sourcePath = "";
        public string SourcePath { get => _sourcePath; set => SetProperty(ref _sourcePath, value); }

        private string _stagingPath = "";
        public string StagingPath { get => _stagingPath; set => SetProperty(ref _stagingPath, value); }

        private string _projectName = "";
        public string ProjectName { get => _projectName; set => SetProperty(ref _projectName, value); }

        // == Step 2: Detected VS Version ==
        private string _vsVersion = "";
        public string VsVersion { get => _vsVersion; set => SetProperty(ref _vsVersion, value); }

        private string _slnFilePath = "";
        public string SlnFilePath { get => _slnFilePath; set => SetProperty(ref _slnFilePath, value); }

        // == Step 4: Gitignore ==
        private string _gitignoreTemplate = "VS2015-2019";
        public string GitignoreTemplate { get => _gitignoreTemplate; set => SetProperty(ref _gitignoreTemplate, value); }

        private bool _includeWebGitignore;
        public bool IncludeWebGitignore { get => _includeWebGitignore; set => SetProperty(ref _includeWebGitignore, value); }

        // == Step 5: GitLab ==
        private string _gitlabUrl = "";
        public string GitLabUrl { get => _gitlabUrl; set => SetProperty(ref _gitlabUrl, value); }

        private string _gitlabProjectUrl = "";
        public string GitLabProjectUrl { get => _gitlabProjectUrl; set => SetProperty(ref _gitlabProjectUrl, value); }

        private string _authMethod = "HTTPS";
        public string AuthMethod { get => _authMethod; set => SetProperty(ref _authMethod, value); }

        private string _personalAccessToken = "";
        public string PersonalAccessToken { get => _personalAccessToken; set => SetProperty(ref _personalAccessToken, value); }

        // == Step 6: Migration ==
        private string _commitMessage = "";
        public string CommitMessage { get => _commitMessage; set => SetProperty(ref _commitMessage, value); }

        private string _remoteUrl = "";
        public string RemoteUrl { get => _remoteUrl; set => SetProperty(ref _remoteUrl, value); }

        // == Step 7: Verify ==
        private string _verifyPath = "";
        public string VerifyPath { get => _verifyPath; set => SetProperty(ref _verifyPath, value); }

        // == Cleanup Stats ==
        private int _clearCaseFilesRemoved;
        public int ClearCaseFilesRemoved { get => _clearCaseFilesRemoved; set => SetProperty(ref _clearCaseFilesRemoved, value); }

        private int _buildArtifactsRemoved;
        public int BuildArtifactsRemoved { get => _buildArtifactsRemoved; set => SetProperty(ref _buildArtifactsRemoved, value); }

        private string _projectSizeBefore = "";
        public string ProjectSizeBefore { get => _projectSizeBefore; set => SetProperty(ref _projectSizeBefore, value); }

        private string _projectSizeAfter = "";
        public string ProjectSizeAfter { get => _projectSizeAfter; set => SetProperty(ref _projectSizeAfter, value); }

        private int _totalFilesForCommit;
        public int TotalFilesForCommit { get => _totalFilesForCommit; set => SetProperty(ref _totalFilesForCommit, value); }

        // == Preserved subdirectories inside build output ==
        public ObservableCollection<PreservedSubDir> PreservedSubDirs { get; } = new ObservableCollection<PreservedSubDir>();

        // == Checklist ==
        private List<ChecklistItem> _checklistItems;
        public List<ChecklistItem> ChecklistItems
        {
            get => _checklistItems ?? (_checklistItems = CreateChecklist());
            set => SetProperty(ref _checklistItems, value);
        }

        private List<ChecklistItem> CreateChecklist()
        {
            return new List<ChecklistItem>
            {
                // Before
                new ChecklistItem("pre", "Git for Windows installed"),
                new ChecklistItem("pre", "Global git config applied"),
                new ChecklistItem("pre", "GitLab credentials configured"),
                new ChecklistItem("pre", "Empty GitLab project created"),
                new ChecklistItem("pre", "VS version identified"),
                // During
                new ChecklistItem("during", "Code copied to staging"),
                new ChecklistItem("during", "ClearCase artifacts cleaned"),
                new ChecklistItem("during", "Build artifacts cleaned"),
                new ChecklistItem("during", "Project size under 1GB"),
                new ChecklistItem("during", ".gitignore placed at root"),
                new ChecklistItem("during", "git status looks clean"),
                new ChecklistItem("during", "First commit done"),
                new ChecklistItem("during", "Push to GitLab succeeded"),
                // After
                new ChecklistItem("post", "Fresh clone done"),
                new ChecklistItem("post", "Project opens in VS without errors"),
                new ChecklistItem("post", "Build succeeds (0 errors)"),
                new ChecklistItem("post", "Project runs correctly (F5)"),
                new ChecklistItem("post", "Protected branches configured"),
                new ChecklistItem("post", "README updated"),
                new ChecklistItem("post", "Team notified with clone URL"),
                new ChecklistItem("post", "ClearCase view archived"),
            };
        }
    }

    public class ChecklistItem : BaseViewModel
    {
        public string Phase { get; set; }
        public string Text { get; set; }

        private bool _isDone;
        public bool IsDone { get => _isDone; set => SetProperty(ref _isDone, value); }

        public ChecklistItem() { }

        public ChecklistItem(string phase, string text)
        {
            Phase = phase;
            Text = text;
        }
    }
}
