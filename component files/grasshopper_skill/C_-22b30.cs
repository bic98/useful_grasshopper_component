using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_22b30 : GH_ScriptInstance
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
  private void RunScript(List<Curve> crv, int degree, int ptCnt, ref object A, ref object B)
  {
    
      List<Curve> newCrv = new List<Curve>();
      Point3d pts = new Point3d();
      for (int i = 0; i < crv.Count; i++)
      {
        Curve now = crv[i];
        var reCrv = now.Rebuild(ptCnt, degree, true);
        newCrv.Add(reCrv);
        pts += CurveCenter(reCrv);
      }

      pts /= crv.Count;

      newCrv.Sort((x, y) =>
        {
        Point3d pointA = CurveCenter(x);
        Point3d pointB = CurveCenter(y);
        Point3d referencePoint = pts;
        Vector3d vecA = pointA - referencePoint;
        Vector3d vecB = pointB - referencePoint;
        var crossProduct = Vector3d.CrossProduct(vecA, vecB).Z;
        if (crossProduct > 0)
          return -1;
        else if (crossProduct < 0)
          return 1;
        else
        {
          double distA = vecA.Length;
          double distB = vecB.Length;
          return distA.CompareTo(distB);
        }
        });

      A = newCrv;
      B = pts;
  }
  #endregion
  #region Additional

    public Point3d CurveCenter(Curve c)
    {
      var arr = c.DivideByCount(2, true);
      var ans = c.PointAt(arr[1]);
      return ans;
    }

  
  #endregion
}