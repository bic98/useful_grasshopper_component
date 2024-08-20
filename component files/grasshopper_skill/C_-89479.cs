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
public abstract class Script_Instance_89479 : GH_ScriptInstance
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
  private void RunScript(Curve x, Curve y, ref object A, ref object B, ref object C, ref object D)
  {
    
      var pts = Discontinuity(x);
      pts.Sort((u, v) => u.DistanceTo(Point3d.Origin).CompareTo(v.DistanceTo(Point3d.Origin)));
      var now = pts[0];
      Curve crv = MoveOrient(x, now, y.PointAt(0));
      A = crv;
      B = y.PointAt(0);
      var cps = CloPts(pts, now, 1);
      Vector3d n = cps[0] - now;
      C = n;
      D = now.DistanceTo(cps[0]);
  }
  #endregion
  #region Additional

    public T MoveOrient<T > (T obj, Point3d now, Point3d nxt, Plane baseNow = default(Plane), Plane baseNxt = default(Plane)) where T : GeometryBase
    {
      if (baseNow == default(Plane))
      {
        baseNow = Plane.WorldXY;
      }

      if (baseNxt == default(Plane))
      {
        baseNxt = Plane.WorldXY;
      }
      Plane st = new Plane(now, baseNow.XAxis, baseNow.YAxis);
      Plane en = new Plane(nxt, baseNxt.XAxis, baseNxt.YAxis);
      Transform orient = Transform.PlaneToPlane(st, en);
      obj.Transform(orient);
      return obj;
    }

    public Point3d MovePt(Point3d p, Vector3d v, double amp)
    {
      v.Unitize();
      Transform move = Transform.Translation(v * amp);
      p.Transform(move);
      return p;
    }

    public List<Point3d> Discontinuity(Curve x)
    {
      var seg = x.DuplicateSegments();
      List<Point3d> pts = new List<Point3d>();
      for (int i = 0; i < seg.Length; i++)
      {
        if (i == 0) pts.Add(seg[i].PointAtStart);
        pts.Add(seg[i].PointAtEnd);
      }

      if (x.IsClosed) pts.RemoveAt(pts.Count - 1);
      return pts;
    }

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
  #endregion
}