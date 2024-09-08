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
using System.Threading;
using System.Threading.Tasks;
using Rhino.Geometry.Intersect;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_69c08 : GH_ScriptInstance
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
  private void RunScript(DataTree<Curve> buildingLine, DataTree<double> buildingHeight, Surface topo3D, bool Run, ref object buildings)
  {

      if (!Run) return;

      var buildingLines = ConvertTreeToNestedList(buildingLine);
      var buildingHeights = ConvertTreeToNestedList(buildingHeight);

      List<List<Curve>> pureLines = new List<List<Curve>>(buildingLines.Count);
      List<List<Point3d>> purePoints = new List<List<Point3d>>(buildingLines.Count);
      List<List<Point3d>> nxtPurePoints = new List<List<Point3d>>(buildingLines.Count);

      var raySrf = new List<GeometryBase>() { topo3D.ToBrep() };
      List<List<double>> pureHeights = new List<List<double>>(buildingLines.Count);
      List<List<double>> deem = new List<List<double>>(buildingLines.Count);

      // 병렬로 빌딩 라인 처리
      Parallel.For(0, buildingLines.Count, i =>
        {
        var now = buildingLines[i][0];
        var disconNow = Discontinuity(now);
        var rayPts = new List<Point3d>(disconNow.Count);

        foreach (var disconPts in disconNow)
        {
          Ray3d ray = new Ray3d(new Point3d(disconPts.X, disconPts.Y, disconPts.Z - 100.0), Vector3d.ZAxis);
          var pts = Intersection.RayShoot(ray, raySrf, 1).ToList();
          if (pts.Count != 0) rayPts.Add(pts[0]);
        }

        if (rayPts.Count != disconNow.Count) return;

        double minDist = double.MaxValue, maxDist = double.MinValue;
        int id = -1;

        for (int j = 0; j < rayPts.Count; j++)
        {
          var dist = rayPts[j].DistanceTo(disconNow[j]);
          if (dist < minDist)
          {
            minDist = dist;
            id = j;
          }

          if (dist > maxDist)
          {
            maxDist = dist;
          }
        }

        lock (nxtPurePoints)
        {
          nxtPurePoints.Add(new List<Point3d>() { rayPts[id] });
          purePoints.Add(new List<Point3d>() { disconNow[id] });
          pureLines.Add(buildingLines[i]);
          pureHeights.Add(buildingHeights[i]);
          deem.Add(new List<double>() { maxDist - minDist });
        }
        });

      // List<List<Curve>> moveLines = new List<List<Curve>>(pureLines.Count);
      // List<List<Brep>> makeBuildings = new List<List<Brep>>(pureLines.Count);
      List<Brep> makeBuildingsParapet = new List<Brep>(pureLines.Count);

      
      // 병렬 처리: 건물 생성 작업
      Parallel.For(0, pureLines.Count, i =>
      {
        var now = pureLines[i][0];
        Plane pl;
        now.TryGetPlane(out pl);
        if (pl.ZAxis.Z < 0) now.Reverse();

        var nxt = MoveOrientPoint(now, purePoints[i][0], nxtPurePoints[i][0]);
        // List<Curve> localMoveLines = new List<Curve> { nxt };
        // List<Brep> localMakeBuildings = new List<Brep> { Extrude(nxt, deem[i][0] + 3.5 * pureHeights[i][0]).ToBrep() };
        //
        List<Brep> faces = new List<Brep>();
        Curve nxtUpper = nxt.DuplicateCurve();
        nxtUpper = MoveOrientPoint(nxtUpper, nxtPurePoints[i][0], MovePt(nxtPurePoints[i][0], Vector3d.ZAxis, deem[i][0] + 3.5 * pureHeights[i][0]));
        faces.Add(NurbsSurface.CreateRuledSurface(nxt, nxtUpper).ToBrep());

        nxtUpper.TryGetPlane(out pl);
        var nxtUpperParapet = OffsetCurve(nxtUpper, pl, -0.3);
        if (nxtUpperParapet == null) return;
        faces.AddRange(nxtUpperParapet);

        var finalBuilding = JoinAndCapBreps(faces);
        if (finalBuilding == null) return;

        // lock (moveLines)
        // {
        //   moveLines.Add(localMoveLines);
        // }
        //
        // lock (makeBuildings)
        // {
        //   makeBuildings.Add(localMakeBuildings);
        // }

        lock (makeBuildingsParapet)
        {
          makeBuildingsParapet.Add(finalBuilding);
        }
      });
 


      buildings = makeBuildingsParapet; 
  }
  #endregion
  #region Additional
  
  public static Brep JoinAndCapBreps(List<Brep> breps)
  {
    if (breps == null || breps.Count == 0)
      return null;

    // Join the Breps
    Brep[] joinedBreps = Brep.JoinBreps(breps, Rhino.RhinoMath.ZeroTolerance);
    if (joinedBreps == null || joinedBreps.Length == 0)
      return null;

    // Cap the joined Brep
    Brep cappedBrep = joinedBreps[0].CapPlanarHoles(Rhino.RhinoMath.ZeroTolerance);
    return cappedBrep;
  }
  
    // 포인트를 주어진 벡터 방향으로 이동시키는 함수
    public Point3d MovePt(Point3d p, Vector3d v, double amp)
    {
      v.Unitize(); // 벡터를 단위 벡터로 변환
      p.Transform(Transform.Translation(v * amp)); // 변환을 바로 적용
      return p; // 이동된 포인트 반환
    }

    // 주어진 커브로부터 평면 브렙 생성
    public Brep PlanarSrf(Curve c)
    {
      // 유효성 검사 및 평면 여부 확인
      if (c.IsValid && c.IsPlanar() && c.IsClosed)
      {
        var tmp = Brep.CreatePlanarBreps(c, 0.01);
        if (tmp != null && tmp.Length > 0)
          return tmp[0]; // 유효한 브렙 반환
      }

      return null; // 유효하지 않으면 null 반환
    }

    // 커브를 오프셋하고 결과 브렙 생성
    public List<Brep> OffsetCurve(Curve c, Plane p, double interval)
    {
      var seg = c.DuplicateSegments(); // 커브 세그먼트 복제
      var joinseg = Curve.JoinCurves(seg); // 세그먼트를 조인
      List<Curve> outLines = new List<Curve>(joinseg.Length); // 리스트의 초기 용량 설정

      foreach (var js in joinseg)
      {
        var offset = js.Offset(p, interval, 0.001, CurveOffsetCornerStyle.Sharp);
        if (offset != null && offset.Length > 0)
          outLines.AddRange(offset);
      }

      var ret = Curve.JoinCurves(outLines);
      if (ret == null || ret.Length == 0) return null; // 오프셋 결과가 유효하지 않은 경우

      var boundary = PlanarSrf(c);
      if (boundary == null) return null; // 평면 브렙이 생성되지 않은 경우

      var outBrep = boundary.Split(ret, 0.001); // 오프셋 커브로 분할
      if (outBrep == null || outBrep.Length == 0) return null; // 분할 결과가 없는 경우
      
      List<Brep> parapet = new List<Brep>() { outBrep[0] };
      foreach (var crv in ret)
      {
        var tmp = crv.DuplicateCurve(); 
        var nxt = MoveOrientPoint(tmp, crv.PointAtStart, MovePt(crv.PointAtStart, -Vector3d.ZAxis, 1.3));
        parapet.Add(NurbsSurface.CreateRuledSurface(crv, nxt).ToBrep());
      }

      return parapet;  // 첫 번째 분할 결과 반환
    }

    // 주어진 커브를 주어진 높이로 extrusion(압출)하는 함수
    public static Extrusion Extrude(Curve curve, double height)
    {
      return Extrusion.Create(curve, height, true); // extrusion 생성 및 반환
    }

    // 커브의 불연속 점들을 찾는 함수
    public List<Point3d> Discontinuity(Curve x)
    {
      var seg = x.DuplicateSegments(); // 커브 세그먼트 복제
      List<Point3d> pts = new List<Point3d>(seg.Length + 1); // 리스트의 초기 용량 설정

      // 모든 세그먼트의 시작과 끝점을 추가
      foreach (var s in seg)
      {
        if (pts.Count == 0) pts.Add(s.PointAtStart);
        pts.Add(s.PointAtEnd);
      }

      // 닫힌 커브의 경우 마지막 중복 점 제거
      if (x.IsClosed) pts.RemoveAt(pts.Count - 1);
      return pts; // 불연속점 리스트 반환
    }

    // 오브젝트를 두 점 사이에서 이동시키는 함수
    public T MoveOrientPoint<T > (T obj, Point3d now, Point3d nxt) where T : GeometryBase
    {
      Plane st = new Plane(now, Plane.WorldXY.XAxis, Plane.WorldXY.YAxis); // 시작 평면
      Plane en = new Plane(nxt, Plane.WorldXY.XAxis, Plane.WorldXY.YAxis); // 목표 평면
      obj.Transform(Transform.PlaneToPlane(st, en)); // 변환 적용
      return obj; // 변환된 오브젝트 반환
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
      ;
    }
  #endregion
}