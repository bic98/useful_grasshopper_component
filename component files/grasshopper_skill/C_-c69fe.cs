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
using Rhino.Geometry.Collections;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_c69fe : GH_ScriptInstance
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
  private void RunScript(Mesh x, int y, int z, ref object A)
  {
    var pts = MeshToPoints(x);
    BrepFace srf = CreatePatchPoints(pts, y, y); 
    var outline = MeshOutlineToCurve(x);  
    Curve[] pulled = Curve.PullToBrepFace(outline, srf, 0.001);
    var splitBreps = srf.Split(pulled, 0.001).Faces;
    A = splitBreps.Last(); 
    
  }
  #endregion
  #region Additional
  
  public BrepFace CreatePatchPoints(List<Point3d> pts, int u, int v)
  {
    var geomPts = new List<GeometryBase>();
    foreach (var pt in pts)
    {
      geomPts.Add(new Point(pt));
    }
    return Brep.CreatePatch(geomPts, u, v, 0.001).Faces[0];
  }
  
  public Curve MeshOutlineToCurve(Mesh mesh)
  {
    if (mesh == null || !mesh.IsValid)
    {
      return null;
    }
    var outlines = mesh.GetNakedEdges();
    if (outlines == null || outlines.Length == 0)
    {
      return null;
    }
    // Convert outlines to curves
    List<Curve> outlineCurves = new List<Curve>();
    foreach (var outline in outlines)
    {
      outlineCurves.Add(outline.ToNurbsCurve());
    }

    return Curve.JoinCurves(outlineCurves)[0];
  }

  public static List<Point3d> MeshToPoints(Mesh mesh)
  {
    if (mesh == null || !mesh.IsValid)
    {
      return null;
    }

    // Extract vertices from the mesh
    List<Point3d> points = new List<Point3d>();
    foreach (var vertex in mesh.Vertices)
    {
      points.Add(vertex);
    }

    return points;
  }
  #endregion
}