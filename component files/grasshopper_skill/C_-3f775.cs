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
using Rhino.Render;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_3f775 : GH_ScriptInstance
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
  private void RunScript(DataTree<Point3d> x, object y, ref object A, ref object B, ref object C, ref object D)
  {
    var li = ConvertDataTreeToList(x);
    List<Curve> ans = new List<Curve>(); 
    for (int i = 0; i < li.Count; i++)
    {
      var now = li[i]; 
      now.Sort((u, v) => u.Y.CompareTo(v.Y));
      var crv = new PolylineCurve(now);
      var crvs = crv.DuplicateSegments();
      foreach (var VARIABLE in crvs)
      {
        ans.Add(VARIABLE); 
      }
      li[i] = now; 
    }

    for (int i = 0; i < 4; i++)
    {
      List<Point3d> tmp = new List<Point3d>();
      for (int j = 0; j < li.Count(); j++)
      {
        if (li[j].Count == 2)
        {
          if(i == 0 || i == 3) continue;
          tmp.Add(li[j][i - 1]); 
        }
        else tmp.Add(li[j][i]); 
      }

      tmp.Sort((u, v) => u.X.CompareTo(v.X));
      var crv = new PolylineCurve(tmp);
      var crvs = crv.DuplicateSegments();
      foreach (var VARIABLE in crvs)
      {
        ans.Add(VARIABLE); 
      }
    }

    List<Surface> srf = new List<Surface>();
    HashSet<int> hS = new HashSet<int>(); 
    for (int i = 0; i < li.Count; i++)
    {
      for (int j = 0; j < li[i].Count; j++)
      {
        var nowP = li[i][j];
        var pic = PointsInCurves(ans, nowP);
        List<Point3d> ccwP = new List<Point3d>();
        for (int p = 0; p < pic.Count; p++)
        {
          var a = pic[p].PointAtEnd;
          var b = pic[p].PointAtStart;
          if (a == nowP) ccwP.Add(b);
          else ccwP.Add(a);

        }
        
        ccwP = SortPointsClockwise(ccwP, nowP);
        for (int p = 0; p < ccwP.Count; p++)
        {
          Surface tmp = NurbsSurface.CreateFromCorners(nowP, ccwP[p], ccwP[(p + 1) % ccwP.Count]);
          var code = tmp.PointAt(0.5, 0.5); 
          srf.Add(tmp);
        }

        if (i == 3 && j == 1)
        {
          A = pic;
          B = ccwP; 
        }

      }
    }

    C = srf; 
  }
  #endregion
  #region Additional
  
  public List<Point3d> SortPointsClockwise(List<Point3d> points, Point3d referencePoint)
  {
    // 기준점과의 벡터를 이용하여 정렬
    points.Sort((pointA, pointB) =>
    {
      Vector3d vecA = pointA - referencePoint;
      Vector3d vecB = pointB - referencePoint;

      // z축을 기준으로 두 벡터의 외적을 계산
      var crossProduct = Vector3d.CrossProduct(vecA, vecB).Z; 

      // CCW 방향에 따라 정렬
      if (crossProduct > 0)
        return 1; // vecA가 vecB보다 시계방향으로 더 작은 각도를 가짐
      else if (crossProduct < 0)
        return -1; // vecB가 vecA보다 시계방향으로 더 작은 각도를 가짐
      else
      {
        // 외적이 0이면, 두 점이 같은 선상에 있음, 기준점에서 더 가까운 점을 먼저 배치
        double distA = vecA.Length;
        double distB = vecB.Length;
        return distA.CompareTo(distB);
      }
    });

    return points;
  }
  public List<Curve> PointsInCurves(List<Curve> crves, Point3d pt)
  {
    List<Curve> ans = new List<Curve>();
    for (int i = 0; i < crves.Count; i++)
    {
      double t;
      if (crves[i].ClosestPoint(pt, out t, 0.001))
      {
        ans.Add(crves[i]);
      }
    }

    return ans;
  }
  public List<List<T>> ConvertDataTreeToList<T>(DataTree<T> dataTree)
  {
    List<List<T>> list2D = new List<List<T>>();

    foreach (GH_Path path in dataTree.Paths)
    {
      List<T> branchList = dataTree.Branch(path).ToList();
      list2D.Add(branchList);
    }

    return list2D;
  }
  #endregion
}