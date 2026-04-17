using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CCToGitlabMgr.Services
{
    public static class SlnParser
    {
        public class SlnInfo
        {
            public string FormatVersion { get; set; }
            public string VisualStudioVersion { get; set; }
            public string DetectedVS { get; set; }
            public string RecommendedGitignore { get; set; }
        }

        /// <summary>
        /// Parse a .sln file and detect the Visual Studio version
        /// </summary>
        public static SlnInfo Parse(string slnPath)
        {
            if (!File.Exists(slnPath))
                return null;

            var lines = File.ReadLines(slnPath).Take(10).ToList();
            var info = new SlnInfo();

            foreach (var line in lines)
            {
                // Match: Microsoft Visual Studio Solution File, Format Version XX.XX
                var fmtMatch = Regex.Match(line, @"Format Version\s+(\d+\.?\d*)");
                if (fmtMatch.Success)
                    info.FormatVersion = fmtMatch.Groups[1].Value;

                // Match: VisualStudioVersion = XX.X.XXXXX.X
                var vsMatch = Regex.Match(line, @"VisualStudioVersion\s*=\s*(\d+)");
                if (vsMatch.Success)
                    info.VisualStudioVersion = vsMatch.Groups[1].Value;
            }

            // Determine VS version from format + VS version header
            if (info.FormatVersion != null)
            {
                if (info.FormatVersion.StartsWith("11"))
                {
                    info.DetectedVS = "2010";
                    info.RecommendedGitignore = "VS2010";
                }
                else if (info.FormatVersion.StartsWith("12"))
                {
                    // Could be 2013, 2015, 2017, 2019, or 2022
                    if (info.VisualStudioVersion != null)
                    {
                        int major;
                        if (int.TryParse(info.VisualStudioVersion, out major))
                        {
                            if (major >= 17)
                            {
                                info.DetectedVS = "2022";
                                info.RecommendedGitignore = "VS2022";
                            }
                            else if (major >= 15)
                            {
                                info.DetectedVS = major == 16 ? "2019" : "2017";
                                info.RecommendedGitignore = "VS2015-2019";
                            }
                            else if (major >= 14)
                            {
                                info.DetectedVS = "2015";
                                info.RecommendedGitignore = "VS2015-2019";
                            }
                            else
                            {
                                info.DetectedVS = "2013";
                                info.RecommendedGitignore = "VS2010";
                            }
                        }
                    }
                    else
                    {
                        info.DetectedVS = "2015/2019";
                        info.RecommendedGitignore = "VS2015-2019";
                    }
                }
                else if (info.FormatVersion.StartsWith("10"))
                {
                    info.DetectedVS = "2008";
                    info.RecommendedGitignore = "VS2010";
                }
            }

            if (info.DetectedVS == null)
            {
                info.DetectedVS = "Unknown";
                info.RecommendedGitignore = "VS2015-2019";
            }

            return info;
        }

        /// <summary>
        /// Find the first .sln file in a directory
        /// </summary>
        public static string FindSln(string directory)
        {
            if (!Directory.Exists(directory))
                return null;

            var slnFiles = Directory.GetFiles(directory, "*.sln", SearchOption.TopDirectoryOnly);
            if (slnFiles.Length > 0)
                return slnFiles[0];

            // Try one level deep
            slnFiles = Directory.GetFiles(directory, "*.sln", SearchOption.AllDirectories);
            return slnFiles.Length > 0 ? slnFiles[0] : null;
        }
    }
}
