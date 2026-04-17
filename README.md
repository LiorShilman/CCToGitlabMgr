# CCToGitlabMgr

`CCToGitlabMgr` is a Windows WPF desktop tool for guiding a ClearCase-to-GitLab migration.

It wraps the common migration flow into a step-by-step UI so you can prepare a project, clean legacy artifacts, generate a suitable `.gitignore`, initialize Git, create the first commit, push to a remote repository, and verify the migrated result.

## What It Does

The application walks through these stages:

1. Configure recommended global Git settings.
2. Copy a ClearCase-backed project into a staging folder.
3. Detect the Visual Studio solution and recommend a matching `.gitignore`.
4. Clean ClearCase leftovers and build artifacts.
5. Prepare the remote repository details.
6. Run the initial Git migration flow.
7. Verify the migrated project from a clean path.
8. Track a post-migration checklist.

## Main Features

- WPF desktop UI with a guided multi-step workflow
- Built-in console output for Git and cleanup operations
- Automatic `.sln` discovery in the staged project
- Visual Studio version detection from the solution file
- Recommended `.gitignore` template selection
- Cleanup helpers for ClearCase metadata and build output
- Git initialization, add, commit, remote setup, and push
- Verification and migration checklist support

## Tech Stack

- C#
- WPF
- .NET Framework 4.7.2
- Visual Studio 2022

## Requirements

- Windows
- Git for Windows installed and available in `PATH`
- Visual Studio 2022 with `.NET desktop development`
- `.NET Framework 4.7.2 targeting pack`

## Running The App

1. Open `CCToGitlabMgr.sln` in Visual Studio 2022.
2. Set `CCToGitlabMgr` as the startup project if needed.
3. Build the solution.
4. Run with `F5` or `Start`.

## Typical Workflow

1. Enter your Git user name and email in the `Git Config` step.
2. Choose the ClearCase source folder and a separate staging folder in `Prepare`.
3. Copy the project to staging and let the tool detect the solution details.
4. Run cleanup actions for ClearCase artifacts and build output.
5. Generate or preview the recommended `.gitignore`.
6. Enter the remote repository URL and initial commit message.
7. Run the migration flow to initialize Git, commit, and push.
8. Verify the migrated project from a clean location.

## Project Structure

- `CCToGitlabMgr/Views` - WPF step views
- `CCToGitlabMgr/ViewModels` - UI logic and workflow commands
- `CCToGitlabMgr/Services` - Git execution, cleanup, and solution parsing
- `CCToGitlabMgr/Models` - migration state and checklist models
- `CCToGitlabMgr/Themes` - visual styling resources

## Notes

- Work on a staging copy, not directly inside the original ClearCase workspace.
- Review cleanup results before committing.
- Push to an empty remote repository for the initial migration when possible.
- The app can be used with other Git remotes as well, even though the workflow is named around GitLab.

## Status

This project is currently focused on streamlining first-pass repository migration for legacy Visual Studio solutions.
