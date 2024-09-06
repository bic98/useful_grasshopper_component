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
public abstract class Script_Instance_ea199 : GH_ScriptInstance
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
  private void RunScript(DataTree<Curve> topoLine, DataTree<double> topoHeight, int resolution, ref object A, ref object B, ref object C, ref object D)
  {
      List<List<Curve>> topoLineList = ConvertTreeToNestedList(topoLine);
      List<List<double>> topoHeightList = ConvertTreeToNestedList(topoHeight);
      List<List<Curve>> topoLineRet = new List<List<Curve>>();
      List<GeometryBase> gemoList = new List<GeometryBase>();
      List<Point3d> allDividePts = new List<Point3d>();

      for (int i = 0; i < topoLineList.Count; i++)
      {
        if (topoLineList[i] == null || topoHeightList[i] == null) continue; 
        Curve now = topoLineList[i][0];
        double height = topoHeightList[i][0];
        Point3d st = now.PointAtStart;
        Point3d en = MovePt(st, Vector3d.ZAxis, height);
        Curve nxt = MoveOrientPoint(now, st, en);
        topoLineRet.Add(new List<Curve>() { nxt });
        gemoList.Add(now);
        Point3d[] divLenPts;
        nxt.DivideByLength(10, false, out divLenPts); 
        if(divLenPts != null) allDividePts.AddRange(divLenPts);
      }

      Brep bbox = CalculateBoundingBox(gemoList).ToBrep();
      var segRectangle = bbox.Faces;
      var sortedRectangle = segRectangle.OrderBy(x => x.GetBoundingBox(false).Center.Z).ToList();
      var bottomCrv = Curve.JoinCurves(sortedRectangle[0].ToBrep().Edges)[0];
      var bottomCrvOffset = OffsetCurve(bottomCrv, Plane.WorldXY, 20);

      var findNearestCrv = bbox.Edges.Where(edge =>
        {
          Vector3d edgeDirection = edge.PointAtEnd - edge.PointAtStart;
          edgeDirection.Unitize();
          return Math.Abs(edgeDirection * Vector3d.ZAxis - 1) < RhinoMath.ZeroTolerance;
        })
        .Select(edge => edge.ToNurbsCurve())
        .ToList(); 
      
      
      
      for (int i = 0; i < findNearestCrv.Count; i++)
      {
        var now = findNearestCrv[i];
        double t;
        allDividePts.Sort((x, y) => 
        {
          now.ClosestPoint(x, out t);
          double distX = now.PointAt(t).DistanceTo(x);
    
          now.ClosestPoint(y, out t);
          double distY = now.PointAt(t).DistanceTo(y);
    
          return distX.CompareTo(distY);
        });
        now.ClosestPoint(allDividePts[0], out t);
        var nxt = now.PointAt(t);
        allDividePts.Add(nxt); 
      }
      
      var delMesh = CreateDelaunayMesh(allDividePts);
      int uCount = 0, vCount = 0;
      var ptsSrf = DivideSurface(PlanarSrf(bottomCrvOffset), resolution, ref uCount, ref vCount);
      
      List<Point3d> ptsDelMesh = new List<Point3d>();
      
      for (int i = 0; i < ptsSrf.Count; i++)
      {
        var now = ptsSrf[i]; 
        Ray3d ray = new Ray3d(new Point3d(now.X, now.Y, now.Z - 10.0), Vector3d.ZAxis);
        Point3d? intersectionPoint = MeshRay(delMesh, ray);
        if(intersectionPoint != null) ptsDelMesh.Add(intersectionPoint.Value);
        else
        {
          Print(intersectionPoint.ToString());
        }
      }
      Print(uCount.ToString());
      Print(vCount.ToString());
      var tmp = (uCount + 1) * (1 + vCount);
      Print(tmp.ToString());
      Print(((uCount + 1) *(1 +  vCount)).ToString());
      Print(ptsDelMesh.Count.ToString());
      
      
      var finalTopo = SurfaceFromPoints(ptsDelMesh, uCount + 1, vCount + 1);
      
      
      
      D = delMesh; 
      C = ptsSrf;
      B = ptsDelMesh; 
      A = finalTopo;
  }
  #endregion
  #region Additional
 public Surface SurfaceFromPoints(List<Point3d> points, int uCount, int vCount)
  {
    if (points == null || points.Count < 4)
    {
      throw new ArgumentException("At least 4 points are required to create a surface.");
    }

    if (uCount * vCount != points.Count)
    {
      throw new ArgumentException("The number of points must be equal to uCount * vCount.");
    }

    // Create a NurbsSurface from the points
    NurbsSurface surface = NurbsSurface.CreateFromPoints(points, uCount, vCount, 3, 3);

    return surface;
  }
  
  
  public static Point3d? MeshRay(Mesh mesh, Ray3d ray)
  {
    if (mesh == null || !mesh.IsValid)
    {
      return null;
    }

    // Perform ray intersection with the mesh
    int[] t;
    var intersection = Rhino.Geometry.Intersect.Intersection.MeshRay(mesh, ray, out t);

    if (intersection >= 0)
    {
      Point3d intersectionPoint = ray.PointAt(intersection);
      return intersectionPoint;
    }

    return null;
  }
  public int Gcd(int a, int b)
  {
    while (b != 0)
    {
      int temp = b;
      b = a % b;
      a = temp;
    }

    return Math.Abs(a);
  }
  public Surface PlanarSrf(Curve c)
  {
    string log;
    Surface ret = null;
    if (c.IsValidWithLog(out log) && c.IsPlanar() && c.IsClosed)
    {
      var tmp = Brep.CreatePlanarBreps(c, 0.01);
      ret = tmp[0].Faces[0];
      return ret;
    }
    return ret;
  }
  
  
  public List<Point3d> DivideSurface(Surface surface, int k, ref int uCount, ref int vCount)
  {
    List<Point3d> pointsOnSurface = new List<Point3d>();
    int ul = (int)surface.Domain(0).Length / 100 * 100;
    int vl = (int)surface.Domain(1).Length / 100 * 100; 
    Print(ul.ToString());
    Print(vl.ToString());
    int gcd = Gcd(ul, vl); 
    var candidates = GetDivisors(gcd); 
    int u = ul / candidates[k % candidates.Count];
    int v = vl / candidates[k % candidates.Count];
    Print(u.ToString());
    Print(v.ToString());

    double uSteps = surface.Domain(0).Length / u; 
    double vSteps = surface.Domain(1).Length / v;
    
    for (int i = 0; i < u + 1; i++)
    {
      for (int j = 0; j < v + 1; j++)
      {
        double uParam = surface.Domain(0).T0 + i * uSteps;
        double vParam = surface.Domain(1).T0 + j * vSteps;
        Point3d pt = surface.PointAt(uParam, vParam);
        pointsOnSurface.Add(pt);
      }
    }

    uCount = u;
    vCount = v;
    return pointsOnSurface;
  }


  public List<int> GetDivisors(int n)
  {
    List<int> divisors = new List<int>();

    for (int i = 1; i <= Math.Sqrt(n); i++)
    {
      if (n % i == 0)
      {
        if (i != n / i)
        {
          divisors.Add(n / i);
        }
      }
    }

    divisors.Sort();

    return divisors;
  }
  public static Mesh CreateDelaunayMesh(List<Point3d> pts)
  {
    if (pts == null || pts.Count < 3)
    {
      return null;
    }

    // Create Node2List from points
    var nodes =new Grasshopper.Kernel.Geometry.Node2List(); 
    for (int i = 0; i < pts.Count; i++)
    {
      nodes.Append(new Grasshopper.Kernel.Geometry.Node2(pts[i].X, pts[i].Y));
    }

    // Solve Delaunay
    var faces = new List<Grasshopper.Kernel.Geometry.Delaunay.Face>();
    faces = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Faces(nodes, 0);
    var delMesh = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Mesh(nodes, 0, ref faces);

    // Update mesh vertices with original points
    for (int i = 0; i < pts.Count; i++)
    {
      delMesh.Vertices.SetVertex(i, pts[i]);
    }

    return delMesh;
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

    public static BoundingBox CalculateBoundingBox(IEnumerable < GeometryBase > geometries)
    {
      BoundingBox bbox = BoundingBox.Empty;
      foreach (var geometry in geometries)
      {
        bbox.Union(geometry.GetBoundingBox(true));
      }

      return bbox;
    }

    public Point3d MovePt(Point3d p, Vector3d v, double amp)
    {
      v.Unitize();
      Transform move = Transform.Translation(v * amp);
      p.Transform(move);
      return p;
    }

    public T MoveOrientPoint<T > (T obj, Point3d now, Point3d nxt) where T : GeometryBase
    {
      Plane baseNow = Plane.WorldXY;
      Plane st = new Plane(now, baseNow.XAxis, baseNow.YAxis);
      Plane en = new Plane(nxt, baseNow.XAxis, baseNow.YAxis);
      Transform orient = Transform.PlaneToPlane(st, en);
      obj.Transform(orient);
      return obj;
    }

    public static DataTree<T> MakeDataTree2D<T > (List < List < T >> ret)
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

    public List<List<T>> ConvertTreeToNestedList<T > (DataTree < T > tree)
    {
      List<List<T>> nestedList = new List<List<T>>();
      foreach (GH_Path path in tree.Paths)
      {
        List<T> subList = new List<T>(tree.Branch(path));
        nestedList.Add(subList);
      }
      return nestedList;
    }
  #endregion
}