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
public abstract class Script_Instance_b9f9d : GH_ScriptInstance
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
  private void RunScript(List<Curve> streetLines, Brep topo3D, ref object A, ref object B, ref object C)
  {
    var regionCurve = Curve.CreateBooleanUnion(streetLines, 0.001).ToList(); 
    List<Curve> pulledCurves = new List<Curve>();
    List<Brep> breps = new List<Brep>(); 
    object lockObject = new object(); // 동기화를 위한 객체
    ParallelOptions parallelOptions = new ParallelOptions();
    parallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount; // 모든 프로세서 사용

    Parallel.For(0, regionCurve.Count, parallelOptions, i =>
    {
      var crv = regionCurve[i];
      Curve[] upperCrv = null;
      Brep[] brep = null;

      try
      {
        // 커브를 브렙에 투영
        upperCrv = Curve.ProjectToBrep(crv, topo3D, Vector3d.ZAxis, 0.001);
        
        // 투영된 커브로 브렙을 분할
        if (upperCrv != null && upperCrv.Length > 0)
        {
          brep = topo3D.Split(upperCrv, 0.01);

          if (brep != null && brep.Length > 0)
          {
            lock (lockObject)
            {
              breps.Add(brep.Last());
              pulledCurves.AddRange(upperCrv);
            }
          }
        }
      }
      finally
      {
        // 메모리 해제
        if (upperCrv != null)
        {
          foreach (var curve in upperCrv)
          {
            curve.Dispose();
          }
        }

        if (brep != null)
        {
          foreach (var b in brep)
          {
            b.Dispose();
          }
        }
      }
    });
    A = pulledCurves;
    B = breps; 
  }
  #endregion
  #region Additional

  #endregion
}