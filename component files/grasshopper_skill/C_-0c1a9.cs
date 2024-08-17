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
public abstract class Script_Instance_0c1a9 : GH_ScriptInstance
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
  private void RunScript(List<Line> x, object y, ref object A)
  {
    //divide the line by 2 square
    List<List<Point3d>> pt = new List<List<Point3d>>();
    int cnt = 2;
    for (int i = 0; i < x.Count; i++)
    {
      Curve tmp = x[i].ToNurbsCurve(); 
      var li = tmp.DivideByCount(cnt, true);
      List<Point3d> loc = new List<Point3d>();
      //choose odd index and input the loc List
      for (int j = 1; j < li.Length; j += 2)
      {
        loc.Add(tmp.PointAt(li[j])); 
      }
      pt.Add(loc);
      cnt *= 2; 
    }
    
    //I want to get for the answer
    List<List<Line>> ans = new List<List<Line>>(); 
    
    
    //Select some Point to others points
    for (int i = 0; i < x.Count - 1; i++)
    {
      List<Point3d> st = pt[i];
      List<Point3d> en = pt[i + 1];
      List<Line> liL = new List<Line>(); 
      for (int j = 0; j < st.Count; j++)
      {
        Point3d now = st[j];
        en.Sort((u, v) => u.DistanceTo(now).CompareTo(v.DistanceTo(now)));
        for (int k = 0; k < 2; k++)
        {
          liL.Add(new Line(now, en[k])); 
        }
      }

      ans.Add(liL); 
    }

    var ptLoc = makeDataTree(pt);
    var liLoc = makeDataTree(ans); 
    A = liLoc; 
  }
  #endregion
  #region Additional

  public static DataTree<T> makeDataTree<T>(List<List<T>> ret)
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
  #endregion
}