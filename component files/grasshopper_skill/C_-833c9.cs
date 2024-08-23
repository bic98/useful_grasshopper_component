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
public abstract class Script_Instance_833c9 : GH_ScriptInstance
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
  private void RunScript(List<Surface> x, int y, ref object A, ref object B)
  {
    List<Curve> contour = new List<Curve>();
    for (int i = 0; i < x.Count; i++)
    {
      var crv = x[i].ToBrep().DuplicateEdgeCurves();
      contour.Add(crv[2]);
      if (i == x.Count - 1) contour.Add(crv[1]); 
    }

    List<Curve> vertical = new List<Curve>(); 
    Curve st = contour[0];
    var pts = DividePts(st, y, true); 
    
    for (int i = 0; i < contour.Count - 1; i++)
    {
      var now = contour[i];
      var nxt = contour[i + 1];
      List<Point3d> nxtPts = new List<Point3d>(); 
      for (int j = 0; j < pts.Count; j++)
      {
        double t;
        nxt.ClosestPoint(pts[j], out t);
        var nxtPt = nxt.PointAt(t);
        nxtPts.Add(nxtPt);
        if (i % 2 == 0)
        {
          if(j % 2 == 0) vertical.Add(new Line(pts[j], nxtPt).ToNurbsCurve());
        }
        else
        {
          if (j % 2 == 1) vertical.Add(new Line(pts[j], nxtPt).ToNurbsCurve()); 
        }
      }

      pts = nxtPts; 


    }

    B = vertical; 
    A = contour; 
  }
  #endregion
  #region Additional
  
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