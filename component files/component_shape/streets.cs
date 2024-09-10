using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Collections.Concurrent;
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
  private void RunScript(DataTree<Curve> streets, Brep topo3D, bool Run, ref object streetLine, ref object streetFlatLine)
  {
    if (!Run) return;
    List<Curve> streetLines = new List<Curve>(); 
    var sLine = ConvertTreeToNestedList(streets);
    
    
    foreach (var i in sLine)
    {
      if (i.Count > 0) streetLines.Add(i[0]); 
    }
    
    var regionCurve = Curve.CreateBooleanUnion(streetLines, 0.001).ToList();
    var validCurves = new ConcurrentBag<Curve>();

    Parallel.ForEach(regionCurve, curve =>
      {
      var area = AreaMassProperties.Compute(curve).Area;
      if (area > 100)
      {
        var tmp = Curve.ProjectToBrep(curve, topo3D, Vector3d.ZAxis, 0.001);
        if (tmp != null && tmp.Length > 0)
        {
          foreach (var t in tmp)
          {
            validCurves.Add(t);
          }
        }
      }
      });
    streetLine = validCurves;
    streetFlatLine = regionCurve;
  }
  #endregion
  #region Additional
  public List<List<T>> ConvertTreeToNestedList<T>(DataTree<T> tree)
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