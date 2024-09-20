using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_d96d4 : GH_ScriptInstance
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
  private void RunScript(List<Curve> curves, object y, ref object geojson)
  {
    // StringBuilder를 사용하여 GeoJSON 문자열을 구성합니다.
    StringBuilder sb = new StringBuilder();
    sb.Append("{\"type\":\"FeatureCollection\",\"features\":[");
    bool firstFeature = true;

    foreach (var curve in curves)
    {
      // 커브를 일정 간격으로 샘플링하여 포인트 리스트를 생성합니다.
      var points = new List<double[]>();
      double length = curve.GetLength();
      int numPoints = (int) (length / 1.0); // 필요에 따라 샘플링 간격을 조절하세요.
      if (numPoints < 2) numPoints = 2;

      double t0 = curve.Domain.T0;
      double t1 = curve.Domain.T1;

      for (int i = 0; i <= numPoints; i++)
      {
        double t = t0 + (t1 - t0) * i / numPoints;
        Point3d pt = curve.PointAt(t);
        points.Add(new double[] { pt.X, pt.Y });
      }

      // 첫 번째 피처가 아니면 쉼표를 추가합니다.
      if (!firstFeature)
        sb.Append(",");
      else
        firstFeature = false;

      // 피처를 구성합니다.
      sb.Append("{\"type\":\"Feature\",\"geometry\":");
      sb.Append("{\"type\":\"LineString\",\"coordinates\":[");

      bool firstPoint = true;
      foreach (var pt in points)
      {
        if (!firstPoint)
          sb.Append(",");
        else
          firstPoint = false;

        sb.AppendFormat("[{0},{1}]", pt[0], pt[1]);
      }

      sb.Append("]},\"properties\":{}}");
    }

    sb.Append("]}");

    // 결과 GeoJSON 문자열을 출력으로 설정합니다.
    geojson = sb.ToString();
  }
  #endregion
  #region Additional

  #endregion
}