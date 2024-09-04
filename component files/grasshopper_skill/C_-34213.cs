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
public abstract class Script_Instance_34213 : GH_ScriptInstance
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
  private void RunScript(int sz, int cnt, double x, double y, ref object A, ref object B)
  {
    Curve cir = new Circle(Point3d.Origin, sz).ToNurbsCurve();
    List<Plane> pl = HorizontalPlanes(cir, cnt * 2, true);
    List<Point3d> plOrigin = new List<Point3d>();
    

    var interval = RangeInterval(x, y, (cnt + 1) / 2);
    var reverseInterval = new List<double>(interval);
    reverseInterval.Reverse();
    if((cnt & 1) == 1) interval.RemoveAt(interval.Count  - 1);
    interval.AddRange(reverseInterval); 
    
    
    for (int i = 0; i < pl.Count; i++)
    {
      var now = pl[i].Origin;
      if ((i & 1) == 1) 
      {
        var nxt = MovePt(now, pl[i].YAxis, interval[i / 2]);
        now = nxt;
      }
      plOrigin.Add(now);
    }

    
    
    B = interval; 
    A = plOrigin;
    
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
  #endregion
}