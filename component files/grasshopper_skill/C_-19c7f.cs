using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Linq;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_19c7f : GH_ScriptInstance
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
  private void RunScript(double size, double interval, int count, double rot, ref object A)
  {
    List<double> li = Enumerable.Repeat(0.0, count).ToList();
    li[0] = size; 
    for (int i = 1; i < li.Count; i++)
    {
      li[i] = li[i - 1] + interval;
    }

    var rotAngle = RangeInterval(0, rot, count); 
    List<Curve> crv = new List<Curve>();
    for (int i = 0; i < li.Count; i++)
    {
      double r = li[i];
      var cir = new Circle(Plane.WorldYZ, r).ToNurbsCurve(); 
      cir.Rotate(rotAngle[i], Vector3d.ZAxis, Plane.WorldXY.Origin); 
      crv.Add(cir); 
    }

    A = crv; 
  }
  #endregion
  #region Additional

  public static List<double> RangeInterval(double x, double y, int cnt)
  {
    List<double> amp = new List<double>();
    if (cnt == 1) return amp;
    double gap = (y - x) / (cnt - 1);
    for (int i = 0; i < cnt; i++)
    {
      amp.Add(x + gap * i);
    }

    return amp;
  }
  #endregion
}