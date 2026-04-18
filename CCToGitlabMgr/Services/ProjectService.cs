using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CCToGitlabMgr.Models;

namespace CCToGitlabMgr.Services
{
    public class ProjectInfo
    {
        public string ProjectId { get; set; }
        public string DisplayName { get; set; }
        public string FilePath { get; set; }
        public DateTime LastSavedUtc { get; set; }
        public string LastSavedDisplay => LastSavedUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    }

    public class ProjectService
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(ProjectSaveData));

        public static string ProjectsFolder { get; } =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CCToGitlabMgr", "Projects");

        public static void EnsureFolder()
        {
            if (!Directory.Exists(ProjectsFolder))
                Directory.CreateDirectory(ProjectsFolder);
        }

        public static string GetFilePath(string projectId)
        {
            return Path.Combine(ProjectsFolder, projectId + ".ccmig");
        }

        public static List<ProjectInfo> ListProjects()
        {
            EnsureFolder();
            var results = new List<ProjectInfo>();
            foreach (var file in Directory.GetFiles(ProjectsFolder, "*.ccmig"))
            {
                try
                {
                    using (var stream = File.OpenRead(file))
                    {
                        var data = (ProjectSaveData)_serializer.Deserialize(stream);
                        results.Add(new ProjectInfo
                        {
                            ProjectId = data.ProjectId,
                            DisplayName = data.DisplayName,
                            FilePath = file,
                            LastSavedUtc = data.LastSavedUtc
                        });
                    }
                }
                catch { }
            }
            return results.OrderByDescending(p => p.LastSavedUtc).ToList();
        }

        public static void Save(ProjectSaveData data)
        {
            EnsureFolder();
            data.LastSavedUtc = DateTime.UtcNow;
            var path = GetFilePath(data.ProjectId);
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                _serializer.Serialize(stream, data);
            }
        }

        public static ProjectSaveData Load(string projectId)
        {
            var path = GetFilePath(projectId);
            using (var stream = File.OpenRead(path))
            {
                return (ProjectSaveData)_serializer.Deserialize(stream);
            }
        }

        public static void Delete(string projectId)
        {
            var path = GetFilePath(projectId);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
