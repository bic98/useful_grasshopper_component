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
using Plane = Grasshopper.Kernel.Geometry.Plane;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_5a31a : GH_ScriptInstance
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
  private void RunScript(Rectangle3d rectangle, double yDir, double xDir, int cnt, List<double> shape, double depth, double interval, ref object A, ref object B)
  {
    Curve crv = rectangle.ToNurbsCurve();
    Surface srf = PlanarSrf(crv);
    srf = IntervalTrim(xDir, yDir, srf);
    var range = RangeInterval(0, 1, cnt);
    List<Curve> crves = new List<Curve>();
    for (int i = 0; i < cnt; i++)
    {
      var isoC = IsoCurveU(srf, range[i]);
      isoC.Rotate(shape[i], Vector3d.XAxis, CurveCenter(isoC));
      crves.Add(isoC);
    }

    List<Brep> ret = new List<Brep>();
    for (int i = 0; i < cnt; i++)
    {
      var now = crves[i];
      var osNow = now.Offset(Rhino.Geometry.Plane.WorldYZ, depth, 0.01, 0);
      var nurbSrf = NurbsSurface.CreateRuledSurface(now, osNow[0]).ToBrep().Faces[0];
      var extSrf = Brep.CreateFromOffsetFace(nurbSrf, interval, 0.01, true, true); 
      ret.Add(extSrf);
    }
    B = ret;  
    A = crves; 
  }
  #endregion
  #region Additional

  public Point3d CurveCenter(Curve c)
  {
    var arr = c.DivideByCount(2, true);
    var ans = c.PointAt(arr[1]);
    return ans;
  }
  public Surface PlanarSrf(Curve c)
  {
    string log;
    Surface ret = null;
    if (c.IsValidWithLog(out log) && c.IsPlanar() && c.IsClosed)
    {
      var tmp = Brep.CreatePlanarBreps(c, 0.01);
      ret = tmp[0].Faces[0];
      return ret;
    }
    return ret;
  }

  public Curve IsoCurveU(Surface srf, double uDir)
  {
    var ret = srf.IsoCurve(1, srf.Domain(0).T0 + uDir * (srf.Domain(0).T1 - srf.Domain(0).T0));
    return ret;
  }

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
  
  public Surface IntervalTrim(double xDir, double yDir, Surface srf)
  {
    return srf.Trim(new Interval(srf.Domain(0).T1 * xDir, srf.Domain(0).T1),
      new Interval(srf.Domain(1).T1 * yDir, srf.Domain(1).T1));
  }
  #endregion
}