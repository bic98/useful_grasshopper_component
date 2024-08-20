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
public abstract class Script_Instance_5eefb : GH_ScriptInstance
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
  private void RunScript(Curve x, int y, Interval z, List<Plane> u, ref object A, ref object B)
  {
    var li = HorizontalPlanes(x, y, true);
    var t = x.DivideByCount(y, true);
    var amp = RangeInterval(z.T0, z.T1, li.Count);

    var k = PerpPlanes(x, y, true); 
    List<Curve> ret = new List<Curve>();
    List<Point3d> L = new List<Point3d>();
    List<Point3d> R = new List<Point3d>(); 
    for (int i = 0; i < li.Count; i++)
    {
      Plane p = li[i];
      var v = p.YAxis;
      var tmp = MovePt(p.Origin, v, amp[i]);
      L.Add(tmp); 
      tmp = MovePt(p.Origin, v, -amp[i]);
      R.Add(tmp); 
    }
    ret.Add(Curve.CreateInterpolatedCurve(L, 3));
    ret.Add(Curve.CreateInterpolatedCurve(R, 3));
    ret.Add(new Arc(R[0], -x.TangentAt(t[0]), L[0]).ToNurbsCurve());
    ret.Add(new Arc(R[R.Count - 1], x.TangentAt(t[t.Length - 1]), L[L.Count - 1]).ToNurbsCurve()); 
    A = ret;
    B = k; 
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

  public static List<Plane> HorizontalPlanes(Curve x, int cnt, bool end)
  {
    var t = x.DivideByCount(cnt, end);
    List<Plane> li = new List<Plane>();
    for (int i = 0; i < t.Length; i++)
    {
      var tan = x.TangentAt(t[i]);
      Plane p = new Plane(x.PointAt(t[i]), tan, Vector3d.CrossProduct(Vector3d.ZAxis, tan));
      li.Add(p);
    }

    return li;
  }

  public static List<Plane> PerpPlanes(Curve x, int cnt, bool end)
  {
    var t = x.DivideByCount(cnt, end);
    List<Plane> li = new List<Plane>();
    for (int i = 0; i < t.Length; i++)
    {
      var tan = x.TangentAt(t[i]);
      Plane p = new Plane(x.PointAt(t[i]), tan);
      li.Add(p);
    }

    return li;
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
  public Point3d MovePt(Point3d p, Vector3d v, double amp)
  {
    v.Unitize();
    Vector3d newV = Vector3d.Multiply(v, amp);
    Point3d ans = Point3d.Add(p, newV);
    return ans;
  }
  #endregion
}