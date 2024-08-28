using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Runtime.InteropServices;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_325dc : GH_ScriptInstance
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
  private void RunScript(DataTree<Curve> x, double y, double z, double u, ref object A)
  {
    var li = ConvertTreeToNestedList(x);
    List<List<Curve>> ret = new List<List<Curve>>(); 
    for (int i = 0; i < li.Count; i++)
    {
      var now = li[i][0];
      var nxt = li[i][1];
      var nowP = Discontinuity(now);
      var nxtP = Discontinuity(nxt);
      nxtP.Reverse(); 
      List<Point3d> inlinePts = new List<Point3d>();
      for (int j = 0; j < nowP.Count; j++)
      {
        Curve tmp = new Line(nowP[j], nxtP[j]).ToNurbsCurve();
        Point3d k = tmp.PointAt(y * tmp.Domain.T1);
        inlinePts.Add(k); 
      }
      var mid = Curve.CreateControlPointCurve(inlinePts, 1);
      var centerPointMid = MovePt(CurveCenter(mid), Vector3d.ZAxis, z);
      mid = MoveOrientPoint(mid, CurveCenter(mid), centerPointMid);
      var dnNow = MoveOrientPoint(now, CurveCenter(now), MovePt(CurveCenter(now), Vector3d.ZAxis, -u));
      var dnNxt = MoveOrientPoint(nxt, CurveCenter(nxt), MovePt(CurveCenter(nxt), Vector3d.ZAxis, -u));
      List<Curve> threeCurve = new List<Curve> { dnNow, now, mid, nxt, dnNxt }; 
      ret.Add(threeCurve); 
    }
    A = MakeDataTree2D(ret); 
  }
  #endregion
  #region Additional

  public T MoveOrientPoint<T>(T obj, Point3d now, Point3d nxt) where T : GeometryBase
  {
    T copy = (T) obj.Duplicate();
    Plane baseNow = Plane.WorldXY;
    Plane st = new Plane(now, baseNow.XAxis, baseNow.YAxis);
    Plane en = new Plane(nxt, baseNow.XAxis, baseNow.YAxis);
    Transform orient = Transform.PlaneToPlane(st, en);
    copy.Transform(orient);
    return copy;
  }
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
  public List<List<T>> ConvertTreeToNestedList<T>(DataTree<T> tree)
  {
    List<List<T>> nestedList = new List<List<T>>();
    foreach (GH_Path path in tree.Paths)
    {
      List<T> subList = new List<T>(tree.Branch(path));
      nestedList.Add(subList);
    }
    return nestedList;;
  }
  #endregion
}