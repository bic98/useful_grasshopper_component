using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_d412a : GH_ScriptInstance
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
  private void RunScript(Surface x, ref object A)
  {
   // Get edges of the surface
    var edges = x.ToBrep().Edges.ToList();

    // Get bounding box and its corners
    BoundingBox bbox = x.GetBoundingBox(true);
    Point3d[] corners = bbox.GetCorners();

    // Find max and min Z points
    Point3d maxZPoint = corners.OrderByDescending(pt => pt.Z).FirstOrDefault();
    Point3d minZPoint = corners.OrderBy(pt => pt.Z).FirstOrDefault();
    minZPoint = new Point3d(minZPoint.X, minZPoint.Y, minZPoint.Z - 3);
    // Create a point below the min Z point
    Point3d boxStPt = new Point3d(minZPoint.X, minZPoint.Y, minZPoint.Z - 100);
    Plane pl = Plane.WorldXY;
    pl.Origin = boxStPt;

    // Initialize lists for breps and curves
    List<Brep> breps = new List<Brep>() { x.ToBrep() };
    List<Curve> curves = new List<Curve>();
    
    double maxLength = 0.0;
    foreach (var i in edges)
    {
      Curve upCrv = i.DuplicateCurve();
      Curve dnCrv = Curve.ProjectToPlane(upCrv, pl);
      var sideSrf = NurbsSurface.CreateRuledSurface(upCrv, dnCrv);
      breps.Add(sideSrf.ToBrep());
      curves.Add(dnCrv);
      double length = dnCrv.GetLength();
      if (length > maxLength)
      {
        maxLength = length;
      }
      
    }

    // Create bottom surface
    breps.Add(PlanarSrf(Curve.JoinCurves(curves)[0]));

    // Union all breps
    var union = Brep.JoinBreps(breps, 0.01);
    MeshingParameters meshingParams = new MeshingParameters();
    meshingParams.RefineGrid = true;
    Mesh tmp = new Mesh(); 
    var mesh = Mesh.CreateFromBrep(union[0], meshingParams);
    tmp.Append(mesh); 
    Mesh cutted = tmp; 
    var contourCurves = Mesh.CreateContourCurves(cutted, minZPoint, maxZPoint, 1.0, 0.001);
    ConcurrentBag<Brep> extrudedSurfaces = new ConcurrentBag<Brep>();
    int dist = (int)maxLength + 100; 
    
    Parallel.ForEach(contourCurves, curve =>
    {
      Point3d now = curve.PointAtEnd;
      Point3d nxt = MovePt(now, Vector3d.ZAxis, 1.0);
      Curve nxtCurve = curve.DuplicateCurve();
      MoveOrientPoint(nxtCurve, now, nxt);
      var sideSrf = NurbsSurface.CreateRuledSurface(curve, nxtCurve);
      if (sideSrf == null) return;
      List<Brep> ret = new List<Brep>(); 
      BoundingBox bb = curve.GetBoundingBox(false);
      Plane plane = new Plane(bb.Center, Vector3d.ZAxis);
      Surface planarSurface = new PlaneSurface(plane, new Interval(-dist, dist), new Interval(-dist, dist));
      var paper = planarSurface.ToBrep();
      var cutter = sideSrf.ToBrep(); 
      ret.Add(cutter);
      var cuttedSrf = paper.Split(cutter, 0.001); 
      if (cuttedSrf != null && cuttedSrf.Length > 0)
      {
        Brep caps = cuttedSrf.Last();
        ret.Add(caps);
        MoveOrientPoint(caps, now, nxt);
        ret.Add(caps); 
      }
      var joinBreps = Brep.JoinBreps(ret, 0.001); 
      if (joinBreps != null && joinBreps.Length > 0)
      {
        extrudedSurfaces.Add(joinBreps[0]);
      }
    });
    A = extrudedSurfaces; 
    
  }
  #endregion
  #region Additional
  public T MoveOrientPoint<T>(T obj, Point3d now, Point3d nxt) where T : GeometryBase
  {
    Plane baseNow = Plane.WorldXY;
    Plane st = new Plane(now, baseNow.XAxis, baseNow.YAxis);
    Plane en = new Plane(nxt, baseNow.XAxis, baseNow.YAxis);
    Transform orient = Transform.PlaneToPlane(st, en);
    obj.Transform(orient);
    return obj;
  }
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

  public Point3d MovePt(Point3d p, Vector3d v, double amp)
  {
    v.Unitize();
    Point3d newPoint = new Point3d(p);
    newPoint.Transform(Transform.Translation(v * amp));
    return newPoint;
  }
  #endregion
}