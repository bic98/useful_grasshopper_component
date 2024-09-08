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
using System.Threading.Tasks;


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
  private void RunScript(DataTree<Curve> topoLine, DataTree<double> topoHeight, int resolution, bool Run, ref object topo3D)
  {
    
      if (!Run) return;

      // 데이터를 중첩된 리스트로 변환
      List<List<Curve>> topoLineList = ConvertTreeToNestedList(topoLine);
      List<List<double>> topoHeightList = ConvertTreeToNestedList(topoHeight);
      List<List<Curve>> topoLineRet = new List<List<Curve>>();
      List<GeometryBase> gemoList = new List<GeometryBase>();
      List<Point3d> allDividePts = new List<Point3d>();

      // 병렬 처리: 각 라인을 처리하고 분할 포인트 계산
      Parallel.For(0, topoLineList.Count, i =>
        {
        if (topoLineList[i] == null || topoHeightList[i] == null) return;

        Curve now = topoLineList[i][0];
        double height = topoHeightList[i][0];
        Point3d st = now.PointAtStart;
        Point3d en = MovePt(st, Vector3d.ZAxis, height);
        Curve nxt = MoveOrientPoint(now, st, en);

        lock (topoLineRet)
        {
          topoLineRet.Add(new List<Curve>() { nxt });
          gemoList.Add(now);
        }

        Point3d[] divLenPts;
        nxt.DivideByLength(10, false, out divLenPts);
        if (divLenPts != null)
        {
          lock (allDividePts)
          {
            allDividePts.AddRange(divLenPts);
          }
        }
        });

      // Bounding Box 생성 및 계산
      Brep bbox = CalculateBoundingBox(gemoList).ToBrep();
      var segRectangle = bbox.Faces;
      var sortedRectangle = segRectangle.OrderBy(x => x.GetBoundingBox(false).Center.Z).ToList();
      var bottomCrv = Curve.JoinCurves(sortedRectangle[0].ToBrep().Edges)[0];
      var bottomCrvOffset = OffsetCurve(bottomCrv, Plane.WorldXY, 20);

      // 가장 가까운 커브 찾기
      var findNearestCrv = bbox.Edges
        .Where(edge =>
        {
        Vector3d edgeDirection = edge.PointAtEnd - edge.PointAtStart;
        edgeDirection.Unitize();
        return Math.Abs(edgeDirection * Vector3d.ZAxis - 1) < RhinoMath.ZeroTolerance;
        })
        .Select(edge => edge.ToNurbsCurve())
        .ToList();


      // 병렬 처리: 가장 가까운 커브 찾기에서 거리 정렬 최적화
      Parallel.For(0, findNearestCrv.Count, i =>
        {
        var now = findNearestCrv[i];
        double t;

        // 가장 가까운 포인트를 찾기 위해 거리 비교 (불필요한 정렬 제거)
        Point3d closestPt = allDividePts.OrderBy(pt =>
          {
          now.ClosestPoint(pt, out t);
          return now.PointAt(t).DistanceTo(pt);
          }).First();

        now.ClosestPoint(closestPt, out t);
        var nxt = now.PointAt(t);

        lock (allDividePts)
        {
          allDividePts.Add(nxt);
        }
        });

      // 델로니 메쉬 생성
      var delMesh = CreateDelaunayMesh(allDividePts);
      int uCount = 0, vCount = 0;
      var ptsSrf = DivideSurface(PlanarSrf(bottomCrvOffset), resolution, ref uCount, ref vCount);

      // 초기화: uCount, vCount를 사용하여 2차원 리스트 생성
      Point3d?[,] sortedPtsDelMesh = new Point3d?[uCount + 1, vCount + 1]; // 2D 배열을 사용하여 포인트를 정렬된 위치에 저장

      // 병렬 처리: 레이와 메쉬 교차점 찾기 및 u, v 정렬
      Parallel.For(0, ptsSrf.Count, i =>
        {
        var now = ptsSrf[i];
        Ray3d ray = new Ray3d(new Point3d(now.X, now.Y, now.Z - 1000.0), Vector3d.ZAxis);
        Point3d? intersectionPoint = MeshRay(delMesh, ray);
        if (intersectionPoint != null)
        {
          // i를 u, v 인덱스로 변환
          int uIndex = i / (vCount + 1);
          int vIndex = i % (vCount + 1);

          // 스레드 안전한 방식으로 2D 배열에 포인트 저장
          sortedPtsDelMesh[uIndex, vIndex] = intersectionPoint.Value;
        }
        });

      List<Point3d> ptsDelMesh = new List<Point3d>(uCount * vCount);
      for (int u = 0; u <= uCount; u++)
      {
        for (int v = 0; v <= vCount; v++)
        {
          var point = sortedPtsDelMesh[u, v];
          if (point.HasValue)
          {
            ptsDelMesh.Add(point.Value);
          }
        }
      }

      var finalTopo = SurfaceFromPoints(ptsDelMesh, uCount + 1, vCount + 1);
      topo3D = finalTopo;
  }
  #endregion
  #region Additional

    public Surface SurfaceFromPoints(List < Point3d > points, int uCount, int vCount)
    {
      // 입력 검증
      if (points == null || points.Count < 4)
        throw new ArgumentException("At least 4 points are required to create a surface.");
      if (uCount * vCount != points.Count)
        throw new ArgumentException("The number of points must be equal to uCount * vCount.");

      // NurbsSurface 생성 및 반환
      return NurbsSurface.CreateFromPoints(points, uCount, vCount, 3, 3);
    }

    public static Point3d? MeshRay(Mesh mesh, Ray3d ray)
    {
      if (mesh == null || !mesh.IsValid)
        return null;

      // 레이와 메쉬의 교차점 계산
      int[] t;
      var intersection = Rhino.Geometry.Intersect.Intersection.MeshRay(mesh, ray, out t);
      return intersection >= 0 ? (Point3d?) ray.PointAt(intersection) : null;
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
      if (c.IsValid && c.IsPlanar() && c.IsClosed)
      {
        var tmp = Brep.CreatePlanarBreps(c, 0.01);
        return tmp[0].Faces[0];
      }

      return null;
    }

    public List<Point3d> DivideSurface(Surface surface, int k, ref int uCount, ref int vCount)
    {
      List<Point3d> pointsOnSurface = new List<Point3d>();
      int ul = (int) surface.Domain(0).Length / 100 * 100;
      int vl = (int) surface.Domain(1).Length / 100 * 100;
      int gcd = Gcd(ul, vl);
      var candidates = GetDivisors(gcd);
      int u = ul / candidates[Math.Min(candidates.Count - 1, k)];
      int v = vl / candidates[Math.Min(candidates.Count - 1, k)];

      double uSteps = surface.Domain(0).Length / u;
      double vSteps = surface.Domain(1).Length / v;

      // 초기 용량을 설정하여 메모리 할당 최적화
      pointsOnSurface.Capacity = (u + 1) * (v + 1);

      for (int i = 0; i <= u; i++)
      {
        for (int j = 0; j <= v; j++)
        {
          double uParam = surface.Domain(0).T0 + i * uSteps;
          double vParam = surface.Domain(1).T0 + j * vSteps;
          pointsOnSurface.Add(surface.PointAt(uParam, vParam));
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
          divisors.Add(i);
          if (i != n / i)
            divisors.Add(n / i);
        }
      }

      // 조건에 맞는 약수 필터링 및 정렬
      return divisors.Where(x => x >= 10).OrderByDescending(x => x).ToList();
    }

    public static Mesh CreateDelaunayMesh(List < Point3d > pts)
    {
      if (pts == null || pts.Count < 3)
      {
        return null;
      }

      // Create Node2List from points
      var nodes = new Grasshopper.Kernel.Geometry.Node2List();
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

    public Curve OffsetCurve(Curve c, Plane p, double interval)
    {
      var seg = c.DuplicateSegments();
      var joinseg = Curve.JoinCurves(seg);
      List<Curve> outLines = new List<Curve>(joinseg.Length);

      foreach (var js in joinseg)
        outLines.AddRange(c.Offset(p, interval, 0, CurveOffsetCornerStyle.Sharp));

      var ret = Curve.JoinCurves(outLines);
      return ret != null && ret.Length > 0 ? ret[0] : null;
    }

    public static BoundingBox CalculateBoundingBox(IEnumerable < GeometryBase > geometries)
    {
      BoundingBox bbox = BoundingBox.Empty;
      foreach (var geometry in geometries)
        bbox.Union(geometry.GetBoundingBox(true));

      return bbox;
    }

    public Point3d MovePt(Point3d p, Vector3d v, double amp)
    {
      v.Unitize();
      p.Transform(Transform.Translation(v * amp));
      return p;
    }

    public T MoveOrientPoint<T > (T obj, Point3d now, Point3d nxt) where T : GeometryBase
    {
      Plane st = new Plane(now, Plane.WorldXY.XAxis, Plane.WorldXY.YAxis);
      Plane en = new Plane(nxt, Plane.WorldXY.XAxis, Plane.WorldXY.YAxis);
      obj.Transform(Transform.PlaneToPlane(st, en));
      return obj;
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