namespace CCToGitlabMgr.Services
{
    public static class GitignoreTemplates
    {
        public static string GetTemplate(string vsVersion)
        {
            switch (vsVersion)
            {
                case "VS2010": return VS2010;
                case "VS2022": return VS2022;
                case "Angular": return Angular;
                case "Python": return Python;
                default: return VS2015_2019;
            }
        }

        public const string VS2010 = @"# =========================================
#  .gitignore for Visual Studio 2010
#  Project migrated from ClearCase
# =========================================

# ==== Build output ====
[Bb]in/
[Oo]bj/
[Dd]ebug/
[Dd]ebugPublic/
[Rr]elease/
[Rr]eleases/
x64/
x86/
build/
bld/
[Oo]ut/
[Ll]og/
[Ll]ogs/

# ==== Visual Studio 2010 user files ====
*.suo
*.user
*.userosscache
*.sln.docstates
*.vspscc
*.vssscc
*.ncb
*.sdf
*.cachefile
*.VC.db
*.VC.opendb
ipch/

# ==== ReSharper ====
_ReSharper*/
*.[Rr]e[Ss]harper
*.DotSettings.user

# ==== Test results ====
[Tt]est[Rr]esult*/
[Bb]uild[Ll]og.*
*.VisualState.xml
TestResult.xml
nunit-*.xml

# ==== NuGet ====
packages/
*.nupkg
!packages/build/
!packages/repositories.config

# ==== ClearCase leftovers (safety net) ====
*.keep
*.keep.*
*.contrib
*.contrib.*
view.dat
lost+found/
.copyarea.*

# ==== Windows system files ====
Thumbs.db
ehthumbs.db
Desktop.ini
$RECYCLE.BIN/
*.lnk

# ==== Temp / backup ====
*.bak
*.tmp
*.swp
*~";

        public const string VS2015_2019 = @"# =========================================
#  .gitignore for Visual Studio 2015 / 2019
#  Project migrated from ClearCase
# =========================================

# ==== Build output ====
[Bb]in/
[Oo]bj/
[Dd]ebug/
[Dd]ebugPublic/
[Rr]elease/
[Rr]eleases/
x64/
x86/
[Ww][Ii][Nn]32/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
build/
bld/
[Oo]ut/
[Ll]og/
[Ll]ogs/
msbuild.log
msbuild.err
msbuild.wrn

# ==== Visual Studio 2015+ cache folder ====
.vs/

# ==== User-specific files ====
*.rsuser
*.suo
*.user
*.userosscache
*.sln.docstates

# ==== Compiled source ====
*.com
*.class
*.dll
*.exe
*.o
*.so
*.pdb
*.ilk

# ==== NuGet ====
*.nupkg
*.snupkg
**/packages/*
!**/packages/build/
*.nuget.props
*.nuget.targets
project.lock.json
project.fragment.lock.json
artifacts/

# ==== Node.js / Web (for web projects) ====
node_modules/
npm-debug.log*
.npm
bower_components/
wwwroot/lib/

# ==== ReSharper / Rider ====
_ReSharper*/
*.[Rr]e[Ss]harper
*.DotSettings.user
.idea/

# ==== Test results ====
[Tt]est[Rr]esult*/
[Bb]uild[Ll]og.*
*.VisualState.xml
TestResult.xml
[Cc]overage*/
coverage.opencover.xml
coverage.cobertura.xml

# ==== Publish / deployment ====
[Pp]ublish/
*.[Pp]ublish.xml
*.azurePubxml
*.pubxml
*.pubxml.user
PublishScripts/

# ==== ClearCase leftovers ====
*.keep
*.keep.*
*.contrib
*.contrib.*
view.dat
lost+found/
.copyarea.*

# ==== Windows / OS ====
Thumbs.db
ehthumbs.db
Desktop.ini
$RECYCLE.BIN/
*.lnk

# ==== Temp / backup ====
*.bak
*.tmp
*.swp
*~";

        public const string VS2022 = @"# =========================================
#  .gitignore for Visual Studio 2022
#  Project migrated from ClearCase
# =========================================

# ==== Build output ====
[Bb]in/
[Oo]bj/
[Dd]ebug/
[Dd]ebugPublic/
[Rr]elease/
[Rr]eleases/
x64/
x86/
[Ww][Ii][Nn]32/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
build/
bld/
[Oo]ut/
[Ll]og/
[Ll]ogs/

# ==== Visual Studio 2022 cache ====
.vs/
*.rsuser
*.suo
*.user
*.userosscache
*.sln.docstates

# ==== Compiled / binary ====
*.dll
*.exe
*.pdb
*.ilk
*.obj
*.o
*.so
*.class

# ==== .NET SDK / NuGet ====
*.nupkg
*.snupkg
**/packages/*
!**/packages/build/
project.lock.json
project.fragment.lock.json
artifacts/
*.nuget.props
*.nuget.targets

# ==== User secrets (.NET) ====
*.pfx
*.snk
secrets.json
appsettings.*.Local.json
appsettings.Development.json

# ==== Web / Front-end ====
node_modules/
npm-debug.log*
yarn-debug.log*
yarn-error.log*
.pnpm-debug.log*
.npm
bower_components/
wwwroot/lib/
dist/
.next/
.nuxt/

# ==== Rider / ReSharper ====
_ReSharper*/
*.[Rr]e[Ss]harper
*.DotSettings.user
.idea/

# ==== Test / Coverage ====
[Tt]est[Rr]esult*/
[Bb]uild[Ll]og.*
*.VisualState.xml
TestResult.xml
[Cc]overage*/
coverage.opencover.xml
coverage.cobertura.xml
BenchmarkDotNet.Artifacts/

# ==== Publish / Azure / Docker ====
[Pp]ublish/
*.[Pp]ublish.xml
*.azurePubxml
*.pubxml
*.pubxml.user
PublishScripts/
docker-compose.override.yml

# ==== Logs ====
*.log
nlog.xml.log
serilog-*.txt

# ==== Visual Studio 2022 extras ====
.localhistory/
.mfractor/
[*.]VC.db
[*.]VC.opendb

# ==== ClearCase leftovers ====
*.keep
*.keep.*
*.contrib
*.contrib.*
view.dat
lost+found/
.copyarea.*

# ==== OS / Editor ====
Thumbs.db
ehthumbs.db
Desktop.ini
$RECYCLE.BIN/
*.lnk
.DS_Store

# ==== Temp / backup ====
*.bak
*.tmp
*.swp
*~
*.orig";

        public const string Angular = @"# =========================================
#  .gitignore for Angular / TypeScript
#  IDE: Visual Studio Code
#  Project migrated from ClearCase
# =========================================

# ==== Node.js / npm ====
node_modules/
npm-debug.log*
yarn-debug.log*
yarn-error.log*
pnpm-debug.log*
lerna-debug.log*
.pnpm-store/
.npm/
.yarn/
.yarn-integrity
package-lock.json.bak

# ==== Build output ====
dist/
dist-ssr/
build/
out/
out-tsc/
tmp/
*.tsbuildinfo
.tsbuildinfo

# ==== Angular CLI ====
.angular/
.angular/cache/
/connect.lock
/libpeerconnection.log
/typings
testem.log

# ==== Testing / Coverage ====
coverage/
*.lcov
.nyc_output/
karma-test-results/
junit.xml
e2e/*.js
e2e/*.map
cypress/videos/
cypress/screenshots/
cypress/downloads/
playwright-report/
test-results/

# ==== TypeScript ====
*.js.map
*.d.ts.map

# ==== Environment / Secrets ====
.env
.env.local
.env.development.local
.env.test.local
.env.production.local
.env.*.local
src/environments/environment.local.ts
src/environments/environment.prod.ts
secrets.json
*.pem
*.key

# ==== VS Code ====
.vscode/*
!.vscode/settings.json
!.vscode/tasks.json
!.vscode/launch.json
!.vscode/extensions.json
!.vscode/*.code-snippets
.history/
.ionide/

# ==== JetBrains IDEs ====
.idea/
*.iml
*.iws
*.ipr

# ==== Editor / OS ====
.DS_Store
Thumbs.db
ehthumbs.db
Desktop.ini
$RECYCLE.BIN/
*.swp
*.swo
*~
.project
.classpath
.c9/
*.launch
.settings/
*.sublime-workspace
*.sublime-project

# ==== Logs ====
*.log
logs/
debug.log
error.log

# ==== Cache directories ====
.cache/
.parcel-cache/
.next/
.nuxt/
.svelte-kit/
.turbo/
.vite/
.eslintcache
.stylelintcache

# ==== Storybook ====
storybook-static/
.storybook/build/

# ==== Misc ====
*.bak
*.tmp
*.orig
.tern-port
.vscode-test/

# ==== ClearCase leftovers ====
*.keep
*.keep.*
*.contrib
*.contrib.*
view.dat
lost+found/
.copyarea.*";

        public const string Python = @"# =========================================
#  .gitignore for Python
#  Project migrated from ClearCase
# =========================================

# ==== Byte-compiled / optimized ====
__pycache__/
*.py[cod]
*$py.class
*.pyo

# ==== Distribution / packaging ====
dist/
build/
*.egg-info/
*.egg
eggs/
wheels/
*.whl
sdist/
develop-eggs/
.installed.cfg
MANIFEST
pip-wheel-metadata/

# ==== Virtual environments ====
venv/
.venv/
env/
.env/
ENV/
.conda/
*.virtualenv

# ==== IDE / Editor ====
.vscode/
.idea/
*.iml
*.iws
*.ipr
*.sublime-workspace
*.sublime-project
.spyderproject
.spyproject
.ropeproject

# ==== Jupyter Notebook ====
.ipynb_checkpoints/

# ==== Testing / Coverage ====
.pytest_cache/
.tox/
.nox/
.coverage
.coverage.*
htmlcov/
*.cover
*.py,cover
.hypothesis/
nosetests.xml
coverage.xml
junit.xml
test-results/

# ==== mypy / type checking ====
.mypy_cache/
.pytype/
.pyre/

# ==== Environments / Secrets ====
.env
.env.local
.env.*
*.pem
*.key
secrets.json
config.local.py

# ==== Logs ====
*.log
logs/
pip-log.txt
pip-delete-this-directory.txt

# ==== Database ====
*.db
*.sqlite3

# ==== Celery ====
celerybeat-schedule
celerybeat.pid

# ==== Flask / Django ====
instance/
*.sage.py
db.sqlite3
local_settings.py
staticfiles/
media/

# ==== Scrapy ====
.scrapy

# ==== Sphinx documentation ====
docs/_build/

# ==== PyInstaller ====
*.manifest
*.spec

# ==== ClearCase leftovers ====
*.keep
*.keep.*
*.contrib
*.contrib.*
view.dat
lost+found/
.copyarea.*

# ==== OS / System ====
Thumbs.db
ehthumbs.db
Desktop.ini
$RECYCLE.BIN/
.DS_Store
*.lnk

# ==== Temp / backup ====
*.bak
*.tmp
*.swp
*~
*.orig";
    }
}
