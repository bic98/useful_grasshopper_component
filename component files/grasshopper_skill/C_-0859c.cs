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
public abstract class Script_Instance_0859c : GH_ScriptInstance
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
  private void RunScript(Surface x, int y, ref object A, ref object B, ref object C)
  {
    var dsd = DivideSurfaceDomain(x, y, y);
    List<Surface> ret1 = new List<Surface>(); 
    List<Surface> ret2 = new List<Surface>();
    for (int i = 0; i < dsd.Count; i++)
    {
      List<Surface> temp = new List<Surface>();
      for (int j = 0; j < dsd[i].Count; j++)
      {
        if (j < i) ret1.Add(dsd[i][j]); 
        else ret2.Add(dsd[i][j]);
      }
    }

    A = ret1;
    B = ret2;
    C = dsd; 
  }
  #endregion
  #region Additional
  
  public List<List<Surface>> DivideSurfaceDomain(Surface srf, int uCount, int vCount)
  {
    List<List<Surface>> dividedSurfaces = new List<List<Surface>>();
    // Calculate U intervals
    List<Interval> uIntervals = CreateIntervals(srf.Domain(0), uCount);
    // Calculate V intervals
    List<Interval> vIntervals = CreateIntervals(srf.Domain(1), vCount);
    // Create subsurfaces
    foreach (var uInterval in uIntervals)
    {
      List<Surface> subSurfaces = new List<Surface>();
      foreach (var vInterval in vIntervals)
      {
        Surface subSurface = srf.Trim(uInterval, vInterval);
        if (subSurface != null)
        {
          subSurfaces.Add(subSurface);
        }
      }
      dividedSurfaces.Add(subSurfaces);
    }

    return dividedSurfaces;
  }

  private List<Interval> CreateIntervals(Interval domain, int count)
  {
    List<Interval> intervals = new List<Interval>();
    double start = domain.T0;
    double step = (domain.T1 - domain.T0) / count;

    for (int i = 0; i < count; i++)
    {
      double end = start + step;
      intervals.Add(new Interval(start, end));
      start = end;
    }

    return intervals;
  } 
  
  #endregion
}