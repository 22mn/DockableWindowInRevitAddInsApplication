using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.Attributes;

namespace RevitAddInsWPFSample
{
    // external application class
    public class MainClass : IExternalApplication
    {
        // execute when application open
        public Result OnStartup(UIControlledApplication application)
        {
            // create a ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(Tab.AddIns, "TwentyTwo Sample");
            // assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            // assembly path
            string assemblyPath = assembly.Location;

            // Create Register Button
            PushButton registerButton = ribbonPanel.AddItem(new PushButtonData("Register Window", "Register",
                assemblyPath, "RevitAddInsWPFSample.Register")) as PushButton;
            // accessibility check for register Button
            registerButton.AvailabilityClassName = "RevitAddInsWPFSample.CommandAvailability";
            // btn tooltip 
            registerButton.ToolTip = "Register dockable window at the zero document state.";
            // register button icon images
            registerButton.LargeImage = GetResourceImage(assembly, "RevitAddInsWPFSample.Resources.register32.png");
            registerButton.Image = GetResourceImage(assembly, "RevitAddInsWPFSample.Resources.register16.png");

            // Create Show Button
            PushButton showButton = ribbonPanel.AddItem(new PushButtonData("Show Window", "Show", assemblyPath,
                "RevitAddInsWPFSample.Show")) as PushButton;
            // btn tooltip
            showButton.ToolTip = "Show the registered dockable window.";
            // show button icon images
            showButton.LargeImage = GetResourceImage(assembly, "RevitAddInsWPFSample.Resources.show32.png");
            showButton.Image = GetResourceImage(assembly, "RevitAddInsWPFSample.Resources.show16.png");

            // return status
            return Result.Succeeded;
        }

        // execute when application close
        public Result OnShutdown(UIControlledApplication application)
        {
            // return status
            return Result.Succeeded;

        }

        // get embedded images from assembly resources
        public ImageSource GetResourceImage(Assembly assembly, string imageName)
        {
            try
            {
                // bitmap stream to construct bitmap frame
                Stream resource = assembly.GetManifestResourceStream(imageName);
                // return image data
                return BitmapFrame.Create(resource);
            }
            catch
            {
                return null;
            }
        }

    }
    
    // external command availability
    public class CommandAvailability : IExternalCommandAvailability
    {
        // interface member method
        public bool IsCommandAvailable(UIApplication app, CategorySet cate)
        {
            // zero doc state
            if (app.ActiveUIDocument == null)
            {
                // disable register btn
                return true;
            }
            // enable register btn
            return false;
        }
    }

    // external command class
    [Transaction(TransactionMode.Manual)]
    public class Register : IExternalCommand
    {
        Viewer dockableWindow = null;
        ExternalCommandData edata = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            // dockable window
            Viewer dock = new Viewer();
            dockableWindow = dock;
            edata = commandData;

            // create a new dockable pane id
            DockablePaneId id = new DockablePaneId(new Guid("{68D44FAC-CF09-46B2-9544-D5A3F809373C}"));
            try
            {
                // register dockable pane
                commandData.Application.RegisterDockablePane(id, "TwentyTwo Dockable Sample",
                        dockableWindow as IDockablePaneProvider);
                TaskDialog.Show("Info Message", "Dockable window have registered!");
                // subscribe document opened event
                commandData.Application.Application.DocumentOpened += new EventHandler<Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(Application_DocumentOpened);
                // subscribe view activated event
                commandData.Application.ViewActivated += new EventHandler<ViewActivatedEventArgs>(Application_ViewActivated);
            }
            catch(Exception ex)
            {
                // show error info dialog
                TaskDialog.Show("Info Message", ex.Message);
            }
          
            // return result
            return Result.Succeeded;
        }

        // view activated event
        public void Application_ViewActivated(object sender, ViewActivatedEventArgs e)
        {
            // provide ExternalCommandData object to dockable page
            dockableWindow.CustomInitiator(edata);

        }
        // document opened event
        private void Application_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            // provide ExternalCommandData object to dockable page
            dockableWindow.CustomInitiator(edata);
        }

    }

    // external command class
    [Transaction(TransactionMode.Manual)]
    public class Show : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // dockable window id
                DockablePaneId id = new DockablePaneId(new Guid("{68D44FAC-CF09-46B2-9544-D5A3F809373C}"));
                DockablePane dockableWindow = commandData.Application.GetDockablePane(id);
                dockableWindow.Show();
            }
            catch(Exception ex)
            {
                // show error info dialog
                TaskDialog.Show("Info Message", ex.Message);
            }
            // return result
            return Result.Succeeded;
        }
    }
}
