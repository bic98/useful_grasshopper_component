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
using Rhino.UI;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_1371a : GH_ScriptInstance
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
  private void RunScript(List<Point3d> pts, int id, List<Curve> line, double d, ref object A)
  {

    List<List<Curve>> ans = new List<List<Curve>>();
    List<bool> vis = Enumerable.Repeat(false, line.Count + 1).ToList();
    var convex = ConvexHull2D(pts);
    convex.Add(convex[0]);
    var interPolate = Curve.CreateInterpolatedCurve(convex, 1);
    var purePts = PureDiscontinuity(interPolate);
    var pureCrv = Curve.CreateInterpolatedCurve(purePts, 1).DuplicateSegments().ToList();
    pureCrv = pureCrv.Where(x => x.GetLength() > 5.0).ToList();
    List<Curve> crv = new List<Curve> { pureCrv[id % pureCrv.Count] };
    
    for (int i = 0; i < 100; i++)
    {
      if (crv.Count == 0) break;
      var reference = Curve.JoinCurves(crv)[0];
      List<Curve> nxtCrv = new List<Curve>();
      for (int j = 0; j < line.Count; j++)
      {
        if (vis[j]) continue;
        double t;
        var now = CurveCenter(line[j]);
        reference.ClosestPoint(now, out t);
        var nxt = reference.PointAt(t);
        if (nxt.DistanceTo(now) < d)
        {
          vis[j] = true;
          nxtCrv.Add(line[j]);
        }
      }

      crv = nxtCrv; 
      ans.Add(nxtCrv);
    }
    A = MakeDataTree2D(ans);
  }
  #endregion
  #region Additional
  
  
public Point3d CurveCenter(Curve c)
{
  var arr = c.DivideByCount(2, true);
  var ans = c.PointAt(arr[1]);
  return ans;
}

public List<Point3d> PureDiscontinuity(Curve x)
{
  var seg = x.DuplicateSegments();
  List<Point3d> pts = new List<Point3d>();
  for (int i = 0; i < seg.Length - 1; i++)
  {
    var nowC = seg[i];
    var nxtC = seg[i + 1];
    var nowV = nowC.PointAtEnd - nowC.PointAtStart;
    var nxtV = nxtC.PointAtEnd - nxtC.PointAtStart;
    nowV.Unitize();
    nxtV.Unitize();
    double dp = Math.Abs(Vector3d.Multiply(nowV, nxtV));
    if (i == 0)
    {
      var prevC = seg[seg.Length - 1];
      var prevV = prevC.PointAtEnd - prevC.PointAtStart;
      prevV.Unitize();
      if (Math.Abs(Vector3d.Multiply(prevV, nowV)) < 0.99) pts.Add(nowC.PointAtStart);
    }

    if (dp < 0.99)
    {
      pts.Add(nowC.PointAtEnd);
    }
  }

  if (!x.IsClosed) pts.Add(seg[seg.Length - 1].PointAtEnd);
  return pts;
}

public List<Point3d> ConvexHull2D(List < Point3d > pts)
{
  if (pts.Count <= 1)
  {
    return pts;
  }

  pts.Sort((a, b) => a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

  List<Point3d> hull = new List<Point3d>(pts.Count + 1);
  int s = 0, t = 0;
  // Build lower hull
  foreach (var p in pts)
  {
    while (t >= s + 2 && CrossProduct(hull[t - 2], hull[t - 1], p) <= 0)
      t--;
    if (hull.Count > t)
      hull[t] = p;
    else
      hull.Add(p);
    t++;
  }

  s = --t;
  pts.Reverse();
  // Build upper hull
  foreach (var p in pts)
  {
    while (t >= s + 2 && CrossProduct(hull[t - 2], hull[t - 1], p) <= 0)
      t--;
    if (hull.Count > t)
      hull[t] = p;
    else
      hull.Add(p);
    t++;
  }

  return hull.GetRange(0, t - 1 - (t == 2 && hull[0].Equals(hull[1]) ? 1 : 0));
}

    // Function to compute the cross product of vectors (b - a) and (c - a)
private double CrossProduct(Point3d a, Point3d b, Point3d c)
{
  return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
}

public static DataTree<T> MakeDataTree2D<T>(List<List<T>> ret)
{
  DataTree<T> tree = new DataTree<T>();
  for (int i = 0; i < ret.Count; i++)
  {
    GH_Path path = new GH_Path(i);
    for (int j = 0; j < ret[i].Count; j++)
    {

      tree.Add(ret[i][j], path);
    }
  }

  return tree;
}
  #endregion
}