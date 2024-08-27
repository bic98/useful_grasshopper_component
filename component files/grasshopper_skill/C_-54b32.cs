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
public abstract class Script_Instance_54b32 : GH_ScriptInstance
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
  private void RunScript(List<Point3d> pts, int near, ref object A)
  {

    List<List<(double a, Point3d p)>> adj = new List<List<(double a, Point3d p)>>();  
    for (int i = 0; i < pts.Count; i++)
    {
      var nxtPts = CloPts(pts, pts[i], near);
      List < (double a, Point3d p)> tmp = new List<(double a, Point3d p) > ();
      for (int j = 0; j < nxtPts.Count; j++)
      {
        double dist = pts[i].DistanceTo(nxtPts[j]);
        tmp.Add((dist, nxtPts[j]));
      }

      adj[i] = tmp;
    }


    A = adj;
  }
  #endregion
  #region Additional

  public List<Point3d> CloPts(List < Point3d > pts, Point3d now, int cnt)
  {
    pts.Sort((u, v) => u.DistanceTo(now).CompareTo(v.DistanceTo(now)));
    List<Point3d> ans = new List<Point3d>();

    for (int i = 0; i < pts.Count; i++)
    {
      if (ans.Count == cnt) break;
      if (pts[i] == now) continue;
      ans.Add(pts[i]);
    }

    return ans;
  }

  public List<Curve> Shatter(Curve crv, int cnt)
  {
    List<Curve> ret = new List<Curve>();
    var crvT = crv.DivideByCount(cnt, true);
    for (int i = 0; i < crvT.Length - 1; i++)
    {
      ret.Add(crv.Trim(crvT[i], crvT[i + 1]));
    }

    return ret;
  }
  #endregion
}