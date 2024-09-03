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
using Rhino.Geometry.Intersect;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_40ea4 : GH_ScriptInstance
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
  private void RunScript(Curve crv, int cnt, int id, ref object A, ref object isoCurve, ref object PerpCurve)
  {
    List<Curve> seg = crv.DuplicateSegments().ToList(); 
    seg.Sort((a, b) => b.GetLength().CompareTo(a.GetLength()));
    Curve longSeg = seg[id % seg.Count];
    List<Point3d> divSeg = DividePts(longSeg, cnt, true);
    
    Surface ps = PlanarSrf(crv);
    Plane pl = SrfPlane(ps); 
    List<Curve> ret = new List<Curve>();
    List<Curve> perpCurve = new List<Curve>(); 
    for (int i = 0; i < divSeg.Count; i++)
    {
      Point3d now = divSeg[i];
      double u, v, t; 
      ps.ClosestPoint(now, out u, out v);
      var tmp = ps.IsoCurve(1, u);
      ret.Add(tmp);
      longSeg.ClosestPoint(now, out t);
      var vec = longSeg.TangentAt(t); 
      vec.Rotate(Math.PI / 2, pl.Normal); 
      perpCurve.Add(new Line(now, vec, crv.GetLength()).ToNurbsCurve());
    }

    A = longSeg; 
    isoCurve = ret;
    PerpCurve = perpCurve; 

  }
  #endregion
  #region Additional

  public List<Curve> SplitWithCurves(Curve crv, List<Curve> cList)
  {
    List<Curve> pieces = new List<Curve>();
    List<double> tList = new List<double>();
    foreach (Curve c in cList)
    {
      var events = Intersection.CurveCurve(crv, c, 0.001, 0.001);
      if (events != null)
      {
        for (int i = 0; i < events.Count; i++)
        {
          var ccx = events[i];
          tList.Add(ccx.ParameterA);
        }
      }
    }

    if (!tList.Any())
    {
      pieces.Add(crv);
      return pieces;
    }

    tList.Sort();
    pieces = crv.Split(tList).ToList();
    if (crv.IsClosed && pieces.Count >= 3)
    {
      Curve first = pieces.First();
      Curve last = pieces.Last();
      first = Curve.JoinCurves(new Curve[2] { first, last })[0];
      pieces = pieces.Skip(1).Take(pieces.Count - 2).ToList();
      pieces.Insert(0, first);
    }

    return pieces;
  }
  public Plane SrfPlane(Surface srf)
  {
    Plane pl = new Plane();
    srf.TryGetPlane(out pl);
    return pl; 
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

  public static List<Point3d> DividePts(Curve crv, int cnt, bool end)
  {
    List<Point3d> ret = new List<Point3d>();
    var crvT = crv.DivideByCount(cnt, end);
    for (int i = 0; i < crvT.Length; i++)
    {
      var pt = crv.PointAt(crvT[i]);
      ret.Add(pt);
    }

    return ret;
  }
  #endregion
}