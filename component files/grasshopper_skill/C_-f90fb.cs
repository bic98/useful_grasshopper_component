using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Data;
using System.Linq;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_f90fb : GH_ScriptInstance
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
  private void RunScript(List<Curve> x, int y, ref object A, ref object B)
  {
    List<List<Point3d>> pdPts = new List<List<Point3d>>();
    List<Point3d> stPts = new List<Point3d>();
    for(int i = 0; i < x.Count; i++)
    {
      var tmp = PureDiscontinuity(x[i]); 
      pdPts.Add(PureDiscontinuity(x[i])); 
    }

    A = MakeDataTree2D(pdPts); 
  }
  #endregion
  #region Additional

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
      double dp = Math.Abs(nowV * nxtV);
      if (i == 0)
      {
        var prevC = seg[seg.Length - 1];
        var prevV = prevC.PointAtEnd - prevC.PointAtStart;
        prevV.Unitize(); 
        if(Math.Abs(prevV * nowV) < 0.99) pts.Add(nowC.PointAtStart);
      }
      if (dp < 0.99)
      {
        pts.Add(nowC.PointAtEnd);
      }

      Print(dp.ToString()); 
    }
    if (!x.IsClosed) pts.Add(seg[seg.Length - 1].PointAtEnd);
    return pts;
  }
  #endregion
}