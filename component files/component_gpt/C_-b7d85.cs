using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Drawing;
using System.IO;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_b7d85 : GH_ScriptInstance
{
  #region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { /* Implementation hidden. */ }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { /* Implementation hidden. */ }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { /* Implementation hidden. */ }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
  #endregion

  #region Members
  /// <summary>Gets the current Rhino document.</summary>
  private readonly RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private readonly GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private readonly IGH_Component Component;
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private readonly int Iteration;
  #endregion
  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments,
  /// Output parameters as ref arguments. You don't have to assign output parameters,
  /// they will have a default value.
  /// </summary>
  #region Runscript
  private void RunScript(object button, string isometric, ref object viewportCapture)
  {
    viewportCapture = CaptureViewportWithAllObjectsInIsometric(isometric);
  }
  #endregion
  #region Additional

    public static Bitmap CaptureViewportWithAllObjectsInIsometric(string isometricView)
    {
      string tempPath = Path.Combine(Path.GetTempPath(), "temp_vp" + DateTime.Now.Ticks + ".png");

      // Get the active Rhino document's active view
      var view = RhinoDoc.ActiveDoc.Views.ActiveView;

      // Ensure there's an active view to work with
      if (view == null)
      {
        RhinoApp.WriteLine("No active view found.");
        return null;
      }

      // Set the view to parallel projection
      view.ActiveViewport.ChangeToParallelProjection(true);

      // Set the view to isometric NE, NW, SE, or SW
      if (isometricView.Equals("NE", StringComparison.OrdinalIgnoreCase))
      {
        view.ActiveViewport.SetCameraLocation(new Point3d(1, -1, 1), true);
        view.ActiveViewport.SetCameraDirection(new Vector3d(-1, 1, -1), true);
      }
      else if (isometricView.Equals("NW", StringComparison.OrdinalIgnoreCase))
      {
        view.ActiveViewport.SetCameraLocation(new Point3d(-1, -1, 1), true);
        view.ActiveViewport.SetCameraDirection(new Vector3d(1, 1, -1), true);
      }
      else if (isometricView.Equals("SE", StringComparison.OrdinalIgnoreCase))
      {
        view.ActiveViewport.SetCameraLocation(new Point3d(1, 1, 1), true);
        view.ActiveViewport.SetCameraDirection(new Vector3d(-1, -1, -1), true);
      }
      else if (isometricView.Equals("SW", StringComparison.OrdinalIgnoreCase))
      {
        view.ActiveViewport.SetCameraLocation(new Point3d(-1, 1, 1), true);
        view.ActiveViewport.SetCameraDirection(new Vector3d(1, -1, -1), true);
      }
      
      // Select all objects
      var allObjects = RhinoDoc.ActiveDoc.Objects.FindByObjectType(ObjectType.AnyObject);
      foreach (var obj in allObjects)
      {
        obj.Select(true);
      }

      // Zoom to selected objects
      RhinoApp.RunScript("_Zoom _Selected", false);
      
      foreach (var obj in allObjects)
      {
        obj.Select(false);
      }
      
      // Set the view to rendered mode (if not already)
      view.ActiveViewport.DisplayMode = DisplayModeDescription.FindByName("Rendered");
      RhinoDoc.ActiveDoc.Views.Redraw();

      // Capture the viewport to the file
      var size = view.Size;
      var bitmap = view.CaptureToBitmap(new Size(size.Width, size.Height));
      bitmap.Save(tempPath);

      // Load the image from the file into a System.Drawing.Bitmap
      var loadedBitmap = new Bitmap(tempPath);

      // Optionally, clean up the temporary file
      // File.Delete(tempPath);

      return loadedBitmap;
    }
  #endregion
}