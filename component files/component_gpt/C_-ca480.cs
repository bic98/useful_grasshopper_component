using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Text;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_ca480 : GH_ScriptInstance
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
  private void RunScript(List<Curve> curves, double thresholdLength, double bufferDistance, ref object windowCurves, ref object windowSurfaces)
  {
    // 선택된 커브들과 서피스를 저장할 리스트
    List<Curve> windowCurvesList = new List<Curve>();
    List<Brep> windowSurfacesList = new List<Brep>();

    foreach (var curve in curves)
    {
        // 커브를 Polyline으로 변환
        Polyline polyline;
        if (!curve.TryGetPolyline(out polyline))
        {
            // Polyline으로 변환되지 않는 경우, 일정 간격으로 분할하여 Polyline 생성
            double length = curve.GetLength();
            int segmentCount = (int)(length / 0.5); // 필요에 따라 분할 간격 조절
            if (segmentCount < 2) segmentCount = 2;

            Point3d[] points;
            curve.DivideByCount(segmentCount, true, out points);
            polyline = new Polyline(points);
        }

        // Polyline의 세그먼트들을 가져옴
        for (int i = 0; i < polyline.Count - 1; i++)
        {
            Line segment = new Line(polyline[i], polyline[i + 1]);
            double segmentLength = segment.Length;

            // 세그먼트의 길이가 임계값보다 큰 경우 선택
            if (segmentLength >= thresholdLength)
            {
                // 세그먼트의 방향 벡터 계산
                Vector3d direction = segment.Direction;
                direction.Unitize();

                // 세그먼트에 수직인 벡터 계산 (Z축 방향이 위쪽인 경우)
                Vector3d normal = Vector3d.CrossProduct(direction, Vector3d.ZAxis);
                normal.Unitize();
                normal *= (bufferDistance / 2.0);

                // 사각형의 네 점 계산
                Point3d p1 = segment.From + normal;
                Point3d p2 = segment.To + normal;
                Point3d p3 = segment.To - normal;
                Point3d p4 = segment.From - normal;

                // 버퍼 폴리라인 생성
                Polyline bufferPolyline = new Polyline(new List<Point3d> { p1, p2, p3, p4, p1 });
                PolylineCurve bufferCurve = new PolylineCurve(bufferPolyline);

                // 결과 커브 리스트에 추가
                windowCurvesList.Add(bufferCurve);

                // 서피스 생성
                Brep[] breps = Brep.CreatePlanarBreps(bufferCurve);
                if (breps != null && breps.Length > 0)
                {
                    windowSurfacesList.AddRange(breps);
                }
            }
        }
    }

    // 출력 설정
    windowCurves = windowCurvesList;
    windowSurfaces = windowSurfacesList;
  }
  #endregion
  #region Additional

  #endregion
}