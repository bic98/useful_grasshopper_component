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
public abstract class Script_Instance_f9c51 : GH_ScriptInstance
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
    
      Dictionary<Plane, List<Curve>> coplanarGroups = new Dictionary<Plane, List<Curve>>();
      List<Curve> checkLinear = new List<Curve>();

      for (int i = 0; i < x.Count; i++)
      {
        if (x[i].IsLinear())
        {
          checkLinear.Add(x[i]);
        }
        else
        {
          Plane curvePlane;
          if (x[i].TryGetPlane(out curvePlane, 0.01))
          {
            bool found = false;

            // 이미 존재하는 평면인지 확인
            foreach (var plane in coplanarGroups.Keys)
            {
              if (ArePlanesEqual(plane, curvePlane))
              {
                coplanarGroups[plane].Add(x[i]);
                found = true;
                break;
              }
            }

            // 존재하지 않으면 새로운 그룹에 추가
            if (!found)
            {
              coplanarGroups[curvePlane] = new List<Curve> { x[i] };
            }
          }
        }
      }

      x = checkLinear;
      for (int i = 0; i < 10000; i++)
      {
        List<Curve> bucket = new List<Curve>();
        if (x.Count >= 3)
        {
          List<Curve> rdCurve = new List<Curve>();
          List<bool> vis = Enumerable.Repeat(false, x.Count).ToList();
          while (rdCurve.Count < 3)
          {
            int now = GetRandomInt(0, x.Count - 1);
            if (vis[now] == false)
            {
              rdCurve.Add(x[now]);
              vis[now] = true;
            }
          }

          var ok = CheckCoplanar(rdCurve);


          if (ok.Item1 == true)
          {
            bool found = false;
            foreach (var plane in coplanarGroups.Keys)
            {
              if (ArePlanesEqual(plane, ok.Item2))
              {
                coplanarGroups[plane].AddRange(rdCurve);
                found = true;
                break;
              }
            }

            if (!found)
            {
              coplanarGroups[ok.Item2] = rdCurve;
            }

            for(int j = 0; j < x.Count(); j++)
            {
              if(vis[j]) continue;
              var curve = x[j];
              Vector3d v = new Line(curve.PointAtStart, curve.PointAtEnd).Direction;
              foreach (var g in coplanarGroups)
              {
                if (Math.Abs(Vector3d.Multiply(g.Key.Normal, v)) < 0.1)
                {
                  if (IsCurveOnPlane(curve, g.Key, 1))
                  {
                    g.Value.Add(curve);
                    vis[j] = true;
                    break;
                  }
                }
                else
                {
                  Print(Vector3d.Multiply(g.Key.Normal, v).ToString());
                }
              }
            }
          }


          for (int j = 0; j < x.Count; j++)
          {
            if ((vis[j] == true) && (ok.Item1 == true)) continue;
            bucket.Add(x[j]);
          }
        }
        else
        {
          break;
        }

        x = bucket;
      }


      List<List<Curve>> ans = new List<List<Curve>>();
      foreach (var g in coplanarGroups)
      {
        ans.Add(g.Value);
      }

      A = MakeDataTree2D(ans);
  }
  #endregion
  #region Additional

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

    private bool ArePlanesEqual(Plane plane1, Plane plane2)
    {
      bool normalsAreParallel = plane1.Normal.IsParallelTo(plane2.Normal, 0.1) == 1;
      double distanceDifference = Math.Abs(plane1.DistanceTo(Point3d.Origin) - plane2.DistanceTo(Point3d.Origin));

      return normalsAreParallel && distanceDifference < 1;
    }


    public Tuple<bool, Plane> CheckCoplanar(List < Curve > crves)
    {
      if (crves.Count != 3)
      {
        throw new ArgumentException("This function requires exactly three curves.");
      }

      // 1. 방향 벡터 리스트 생성
      Point3d point1 = RoundPoint(crves[0].PointAtStart);
      Point3d point2 = RoundPoint(crves[1].PointAtStart);

      Vector3d direction1 = new Line(point1, RoundPoint(crves[0].PointAtEnd)).Direction;
      Vector3d direction2 = new Line(point2, RoundPoint(crves[1].PointAtEnd)).Direction;

      // 2. 법선 벡터 계산 (두 직선의 방향 벡터의 외적)
      Vector3d normal = Vector3d.CrossProduct(direction1, direction2);
      normal.Unitize();
      normal.X = Math.Round(normal.X, 3) * 1e3;
      normal.Y = Math.Round(normal.Y, 3) * 1e3;
      normal.Z = Math.Round(normal.Z, 3) * 1e3;

      if (normal.IsZero)
      {
        return new Tuple<bool, Plane>(false, Plane.Unset); // 두 벡터가 평행하면 평면을 정의할 수 없음
      }

      double D = -(normal.X * point1.X + normal.Y * point1.Y + normal.Z * point1.Z);
      // 5. 두 번째 직선이 이 평면에 포함되는지 확인 (허용 오차 사용)
      double distanceToPlane = normal.X * point2.X + normal.Y * point2.Y + normal.Z * point2.Z + D;


      if (Math.Abs(distanceToPlane) > 1e2)
      {
        return new Tuple<bool, Plane>(false, Plane.Unset); // 두 번째 직선이 평면에 속하지 않음
      }

      // 6. 세 번째 직선이 동일한 평면에 있는지 확인 (허용 오차 사용)
      Point3d testPointStart = RoundPoint(crves[2].PointAtStart);
      Point3d testPointEnd = RoundPoint(crves[2].PointAtEnd);

      double distanceToPlaneStart =
        normal.X * testPointStart.X + normal.Y * testPointStart.Y + normal.Z * testPointStart.Z + D;
      double distanceToPlaneEnd = normal.X * testPointEnd.X + normal.Y * testPointEnd.Y + normal.Z * testPointEnd.Z + D;

      if (Math.Abs(distanceToPlaneStart) > 1e2 || Math.Abs(distanceToPlaneEnd) > 1e2)
      {
        return new Tuple<bool, Plane>(false, Plane.Unset); // 세 번째 직선이 평면에 속하지 않음
      }

      // 7. 평면을 정의하고 반환
      Plane plane = new Plane(RoundPoint(point1), normal);
      return new Tuple<bool, Plane>(true, plane);
    }

    private bool IsCurveOnPlane(Curve curve, Plane plane, double tolerance = 0.01)
    {
      // 곡선의 양 끝점 구하기
      Point3d startPoint = RoundPoint(curve.PointAtStart);
      Point3d endPoint = RoundPoint(curve.PointAtEnd);

      // 시작점과 끝점이 평면에 포함되는지 여부 확인
      double distanceToStart = plane.DistanceTo(startPoint);
      double distanceToEnd = plane.DistanceTo(endPoint);

      // 두 점이 모두 평면에 포함되는지 확인
      return Math.Abs(distanceToStart) <= tolerance && Math.Abs(distanceToEnd) <= tolerance;
    }

    private static Point3d RoundPoint(Point3d point)
    {
      return new Point3d(
        Math.Round(point.X, 3),
        Math.Round(point.Y, 3),
        Math.Round(point.Z, 3)
        );
    }

    private int GetRandomInt(int l = 0, int r = int.MaxValue)
    {
      Random rd = new Random(GetSeed());
      return rd.Next(l, r + 1); // Next method in C# is exclusive of the upper bound, so add 1
    }

    // Function to get a random double between l and r
    private double GetRandomDouble(double l = 0.0, double r = 1.0)
    {
      Random rd = new Random(GetSeed());
      return rd.NextDouble() * (r - l) + l;
    }

    // Function to get a time-based seed
    private int GetSeed()
    {
      long seed = DateTime.UtcNow.Ticks;
      return (int) (seed & 0xFFFFFFFF) ^ (int) (seed >> 32); // Combine lower and upper bits for the seed
    }
  #endregion
}