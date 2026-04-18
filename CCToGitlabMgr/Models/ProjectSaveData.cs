using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CCToGitlabMgr.Models
{
    [XmlRoot("MigrationProject")]
    public class ProjectSaveData
    {
        // Metadata
        public string ProjectId { get; set; }
        public string DisplayName { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime LastSavedUtc { get; set; }

        // Context fields
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public bool AutoCrlf { get; set; }
        public bool LongPaths { get; set; }
        public string DefaultBranch { get; set; }
        public string SourcePath { get; set; }
        public string StagingPath { get; set; }
        public string ProjectName { get; set; }
        public string VsVersion { get; set; }
        public string SlnFilePath { get; set; }
        public string GitignoreTemplate { get; set; }
        public bool IncludeWebGitignore { get; set; }
        public string RemotePlatform { get; set; }
        public string GitLabUrl { get; set; }
        public string GitLabProjectUrl { get; set; }
        public string AuthMethod { get; set; }
        // PersonalAccessToken intentionally excluded for security
        public string CommitMessage { get; set; }
        public string RemoteUrl { get; set; }
        public string VerifyPath { get; set; }
        public string DailyWorkingPath { get; set; }
        public int ClearCaseFilesRemoved { get; set; }
        public int BuildArtifactsRemoved { get; set; }
        public string ProjectSizeBefore { get; set; }
        public string ProjectSizeAfter { get; set; }
        public int TotalFilesForCommit { get; set; }

        // Step statuses
        public int CurrentStepIndex { get; set; }
        public List<StepStatusEntry> StepStatuses { get; set; } = new List<StepStatusEntry>();

        // Checklist
        public List<ChecklistEntry> ChecklistEntries { get; set; } = new List<ChecklistEntry>();

        // Preserved sub-dirs
        public List<PreservedSubDirEntry> PreservedSubDirs { get; set; } = new List<PreservedSubDirEntry>();
    }

    public class StepStatusEntry
    {
        public int Number { get; set; }
        public string Status { get; set; }
        public StepStatusEntry() { }
    }

    public class ChecklistEntry
    {
        public string Phase { get; set; }
        public string Text { get; set; }
        public bool IsDone { get; set; }
        public ChecklistEntry() { }
    }

    public class PreservedSubDirEntry
    {
        public string FullPath { get; set; }
        public string RelativePath { get; set; }
        public string ParentBuildDir { get; set; }
        public string Name { get; set; }
        public bool IsPreserved { get; set; }
        public PreservedSubDirEntry() { }
    }
}
