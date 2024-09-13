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
public abstract class Script_Instance_84730 : GH_ScriptInstance
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
  private void RunScript(Surface x, int y, ref object A, ref object B, ref object C)
  {
    var edges = x.ToBrep().Edges.ToList();
    var pts = x.ToBrep().Vertices.ToList();
    var minZPoint = pts.OrderBy(p => p.Location.Z).FirstOrDefault();
    Point3d boxStPt = new Point3d(minZPoint.Location.X, minZPoint.Location.Y, minZPoint.Location.Z - y);
    Plane pl = Plane.WorldXY;
    pl.Origin = boxStPt;
    List<Brep> breps = new List<Brep>() { x.ToBrep() };
    List<Curve> curves = new List<Curve>();
    foreach (var i in edges)
    {
      Curve upCrv = i.DuplicateCurve(); 
      Curve dnCrv = Curve.ProjectToPlane(upCrv, pl);
      var sideSrf = NurbsSurface.CreateRuledSurface(upCrv, dnCrv);
      breps.Add(sideSrf.ToBrep());
      curves.Add(dnCrv);
    }

    breps.Add(PlanarSrf(Curve.JoinCurves(curves)[0]));
    var union = Brep.JoinBreps(breps, 0.01);

    if (union != null && union.Length > 0)
    {
      A = union[0]; 
    }
  }
  #endregion
  #region Additional

  public Brep PlanarSrf(Curve c)
  {
    string log;
    Brep ret = null;
    if (c.IsValidWithLog(out log) && c.IsPlanar() && c.IsClosed)
    {
      var tmp = Brep.CreatePlanarBreps(c, 0.01);
      ret = tmp[0];
      return ret;
    }
    return ret;
  }
  #endregion
}