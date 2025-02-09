using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using AMF.Tools.Properties;
using NuGet.VisualStudio;
using AMF.Common;

namespace AMF.Tools
{
    public class ReverseEngineeringAspNetCore : ReverseEngineeringServiceBase
    {
        private const string AspNetCoreStaticFilesPackageId = "Microsoft.AspNetCore.StaticFiles";
        private static readonly string RamlParserExpressionsPackageId = Settings.Default.RamlParserExpressionsPackageId;
        private static readonly string RamlParserExpressionsPackageVersion = Settings.Default.RamlParserExpressionsPackageVersion;
        private static readonly string RamlNetCoreApiExplorerPackageId = Settings.Default.RamlNetCoreApiExplorerPackageId;
        private static readonly string RamlNetCoreApiExplorerPackageVersion = Settings.Default.RamlNetCoreApiExplorerPackageVersion;

        public ReverseEngineeringAspNetCore(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Tracking.Track("Asp.Net Core Extract RAML");
        }

        protected override void ConfigureProject(Project proj)
        {
            ConfigureNetCoreStartUp(proj);
            ActivityLog.LogInformation(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource, "StatUp configuration added");

            AddCoreContentFiles(Path.GetDirectoryName(proj.FullName));
            ActivityLog.LogInformation(VisualStudioAutomationHelper.RamlVsToolsActivityLogSource, "Content files added");
        }

        protected override void InstallDependencies(Project proj, IVsPackageMetadata[] packs, IVsPackageInstaller installer,
            IVsPackageInstallerServices installerServices)
        {
            InstallNetCoreDependencies(proj, packs, installer, installerServices);
        }

        protected override void RemoveConfiguration(Project proj)
        {
            RemoveNetCoreStartUpConfiguration(proj);
        }

        protected override void RemoveDependencies(Project proj, IVsPackageInstallerServices installerServices, IVsPackageUninstaller installer)
        {
            // AMF.NetCoreApiExplorer
            if (installerServices.IsPackageInstalled(proj, RamlNetCoreApiExplorerPackageId))
            {
                installer.UninstallPackage(proj, RamlNetCoreApiExplorerPackageId, false);
            }
        }

        private void ConfigureNetCoreStartUp(Project proj)
        {
            var startUpPath = Path.Combine(Path.GetDirectoryName(proj.FullName), "Startup.cs");
            if (!File.Exists(startUpPath)) return;

            var lines = File.ReadAllLines(startUpPath).ToList();
            ConfigureNetCoreMvcServices(lines);
            ConfigureNetCoreMvc(lines);

            File.WriteAllText(startUpPath, string.Join(Environment.NewLine, lines));
        }

        private void AddCoreContentFiles(string destinationPath)
        {
            var extensionPath = Path.GetDirectoryName(GetType().Assembly.Location);
            var sourcePath = Path.Combine(extensionPath, "MetadataPackage" + Path.DirectorySeparatorChar + "Content");
            AddRamlController(sourcePath, destinationPath);
            AddViews(sourcePath, destinationPath);
            AddWebContent(sourcePath, destinationPath);
        }

        private void AddWebContent(string sourcePath, string destinationPath)
        {
            var webRoot = "wwwroot";
            CopyFilesRecursively(sourcePath, destinationPath, webRoot);
        }

        private void AddViews(string sourcePath, string destinationPath)
        {
            var subfolder = "Views" + Path.DirectorySeparatorChar + "Raml";
            CopyFilesRecursively(sourcePath, destinationPath, subfolder);
        }

        private static void CopyFilesRecursively(string sourcePath, string destinationPath, string subfolder)
        {
            var viewsSourcePath = Path.Combine(sourcePath, subfolder);
            var viewDestinationPath = Path.Combine(destinationPath, subfolder);

            CopyFilesRecusively(viewsSourcePath, viewDestinationPath);
        }

        private static void CopyFilesRecusively(string sourcePath, string destinationPath)
        {
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            var sourceFilePaths = Directory.GetFiles(sourcePath);
            foreach (var sourceFilePath in sourceFilePaths)
            {
                var destFileName = Path.Combine(destinationPath, Path.GetFileName(sourceFilePath));
                if (!File.Exists(destFileName))
                    File.Copy(sourceFilePath, destFileName, false);
            }

            // Copy sub folders
            var sourceSubFolders = Directory.GetDirectories(sourcePath);
            foreach (var sourceSubFolder in sourceSubFolders)
            {
                var lastDirectory = GetLastDirectory(sourceSubFolder);
                CopyFilesRecusively(sourceSubFolder, Path.Combine(destinationPath, lastDirectory));
            }
        }

        private static string GetLastDirectory(string path)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar);
            var index = path.LastIndexOf(Path.DirectorySeparatorChar);
            return path.Substring(index + 1);
        }

        private static void AddRamlController(string sourcePath, string destinationPath)
        {
            var controllersFolder = "Controllers";

            var controllersDestPath = Path.Combine(destinationPath, controllersFolder + Path.DirectorySeparatorChar);
            if (!Directory.Exists(controllersDestPath))
                Directory.CreateDirectory(controllersDestPath);

            var controllersPath = Path.Combine(sourcePath, controllersFolder);
            var ramlControllerDest = Path.Combine(controllersDestPath, "RamlController.cs");
            File.Copy(Path.Combine(controllersPath, "RamlController.class"), ramlControllerDest);
        }

        private void ConfigureNetCoreMvc(List<string> lines)
        {
            var appUsestaticfiles = "            app.UseStaticFiles();";

            if (lines.Any(l => l.Contains("app.UseStaticFiles();")))
                return;

            var line = TextFileHelper.FindLineWith(lines, "public void Configure(IApplicationBuilder app");
            if (line > 0)
                lines.Insert(line + 2, appUsestaticfiles);
        }

        private void ConfigureNetCoreMvcServices(List<string> lines)
        {
            var addService = "            services.AddScoped<AMF.WebApiExplorer.ApiExplorerDataFilter>();";

            int line;
            if (!lines.Any(l => l.Contains("services.AddMvc")))
            {
                line = TextFileHelper.FindLineWith(lines, "public void ConfigureServices");
                lines.Insert(line + 2, addService);
                lines.Insert(line + 2, AddMvcWithOptions());
                return;
            }

            line = TextFileHelper.FindLineWith(lines, "services.AddMvc()");
            if (line > 0)
            {
                var index = lines[line].IndexOf(".AddMvc(") + ".AddMvc(".Length;
                var modifiedText = lines[line].Insert(index, AddMvcWithOptions());
                lines[line] = modifiedText;

                lines.Insert(line, addService);

                return;
            }

            line = TextFileHelper.FindLineWith(lines, "services.AddMvc(options =>");
            if (line > 0 && lines[line + 1] == "{")
            {
                lines.Insert(line + 1, addService);
                lines.Insert(line + 2, AddOptions());
            }
        }

        private static string AddMvcWithOptions()
        {
            return "options =>" + Environment.NewLine
                   + "                {" + Environment.NewLine
                   + AddOptions()
                   + "                }";
        }

        private static string AddOptions()
        {
            return "                    options.Filters.AddService(typeof(AMF.WebApiExplorer.ApiExplorerDataFilter));" + Environment.NewLine
                   + "                    options.Conventions.Add(new AMF.WebApiExplorer.ApiExplorerVisibilityEnabledConvention());" + Environment.NewLine
                   + "                    options.Conventions.Add(new AMF.WebApiExplorer.ApiExplorerVisibilityDisabledConvention(typeof(AMF.WebApiExplorer.RamlController)));" + Environment.NewLine;
        }

        private void InstallNetCoreDependencies(Project proj, IVsPackageMetadata[] packs, IVsPackageInstaller installer, IVsPackageInstallerServices installerServices)
        {
            var version = VisualStudioAutomationHelper.GetTargetFrameworkVersion(proj);
            // RAML.Parser.Expressions
            if (!installerServices.IsPackageInstalled(proj, AspNetCoreStaticFilesPackageId))
            {
                if(version.StartsWith("1"))
                    NugetInstallerHelper.InstallPackageIfNeeded(proj, packs, installer, AspNetCoreStaticFilesPackageId, "1.0.0", 
                        Settings.Default.NugetExternalPackagesSource);
            }

            // RAML.Parser.Expressions
            if (!installerServices.IsPackageInstalled(proj, RamlParserExpressionsPackageId))
            {
                installer.InstallPackage(NugetPackagesSource, proj, RamlParserExpressionsPackageId, RamlParserExpressionsPackageVersion, false);
            }

            // AMF.NetCoreApiExplorer
            if (!installerServices.IsPackageInstalled(proj, RamlNetCoreApiExplorerPackageId))
            {
                installer.InstallPackage(NugetPackagesSource, proj, RamlNetCoreApiExplorerPackageId, RamlNetCoreApiExplorerPackageVersion, false);
            }
        }

        private void RemoveNetCoreStartUpConfiguration(Project proj)
        {
            var startUpPath = Path.Combine(Path.GetDirectoryName(proj.FullName), "Startup.cs");
            if (!File.Exists(startUpPath)) return;

            var lines = File.ReadAllLines(startUpPath).ToList();

            var addService = "            services.AddScoped<AMF.WebApiExplorer.ApiExplorerDataFilter>();";
            RemoveLine(lines, addService);

            var appUsestaticfiles = "            app.UseStaticFiles();";
            RemoveLine(lines, appUsestaticfiles);

            var option1 = "                    options.Filters.AddService(typeof(AMF.WebApiExplorer.ApiExplorerDataFilter));";
            RemoveLine(lines, option1);

            var option2 = "                    options.Conventions.Add(new AMF.WebApiExplorer.ApiExplorerVisibilityEnabledConvention());";
            RemoveLine(lines, option2);

            var option3 = "                    options.Conventions.Add(new AMF.WebApiExplorer.ApiExplorerVisibilityDisabledConvention(typeof(AMF.WebApiExplorer.RamlController)));";
            RemoveLine(lines, option3);

            File.WriteAllText(startUpPath, string.Join(Environment.NewLine, lines));
        }

        private static void RemoveLine(List<string> lines, string content)
        {
            var line = TextFileHelper.FindLineWith(lines, content);
            if (line > 0)
                lines.RemoveAt(line);
        }
    }
}