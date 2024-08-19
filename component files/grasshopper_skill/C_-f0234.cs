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
public abstract class Script_Instance_f0234 : GH_ScriptInstance
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
  private void RunScript(List<Curve> x, int y, ref object A)
  {
    
      Dictionary<string, List<Curve>> coplanarGroups = new Dictionary<string, List<Curve>>();
      List<Curve> checkLinear = new List<Curve>();

      // 첫 번째 반복: 직선과 곡선을 구분
      for (int i = 0; i < x.Count; i++)
      {
        if (x[i].IsLinear())
        {
          checkLinear.Add(x[i]);
        }
        else
        {
          Plane curvePlane;
          if (x[i].TryGetPlane(out curvePlane, Rhino.RhinoMath.ZeroTolerance))
          {
            // 법선 벡터와 원점에서의 최단 거리를 기반으로 키 생성
            string planeKey = GeneratePlaneKey(curvePlane.Normal, curvePlane.DistanceTo(Point3d.Origin));

            // 이미 키가 존재하는지 확인 후, 곡선을 추가
            if (coplanarGroups.ContainsKey(planeKey))
            {
              coplanarGroups[planeKey].Add(x[i]);
            }
            else
            {
              coplanarGroups[planeKey] = new List<Curve> { x[i] };
            }
          }
        }
      }

      // 두 번째 반복: 남은 직선을 기존 그룹에 추가
      for (int i = 0; i < checkLinear.Count; i++)
      {
        Line li = new Line(checkLinear[i].PointAtStart, checkLinear[i].PointAtEnd);

        // 각 그룹의 법선 벡터와 현재 직선의 방향 벡터를 비교
        foreach (var g in coplanarGroups)
        {
          string[] keyParts = g.Key.Split('_');
          Vector3d normal = new Vector3d(double.Parse(keyParts[0]), double.Parse(keyParts[1]), double.Parse(keyParts[2]));

          // 직선의 방향 벡터와 법선 벡터가 평행한지 확인
          if (Math.Abs(Vector3d.Multiply(normal, li.Direction)) <= RhinoMath.ZeroTolerance)
          {
            coplanarGroups[g.Key].Add(checkLinear[i]);
            break;
          }
        }
      }

      // 결과를 데이터 트리로 변환
      List<List<Curve>> ans = new List<List<Curve>>();
      foreach (var g in coplanarGroups)
      {
        ans.Add(g.Value);
      }
      A = MakeDataTree2D(ans);
  }
  #endregion
  #region Additional
    private static string GeneratePlaneKey(Vector3d normal, double distance)
    {
      normal.Unitize();  // 법선 벡터를 정규화

      // 법선 벡터와 최단 거리 값을 기반으로 고유 키 생성 (소수점 이하 6자리로 반올림)
      string key = string.Format("{0}_{1}_{2}_{3}",
        Math.Round(normal.X, 6),
        Math.Round(normal.Y, 6),
        Math.Round(normal.Z, 6),
        Math.Round(distance, 6));
      return key;
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

    private int GetRandomInt(int l = 0, int r = int.MaxValue)
    {
      Random rd = new Random(GetSeed());
      return rd.Next(l, r + 1); // Next method in C# is exclusive of the upper bound, so add 1
    }

    private double GetRandomDouble(double l = 0.0, double r = 1.0)
    {
      Random rd = new Random(GetSeed());
      return rd.NextDouble() * (r - l) + l;
    }

    private int GetSeed()
    {
      long seed = DateTime.UtcNow.Ticks;
      return (int) (seed & 0xFFFFFFFF) ^ (int) (seed >> 32); // Combine lower and upper bits for the seed
    }
  #endregion
}