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
public abstract class Script_Instance_a8cf6 : GH_ScriptInstance
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
  private void RunScript(List<Curve> obj, List<Point3d> crv, List<Point3d> unit, int id, Brep U, ref object A, ref object B, ref object C, ref object D)
  {
    List<Plane> p = new List<Plane>();
    for (int i = 0; i < crv.Count; i++)
    {
      var poc = PointsInCurves(obj, crv[i]);
      Vector3d normal = new Vector3d();
      for (int j = 0; j < poc.Count; j++)
      {
        var mid = CurveCenter(poc[j]);
        var tmpV = new Vector3d(mid - crv[i]);
        tmpV.Unitize();
        normal += tmpV; 
      }
      Plane pl = new Plane(crv[i], -normal, new Vector3d(0, 0, -normal.Z)); 
      p.Add(pl); 
    }

    var unitP = CloPts(unit, unit[id], 1);
    Vector3d unitNormal = new Vector3d(unitP[0] - unit[id]); 
    Plane unitPl = new Plane(unit[id], unitNormal, new Vector3d(0, 0, unitNormal.Z));
    List<Brep> ans = new List<Brep>(); 
    for (int i = 0; i < p.Count; i++)
    {
      Brep dU = U.DuplicateBrep();
      Brep oU = MoveOrientPlane(dU,  unitPl, p[i]);
      ans.Add(oU); 
    }

    A = ans; 
    B = unit[id];
    C = unitPl;
    D = p; 
  }
  #endregion
  #region Additional

  public T MoveOrientPoint<T>(T obj, Point3d now, Point3d nxt) where T : GeometryBase
  {
    Plane baseNow = Plane.WorldXY; 
    Plane st = new Plane(now, baseNow.XAxis, baseNow.YAxis);
    Plane en = new Plane(nxt, baseNow.XAxis, baseNow.YAxis);
    Transform orient = Transform.PlaneToPlane(st, en);
    obj.Transform(orient);
    return obj;
  }

  public T MoveOrientPlane<T>(T obj, Plane baseNow, Plane baseNxt) where T : GeometryBase
  {
    Transform orient = Transform.PlaneToPlane(baseNow, baseNxt);
    obj.Transform(orient);
    return obj;
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
  
  public List<Point3d> CloPts(List<Point3d> pts, Point3d now, int cnt)
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
  #endregion
}