using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.IO.Pipes;
using System.Linq;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_a3353 : GH_ScriptInstance
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
  private void RunScript(List<Curve> x, int y, int z, int w, int u, ref object A)
  {
    List<List<Point3d>> llp = new List<List<Point3d>>();
    for (int i = 0; i < 3; i++)
    {
      llp.Add(DividePts(x[i], 2, true)); 
    }

    List<List<List<Surface>>> llls = new List<List<List<Surface>>>();
    for (int i = 0; i < 3; i++)
    {
      List<List<Surface>> lls = new List<List<Surface>>();
      for (int j = 0; j < 3; j++)
      {
        Curve c = new Circle(llp[i][j], y).ToNurbsCurve();
        List<Point3d> lp = DividePts(c, z, true);
        List<Surface> ls = new List<Surface>();
        for (int k = 0; k < lp.Count; k++)
        {
          Curve tmp = new Circle(lp[k], 1).ToNurbsCurve(); 
          Surface s = Surface.CreateExtrusion(tmp, new Vector3d(0, 0, w));
          ls.Add(s); 
        }

        w += u; 
        lls.Add(ls); 
      }

      llls.Add(lls); 
    }

    A = MakeDataTree3D(llls); 
  }
  #endregion
  #region Additional

  public static List<Point3d> DividePts(Curve crv, int cnt, bool end)
  {
    List<Point3d> ret = new List<Point3d>();
    var crvT = crv.DivideByCount(cnt, end);
    for (int i = 0; i < crvT.Length; i++)
    {
      var pt = crv.PointAt(crvT[i]);
      ret.Add(pt);
    }

    return ret;
  }

  public static DataTree<T> MakeDataTree3D<T>(List<List<List<T>>> ret)
  {
    DataTree<T> tree = new DataTree<T>();
    for (int i = 0; i < ret.Count; i++)
    {
      for (int j = 0; j < ret[i].Count; j++)
      {
        for (int k = 0; k < ret[i][j].Count; k++)
        {
          GH_Path path = new GH_Path(0, i, j);
          tree.Add(ret[i][j][k], path);
        }
      }
    }

    return tree;
  }
  #endregion
}