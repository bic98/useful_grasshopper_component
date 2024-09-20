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
public abstract class Script_Instance_29c49 : GH_ScriptInstance
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
  private void RunScript(List<Curve> crv, double dist, ref object A, ref object B)
  {
    List<List<Curve>> outLinesList = new List<List<Curve>>();
    List<Curve> cir = new List<Curve>();
    var joinCrv = Curve.JoinCurves(crv); 
    foreach (var c in joinCrv)
    {
      List<Curve> tmp = new List<Curve>(); 
      var offset1 = OffsetCurve(c, Plane.WorldXY, dist);
      var offset2 = OffsetCurve(c, Plane.WorldXY, -dist);
      tmp.Add(offset1);
      tmp.Add(offset2); 
      var enPt = c.PointAtEnd; 
      var stPt = c.PointAtStart;
      var enCrv = PointsInCurves(crv, enPt);
      var stCrv = PointsInCurves(crv, stPt);
      if (enCrv.Count == 1)
      {
        Curve inCir = new Circle(enPt, dist / 2).ToNurbsCurve();
        Curve arc = new Arc(offset1.PointAtEnd, offset1.TangentAtEnd, offset2.PointAtEnd).ToNurbsCurve();
        cir.Add(inCir); 
        tmp.Add(arc); 
      }
      else
      {
        tmp.Add(new Line(offset1.PointAtEnd, offset2.PointAtEnd).ToNurbsCurve()); 
      }
      if (stCrv.Count == 1)
      {
        Curve arc = new Arc(offset1.PointAtStart, -offset1.TangentAtStart, offset2.PointAtStart).ToNurbsCurve();
        Curve inCir = new Circle(stPt, dist / 2).ToNurbsCurve();
        cir.Add(inCir);
        tmp.Add(arc); 
      }
      else
      {
        tmp.Add(new Line(offset1.PointAtStart, offset2.PointAtStart).ToNurbsCurve());
      }

      var joinTmp = Curve.JoinCurves(tmp).ToList();
      outLinesList.Add(joinTmp); 
    }

    A = MakeDataTree2D(outLinesList);
    B = cir; 
  }
  #endregion
  #region Additional

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
  public Curve OffsetCurve(Curve c, Plane p, double interval)
  {
    var seg = c.DuplicateSegments();
    var joinseg = Curve.JoinCurves(seg);
    List<Curve> outLines = new List<Curve>();
    for (int i = 0; i < joinseg.Length; i++)
    {
      outLines.AddRange(c.Offset(p, interval, 0, CurveOffsetCornerStyle.Sharp));
    }

    var ret = Curve.JoinCurves(outLines);
    return ret[0];
  }
  #endregion
}