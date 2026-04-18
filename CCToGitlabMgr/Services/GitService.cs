using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CCToGitlabMgr.Services
{
    public class GitService
    {
        private readonly CommandRunner _runner;

        public event Action<string> Output;

        public GitService(CommandRunner runner)
        {
            _runner = runner;
            _runner.OutputReceived += line => Output?.Invoke(line);
            _runner.ErrorReceived += line => Output?.Invoke($"[stderr] {line}");
        }

        // === Global Config ===

        public async Task<CommandResult> SetGlobalConfigAsync(string key, string value, CancellationToken ct = default)
        {
            Output?.Invoke($"> git config --global {key} \"{value}\"");
            return await _runner.RunGitAsync($"config --global {key} \"{value}\"", null, ct);
        }

        public async Task<CommandResult> GetGlobalConfigAsync(CancellationToken ct = default)
        {
            Output?.Invoke("> git config --global --list");
            return await _runner.RunGitAsync("config --global --list", null, ct);
        }

        public async Task ApplyRecommendedConfigAsync(string userName, string userEmail, CancellationToken ct = default)
        {
            Output?.Invoke("=== Applying recommended Git configuration ===");

            await SetGlobalConfigAsync("user.name", userName, ct);
            await SetGlobalConfigAsync("user.email", userEmail, ct);
            await SetGlobalConfigAsync("core.autocrlf", "true", ct);
            await SetGlobalConfigAsync("init.defaultBranch", "main", ct);
            await SetGlobalConfigAsync("core.bigFileThreshold", "50m", ct);
            await SetGlobalConfigAsync("pull.rebase", "false", ct);
            await SetGlobalConfigAsync("core.longpaths", "true", ct);

            Output?.Invoke("=== Configuration applied successfully ===");
        }

        // === Repository Init ===

        public async Task<CommandResult> InitAsync(string workingDir, CancellationToken ct = default)
        {
            Output?.Invoke($"> git init");
            return await _runner.RunGitAsync("init", workingDir, ct);
        }

        public async Task<CommandResult> SetBranchNameAsync(string workingDir, string branchName = "main", CancellationToken ct = default)
        {
            Output?.Invoke($"> git branch -M {branchName}");
            return await _runner.RunGitAsync($"branch -M {branchName}", workingDir, ct);
        }

        // === Staging & Status ===

        public async Task<CommandResult> StatusAsync(string workingDir, CancellationToken ct = default)
        {
            Output?.Invoke("> git status");
            return await _runner.RunGitAsync("status", workingDir, ct);
        }

        public async Task<CommandResult> AddAllAsync(string workingDir, CancellationToken ct = default)
        {
            Output?.Invoke("> git add .");
            return await _runner.RunGitAsync("add .", workingDir, ct);
        }

        public async Task<CommandResult> ListUntrackedFilesAsync(string workingDir, CancellationToken ct = default)
        {
            Output?.Invoke("> git ls-files --others --exclude-standard");
            return await _runner.RunGitAsync("ls-files --others --exclude-standard", workingDir, ct);
        }

        public async Task<CommandResult> ListIgnoredFilesAsync(string workingDir, CancellationToken ct = default)
        {
            Output?.Invoke("> git ls-files --others --ignored --exclude-standard");
            return await _runner.RunGitAsync("ls-files --others --ignored --exclude-standard", workingDir, ct);
        }

        public async Task<CommandResult> CountFilesAsync(string workingDir, CancellationToken ct = default)
        {
            return await _runner.RunGitAsync("ls-files --others --exclude-standard", workingDir, ct);
        }

        // === Commit ===

        public async Task<CommandResult> CommitAsync(string workingDir, string message, CancellationToken ct = default)
        {
            Output?.Invoke($"> git commit -m \"{TruncateForDisplay(message)}\"");
            // Use -m with the message, escaping quotes
            var escapedMessage = message.Replace("\"", "\\\"");
            return await _runner.RunGitAsync($"commit -m \"{escapedMessage}\"", workingDir, ct);
        }

        // === Remote ===

        public async Task<CommandResult> AddRemoteAsync(string workingDir, string url, string name = "origin", CancellationToken ct = default)
        {
            Output?.Invoke($"> git remote add {name} {url}");
            return await _runner.RunGitAsync($"remote add {name} \"{url}\"", workingDir, ct);
        }

        public async Task<CommandResult> ShowRemoteAsync(string workingDir, CancellationToken ct = default)
        {
            Output?.Invoke("> git remote -v");
            return await _runner.RunGitAsync("remote -v", workingDir, ct);
        }

        // === Push / Pull ===

        public async Task<CommandResult> PushAsync(string workingDir, string remoteName = "origin", string branch = "main", bool setUpstream = true, CancellationToken ct = default)
        {
            var upstream = setUpstream ? "-u " : "";
            Output?.Invoke($"> git push {upstream}{remoteName} {branch}");
            return await _runner.RunGitAsync($"push {upstream}{remoteName} {branch}", workingDir, ct);
        }

        public async Task<CommandResult> PullAsync(string workingDir, CancellationToken ct = default)
        {
            Output?.Invoke("> git pull");
            return await _runner.RunGitAsync("pull", workingDir, ct);
        }

        // === Clone ===

        public async Task<CommandResult> CloneAsync(string url, string targetDir, CancellationToken ct = default)
        {
            Output?.Invoke($"> git clone {url} \"{targetDir}\"");
            var parentDir = Path.GetDirectoryName(targetDir);
            var folderName = Path.GetFileName(targetDir);
            return await _runner.RunGitAsync($"clone \"{url}\" \"{folderName}\"", parentDir, ct);
        }

        // === Branch ===

        public async Task<CommandResult> CheckoutAsync(string workingDir, string branch, bool create = false, CancellationToken ct = default)
        {
            var flag = create ? "-b " : "";
            Output?.Invoke($"> git checkout {flag}{branch}");
            return await _runner.RunGitAsync($"checkout {flag}{branch}", workingDir, ct);
        }

        public async Task<CommandResult> BranchListAsync(string workingDir, CancellationToken ct = default)
        {
            Output?.Invoke("> git branch -a");
            return await _runner.RunGitAsync("branch -a", workingDir, ct);
        }

        // === Log ===

        public async Task<CommandResult> LogAsync(string workingDir, int count = 10, CancellationToken ct = default)
        {
            Output?.Invoke($"> git log --oneline -{count}");
            return await _runner.RunGitAsync($"log --oneline -{count}", workingDir, ct);
        }

        public async Task<CommandResult> LogDetailedAsync(string workingDir, int count = 30, CancellationToken ct = default)
        {
            Output?.Invoke($"> git log -{count} --pretty=format:...");
            return await _runner.RunGitAsync(
                $"log -{count} --pretty=format:\"%h|%ai|%an|%s\"",
                workingDir, ct);
        }

        // === Tags ===

        public async Task<CommandResult> CreateTagAsync(string workingDir, string tagName, string message = null, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(message))
            {
                Output?.Invoke($"> git tag {tagName}");
                return await _runner.RunGitAsync($"tag \"{tagName}\"", workingDir, ct);
            }
            else
            {
                var escapedMsg = message.Replace("\"", "\\\"");
                Output?.Invoke($"> git tag -a {tagName} -m \"{TruncateForDisplay(message)}\"");
                return await _runner.RunGitAsync($"tag -a \"{tagName}\" -m \"{escapedMsg}\"", workingDir, ct);
            }
        }

        public async Task<CommandResult> ListTagsAsync(string workingDir, CancellationToken ct = default)
        {
            Output?.Invoke("> git tag -l --sort=-v:refname");
            return await _runner.RunGitAsync("tag -l --sort=-v:refname", workingDir, ct);
        }

        public async Task<CommandResult> ListTagsWithMessagesAsync(string workingDir, CancellationToken ct = default)
        {
            Output?.Invoke("> git tag -l -n99 --sort=-v:refname");
            return await _runner.RunGitAsync("tag -l -n99 --sort=-v:refname", workingDir, ct);
        }

        public async Task<CommandResult> PushTagAsync(string workingDir, string tagName, string remote = "origin", CancellationToken ct = default)
        {
            Output?.Invoke($"> git push {remote} {tagName}");
            return await _runner.RunGitAsync($"push \"{remote}\" \"{tagName}\"", workingDir, ct);
        }

        public async Task<CommandResult> PushAllTagsAsync(string workingDir, string remote = "origin", CancellationToken ct = default)
        {
            Output?.Invoke($"> git push {remote} --tags");
            return await _runner.RunGitAsync($"push \"{remote}\" --tags", workingDir, ct);
        }

        public async Task<CommandResult> DeleteTagAsync(string workingDir, string tagName, CancellationToken ct = default)
        {
            Output?.Invoke($"> git tag -d {tagName}");
            return await _runner.RunGitAsync($"tag -d \"{tagName}\"", workingDir, ct);
        }

        public async Task<CommandResult> ShowTagAsync(string workingDir, string tagName, CancellationToken ct = default)
        {
            Output?.Invoke($"> git show {tagName}");
            return await _runner.RunGitAsync($"show \"{tagName}\" --no-patch", workingDir, ct);
        }

        // === Stash ===

        public async Task<CommandResult> StashAsync(string workingDir, string message = null, CancellationToken ct = default)
        {
            var msg = string.IsNullOrEmpty(message) ? "" : $" save \"{message}\"";
            Output?.Invoke($"> git stash{msg}");
            return await _runner.RunGitAsync($"stash{msg}", workingDir, ct);
        }

        public async Task<CommandResult> StashPopAsync(string workingDir, CancellationToken ct = default)
        {
            Output?.Invoke("> git stash pop");
            return await _runner.RunGitAsync("stash pop", workingDir, ct);
        }

        // === Diff ===

        public async Task<CommandResult> DiffAsync(string workingDir, bool staged = false, CancellationToken ct = default)
        {
            var flag = staged ? " --staged" : "";
            Output?.Invoke($"> git diff{flag}");
            return await _runner.RunGitAsync($"diff{flag}", workingDir, ct);
        }

        // === SSH Key ===

        public async Task<CommandResult> GenerateSshKeyAsync(string email, CancellationToken ct = default)
        {
            Output?.Invoke($"> ssh-keygen -t ed25519 -C \"{email}\" -N \"\" -f ~/.ssh/id_ed25519");
            return await _runner.RunAsync("ssh-keygen", $"-t ed25519 -C \"{email}\" -N \"\" -f \"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "id_ed25519")}\"", null, ct);
        }

        // === Helpers ===

        public async Task<string> GetVersionAsync(CancellationToken ct = default)
        {
            var result = await _runner.RunGitAsync("--version", null, ct);
            return result.Success ? result.Output.Trim() : null;
        }

        /// <summary>
        /// Run the complete migration flow
        /// </summary>
        public async Task<bool> RunMigrationAsync(string workingDir, string commitMessage, string remoteUrl, string branch = "main", CancellationToken ct = default)
        {
            Output?.Invoke("========================================");
            Output?.Invoke("  Starting ClearCase -> GitLab Migration");
            Output?.Invoke("========================================");

            // 1. Init
            var result = await InitAsync(workingDir, ct);
            if (!result.Success) { Output?.Invoke("FAILED: git init"); return false; }

            // 2. Branch
            result = await SetBranchNameAsync(workingDir, branch, ct);
            if (!result.Success) { Output?.Invoke("FAILED: branch rename"); return false; }

            // 3. Add
            result = await AddAllAsync(workingDir, ct);
            if (!result.Success) { Output?.Invoke("FAILED: git add"); return false; }

            // 4. Status check
            await StatusAsync(workingDir, ct);

            // 5. Commit
            result = await CommitAsync(workingDir, commitMessage, ct);
            if (!result.Success) { Output?.Invoke("FAILED: git commit"); return false; }

            // 6. Add remote
            result = await AddRemoteAsync(workingDir, remoteUrl, "origin", ct);
            if (!result.Success) { Output?.Invoke("FAILED: git remote add"); return false; }

            // 7. Push
            result = await PushAsync(workingDir, "origin", branch, true, ct);
            if (!result.Success)
            {
                Output?.Invoke("FAILED: git push");
                Output?.Invoke("Tip: Check your credentials and that the GitLab project is empty.");
                return false;
            }

            Output?.Invoke("========================================");
            Output?.Invoke("  Migration completed successfully!");
            Output?.Invoke("========================================");
            return true;
        }

        private static string TruncateForDisplay(string text, int maxLen = 80)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var firstLine = text.Split('\n')[0];
            return firstLine.Length > maxLen ? firstLine.Substring(0, maxLen) + "..." : firstLine;
        }
    }
}
