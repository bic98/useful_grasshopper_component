using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Runtime;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_eaeee : GH_ScriptInstance
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
  private void RunScript(List<Curve> edges, Point3d pt, ref object A, ref object B, ref object C)
  {
    var crves = PointsInCurves(edges, pt);
    List<Vector3d> v = new List<Vector3d>();
    Vector3d normal = new Vector3d();
    List<Point3d> en = new List<Point3d>();
    for (int i = 0; i < crves.Count; i++)
    {
      var b = CurveCenter(crves[i]);
      Vector3d tmp = b - pt;
      tmp.Unitize();
      en.Add(MovePt(pt, tmp, 15));
      normal += tmp;
      v.Add(tmp);
    }

    Surface srf = NurbsSurface.CreateFromCorners(en[0], en[1], en[2]);
    C = srf;
    A = normal;
    B = v;
  }
  #endregion
  #region Additional

  public Point3d MovePt(Point3d p, Vector3d v, double amp)
  {
    v.Unitize();
    Transform move = Transform.Translation(v * amp);
    p.Transform(move);
    return p;
  }
  public Point3d CurveCenter(Curve c)
  {
    var arr = c.DivideByCount(2, true);
    var ans = c.PointAt(arr[1]);
    return ans;
  }
  public List<Curve> PointsInCurves(List<Curve> crves, Point3d pt)
  {
    List<Curve> ans = new List<Curve>();
    for (int i = 0; i < crves.Count; i++)
    {
      double t;
      if (crves[i].ClosestPoint(pt, out t, 0.001))
      {
        ans.Add(crves[i]);
      }
    }

    return ans;
  }
  #endregion
}