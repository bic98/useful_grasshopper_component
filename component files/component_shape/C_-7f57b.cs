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
public abstract class Script_Instance_7f57b : GH_ScriptInstance
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
    var contourCurves = Mesh.CreateContourCurves(cutted, minZPoint, maxZPoint, 1, 0.001);
    int dist = (int)maxLength + 100;
    Brep[] planarSurfaces = new Brep[contourCurves.Length];
    Parallel.For(0, contourCurves.Length, i =>
    {
      var curve = contourCurves[i];
      BoundingBox bb = curve.GetBoundingBox(false);
      Plane plane = new Plane(bb.Center, Vector3d.ZAxis);
      Surface planarSurface = new PlaneSurface(plane, new Interval(-dist, dist), new Interval(-dist, dist));
      planarSurfaces[i] = planarSurface.ToBrep();
    });
    ConcurrentBag<Brep> extrudedSurfaces = new ConcurrentBag<Brep>();
    Parallel.For(0, contourCurves.Length, i =>
    {
      var curve = contourCurves[i];
      Point3d now = curve.PointAtEnd;
      Point3d nxt = MovePt(now, Vector3d.ZAxis, 1);
      Curve nxtCurve = curve.DuplicateCurve();
      MoveOrientPoint(nxtCurve, now, nxt);
      var sideSrf = NurbsSurface.CreateRuledSurface(curve, nxtCurve);
      if (sideSrf == null) return;
      var paper = planarSurfaces[i]; 
      var cutter = sideSrf.ToBrep();
      var cuttedSrf = paper.Split(cutter, RhinoDocument.ModelAbsoluteTolerance);
      if (cuttedSrf != null && cuttedSrf.Length > 0)
      {
        Brep caps = cuttedSrf.Last();
        MoveOrientPoint(caps, now, nxt);
        extrudedSurfaces.Add(caps);
        extrudedSurfaces.Add(cutter);
      }
    });
    A = extrudedSurfaces; 
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
  
  private List<List<T>> SplitList<T>(List<T> list, int chunkSize)
  {
    return list
      .Select((item, index) => new { item, index })
      .GroupBy(x => x.index / chunkSize)
      .Select(g => g.Select(x => x.item).ToList())
      .ToList();
  }
  #endregion
}