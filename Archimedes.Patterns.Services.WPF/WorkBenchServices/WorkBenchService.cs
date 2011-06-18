﻿

namespace Archimedes.Services.WPF.WorkBenchServices
{

    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;
    using AvalonDock;
    using System.Collections.Generic;
    using Archimedes.Services.WPF.WorkBenchServices.MessageBox;
    using Archimedes.Services.WPF.AvalonDockService;
    using Archimedes.Services.WPF.WindowViewModelMapping;
    using Archimedes.Patterns.Services;
    using Archimedes.Patterns.WPF.ViewModels;
    using Archimedes.Services.WPF.WorkBenchServices.Loader;

    /// <summary>
    /// Implements the workbenchservice backing AvalonDock
    /// </summary>
    public class AvalonWorkBenchService : IWorkBenchService
    {
        #region Fields

        IWindowViewModelMappings _mappingService = ServiceLocator.Instance.Resolve<IWindowViewModelMappings>();
        IAvalonService _avalonService = ServiceLocator.Instance.Resolve<IAvalonService>();
        DockingManager _dockManager = null;
        string _statusBarText = "";
        bool _isbackgroundWorking = false;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the Statusbar Text has changed
        /// </summary>
        public event EventHandler StatusTextChanged;

        /// <summary>
        /// Raised when the BackgroundWorking State has changed
        /// </summary>
        public event EventHandler IsBackgroundWorkingChanged;

        #endregion

        #region Constructor

        public AvalonWorkBenchService() {

            _avalonService.PrimaryDockManagerChanged += OnPrimaryDockManagerChanged;

            if (_avalonService.PrimaryDockManager != null)
                DockManager = _avalonService.PrimaryDockManager;
        }

        #endregion

        #region IWorkBenchService

        #region Show VM Content Methods

        /// <summary>
        /// Show the VM in a dockable floating window
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="sizeToContent"></param>
        /// <param name="windowSize"></param>
        public void ShowDockableFloating(WorkspaceViewModel viewModel, SizeToContent sizeToContent = SizeToContent.WidthAndHeight, Size? windowSize = null) {
            if (viewModel == null)
                throw new ArgumentNullException("viewModel");

            var content = SetupFloatingContent(viewModel, sizeToContent, windowSize);
            content.ShowAsFloatingWindow(DockManager, false);
        }

        /// <summary>
        /// Show the VM in a as standard UNdockable floating window
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="sizeToContent"></param>
        /// <param name="windowSize"></param>
        public void ShowFloating(WorkspaceViewModel viewModel, SizeToContent sizeToContent = SizeToContent.WidthAndHeight, Size? windowSize = null) {
            if (viewModel == null)
                throw new ArgumentNullException("viewModel");

            var content = SetupFloatingContent(viewModel, sizeToContent, windowSize);
            content.ShowAsUndockableFloatingWindow(DockManager);
        }





        /// <summary>
        /// Show the VM as modal Dialog window (blocks current thread)
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="sizeToContent"></param>
        /// <param name="windowSize"></param>
        /// <returns></returns>
        public bool? ShowDialog(WorkspaceViewModel viewModel, SizeToContent sizeToContent = SizeToContent.WidthAndHeight, Size? windowSize = null) {
            if(viewModel == null)
                throw new ArgumentNullException("viewModel");

            var dockableContent = SetUpDockableContent(viewModel, viewModel.DisplayName);
            dockableContent.DockableStyle = DockableStyle.None;
            dockableContent.FloatingWindowSizeToContent = sizeToContent;
            dockableContent.HideOnClose = false;
            dockableContent.Closing += (s, e) => viewModel.OnRequestClose();
            if (windowSize.HasValue) {
                dockableContent.FloatingWindowSize = windowSize.Value;
            }
            var ret = dockableContent.ShowAsDialoge(DockManager);

            return ret;
        }


        public void ShowDockedContent(WorkspaceViewModel viewModel) {
            if(viewModel == null)
                throw new ArgumentNullException("viewModel");
            var dockableContent = SetUpDockableContent(viewModel, viewModel.DisplayName);
            dockableContent.Show(DockManager);
        }

        public void ShowDockedDocument(WorkspaceViewModel viewModel) {
            if(viewModel == null)
                throw new ArgumentNullException("viewModel");
            var dockableContent = SetUpDockableContent(viewModel, viewModel.DisplayName);
            dockableContent.ShowAsDocument(DockManager);
        }

        #endregion

        #region MessageBox 



        /// <summary>
        /// Show a common MessageBox Dialoge with an additional Detail Message Section for large Text amounts
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="detail">The Detail Message which will be stored in a scrollable message presenter</param>
        /// <param name="title">The title of the MessageBox</param>
        /// <param name="type">Type of the Message, which will result in diffrent images displayed to the user</param>
        /// <param name="button"></param>
        /// <returns></returns>
        public DialogWPFResult MessageBox(string message, string detail, string title, MessageBoxType type = MessageBoxType.None, MessageBoxWPFButton button = MessageBoxWPFButton.OK) {
            var vm = new MessageBoxViewModel(message, button);
            vm.DetailMessage = detail;
            vm.MessageBoxImage = type;
            var view = vm.BuildView();

            var dc = CreateDockableContent(title, view, vm);
            dc.DockableStyle = AvalonDock.DockableStyle.Floating;
            dc.FloatingWindowSizeToContent = SizeToContent.WidthAndHeight;
            dc.HideOnClose = false;
            dc.Closing += (s, e) => vm.OnRequestClose();
            dc.ShowAsDialoge(DockManager);

            return vm.DialogeResult;
        }

        /// <summary>
        /// Show a common MessageBox Dialoge
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="title">The title of the MessageBox</param>
        /// <param name="type">Type of the Message, which will result in diffrent images displayed to the user</param>
        /// <param name="button"></param>
        /// <returns></returns>
        public DialogWPFResult MessageBox(string message, string title, MessageBoxType type = MessageBoxType.None, MessageBoxWPFButton button = MessageBoxWPFButton.OK) {
            var vm = new MessageBoxViewModel(message, button);
            vm.MessageBoxImage = type;
            var view = vm.BuildView();

            var dc = CreateDockableContent(title, view, vm);
            dc.DockableStyle = AvalonDock.DockableStyle.Floating;
            dc.FloatingWindowSizeToContent = SizeToContent.WidthAndHeight;
            dc.HideOnClose = false;
            dc.Closing += (s, e) => vm.OnRequestClose();
            dc.ShowAsDialoge(DockManager);

            return vm.DialogeResult;
        }

        #endregion

        
        /// <summary>
        /// Indicates that a background action is currently launched
        /// </summary>
        public bool IsBackgroundWorking {
            get { return _isbackgroundWorking; }
            set {

                if (_isbackgroundWorking != value) {
                    _isbackgroundWorking = value;
                    if (IsBackgroundWorkingChanged != null)
                        IsBackgroundWorkingChanged(this, EventArgs.Empty);
                }
            }
        }

        #region Loader

        DockableContent _dcLoaderView;

        /// <summary>
        /// Shows the loaderView
        /// </summary>
        public void LoaderView(){
            LoaderView(true);
        }

        /// <summary>
        /// Shows/Hides the LoaderView
        /// </summary>
        /// <param name="display">True means show, false hides the Loader</param>
        public void LoaderView(bool display) {

            if (_dcLoaderView == null && display) {
                var loaderView = new LoadingAnimation() { Width = 100, Height = 100 };
                var vm = new LoaderViewModel();
                _dcLoaderView = CreateDockableContent("Bitte Warten", loaderView, vm);
                _dcLoaderView.DockableStyle = DockableStyle.Floating;
                _dcLoaderView.FloatingWindowSizeToContent = SizeToContent.WidthAndHeight;
                //_dcLoaderView.IsCloseable = false;
                _dcLoaderView.HideOnClose = true;
            }

            if (display) {
                _dcLoaderView.ShowAsFloatingWindow(DockManager, false);
            } else {
                if (_dcLoaderView != null)
                    _dcLoaderView.Close();
            }
        }

        #endregion

        /// <summary>
        /// Makes the Mainwindow active
        /// </summary>
        public void ActivateMainWindow() {
            var wnd = Window.GetWindow(DockManager);
            if (wnd != null)
                wnd.Activate();
        }

        /// <summary>
        /// Returns the standard Thread dispatcher of the UI
        /// </summary>
        public Dispatcher STADispatcher {
            get {
                if (DockManager == null)
                    throw new NotSupportedException("Not Ready!");
                return DockManager.Dispatcher;
            }
        }


        public string StatusText {
            get {
                return _statusBarText;
            }
            set {
                _statusBarText = value;
                if (StatusTextChanged != null)
                    StatusTextChanged(this, EventArgs.Empty);
            }
        }

        AvalonDock.DockingManager DockManager {
            get { return _dockManager; }
            set {
                _dockManager = value;
            }
        }

        public IEnumerable<AvalonDock.DockableContent> HiddenContents {
            get {
                return _dockManager.DockableContents.ToList().FindAll(x => x.State == AvalonDock.DockableContentState.Hidden);
            }
        }
        
        #endregion

        #region Helper Methods

        DockableContent SetupFloatingContent(WorkspaceViewModel viewModel, SizeToContent sizeToContent = SizeToContent.WidthAndHeight, Size? windowSize = null) {
            if (viewModel == null)
                throw new ArgumentNullException("viewModel");

            var dockableContent = SetUpDockableContent(viewModel, viewModel.DisplayName);
            dockableContent.DockableStyle = DockableStyle.Floating;
            dockableContent.FloatingWindowSizeToContent = sizeToContent;
            dockableContent.HideOnClose = false;
            dockableContent.Closing += (s, e) => viewModel.OnRequestClose();


            if (windowSize.HasValue) {
                dockableContent.FloatingWindowSize = windowSize.Value;
            }
            return dockableContent;
        }

        DockableContent SetUpDockableContent(WorkspaceViewModel viewModel, string title) {

            var dockableContent = FindContentByViewModel(viewModel) as DockableContent;
            if (dockableContent != null)
                return dockableContent;

            // Create New Content
            var viewType = _mappingService.GetViewTypeFromViewModelType(viewModel.GetType());
            var view = (FrameworkElement)Activator.CreateInstance(viewType);

            dockableContent = CreateDockableContent(title, view, viewModel);
            return dockableContent;
        }

        DockableContent CreateDockableContent(string title, FrameworkElement view, WorkspaceViewModel viewModel) {
            var dockableContent = new DockableContent()
            {
                Name = "myNewDialoge",
                Title = title
            };
            dockableContent.Content = view;
            dockableContent.DataContext = viewModel;
            viewModel.RequestClose += (s, e) => CloseParent(view, viewModel);
            return dockableContent;
        }

        ManagedContent FindContentByViewModel(WorkspaceViewModel vm) {
            ManagedContent content = null;

            var res = from dockcontent in DockManager.DockableContents
                      where dockcontent.Content != null && ReferenceEquals(((FrameworkElement)dockcontent.Content).DataContext, vm)
                      select dockcontent;
            if (res.Any())
                return res.First();

            var docs = from doc in DockManager.Documents
                       where doc.Content != null && ReferenceEquals(((FrameworkElement)doc.Content).DataContext, vm)
                       select doc;
            if(docs.Any())
                return docs.First();

            foreach (var floatingWnd in DockManager.FloatingWindows) {

               if (floatingWnd.Content is FloatingDockablePane) {
                    var floatingPane = floatingWnd.Content as FloatingDockablePane;
                    foreach (ManagedContent managedContent in floatingPane.Items) {
                        if (ReferenceEquals((managedContent.Content as FrameworkElement).DataContext, vm)) {
                            return managedContent;
                        }
                    }
               }
               if (floatingWnd.Content is DocumentPane) {
                   var floatingPane = floatingWnd.Content as DocumentPane;
                   foreach (ManagedContent managedContent in floatingPane.Items) {
                       if (ReferenceEquals((managedContent.Content as FrameworkElement).DataContext, vm)) {
                           return managedContent;
                       }
                   }
               }

            }

            return content;
        }


        void CloseParent(FrameworkElement element, WorkspaceViewModel vm) {
            var window = Window.GetWindow(element);
            if(window != null){
                window.Closed += (a, b) => CleanUp(element,vm, window);
                try {
                    window.Close();
                } catch {
                    //
                }
            }
            ActivateMainWindow();
        }

        void CleanUp(FrameworkElement element, WorkspaceViewModel vm, Window window) {
            vm.RequestClose -= (s, e) => CloseParent(element, vm);
            window.Closed -= (a, b) => CleanUp(element, vm, window);
        }



        //void UpdateHiddenContent(){
        //    if (DockManager != null) {
        //        var list = DockManager.DockableContents.Where(dc => dc.State == DockableContentState.Hidden).ToList();
        //        HiddenContents.Clear();
        //        foreach (var dc in list)
        //            HiddenContents.Add(dc);
        //    }
        //}
        #endregion

        #region Event Handlers

        void OnPrimaryDockManagerChanged(object sender, EventArgs e) {
            DockManager = _avalonService.PrimaryDockManager;
        }

        #endregion
    }
}
