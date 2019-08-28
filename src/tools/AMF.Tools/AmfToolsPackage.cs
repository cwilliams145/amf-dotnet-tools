﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AMF.Common;
using Caliburn.Micro;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace AMF.Tools
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [InstalledProductRegistration("#1110", "#1112", "1.0", IconResourceID = 1400)] // Info on this package for Help/About
    [Guid(AmfToolsPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class AmfToolsPackage : AsyncPackage
    {
        /// <summary>
        /// VSPackage1 GUID string.
        /// </summary>
        public const string PackageGuidString = "d24cb627-9b37-4ac3-aec3-aba333e88419";

        /// <summary>
        /// Initializes a new instance of the <see cref="AmfToolsPackage"/> class.
        /// </summary>
        public AmfToolsPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        private static IWindowManager windowManager;
        public static IWindowManager WindowManager {
            get
            {
                if(windowManager == null)
                {
                    bootstrapper.Initialize();
                    Tracking.Init();

                    try
                    {
                        windowManager = IoC.Get<IWindowManager>();
                    }
                    catch
                    {
                        windowManager = new WindowManager();
                    }

                    LoadSystemWindowsInteractivity();
                }
                return windowManager;
            }
        }


        private static Bootstrapper bootstrapper = new Bootstrapper();
        #region AsyncPackage Members

        private static Events events;
        private static DocumentEvents documentEvents;


        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            bool isSolutionLoaded = await IsSolutionLoadedAsync();

            if (isSolutionLoaded)
            {
                HandleOpenSolutionAsync();
            }

            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterOpenSolution += HandleOpenSolutionAsync;
        }

        private async Task<bool> IsSolutionLoadedAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;

            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));

            return value is bool isSolOpen && isSolOpen;
        }

        private async void HandleOpenSolutionAsync(object sender = null, EventArgs e = null)
        {
            //AddContractCommand.Initialize(this);
            //AddReferenceCommand.Initialize(this);
            //EditPropertiesCommand.Initialize(this);
            //ExtractRAMLCommand.Initialize(this);

            // Query service asynchronously from the UI thread
            var dte = await GetServiceAsync(typeof(DTE)) as DTE;

            // trigger scaffold when RAML document gets saved
            events = dte.Events;
            documentEvents = events.DocumentEvents;
            documentEvents.DocumentSaved += DocumentEventsOnDocumentSaved;

        }

        #endregion

        private void DocumentEventsOnDocumentSaved(Document document)
        {
            RamlScaffoldServiceBase.TriggerScaffoldOnRamlChanged(document);

            //RamlClientTool.TriggerClientRegeneration(document, GetExtensionPath());
        }

        // workaround http://stackoverflow.com/questions/29362125/visual-studio-extension-could-not-find-a-required-assembly
        private static void LoadSystemWindowsInteractivity()
        {
            // HACK: Force load System.Windows.Interactivity.dll from plugin's 
            // directory
            typeof(System.Windows.Interactivity.Behavior).ToString();
        }

    }
}
