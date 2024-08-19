using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using Rhino.FileIO;
using Rhino.Geometry.Intersect;
using Rhino.PlugIns;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_7186d : GH_ScriptInstance
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
  private void RunScript(List<Curve> x, object y, ref object A, ref object B)
  {
    
      var dictPlane = MakeHashCurvePlane(x);
      List<List<Curve>> ret = new List<List<Curve>>();
      List<List<Plane>> pls = new List<List<Plane>>();
      foreach (var g in dictPlane.Values)
      {
        ret.Add(g);
      }

      foreach (var g in dictPlane)
      {
        pls.Add(new List<Plane> { g.Key });
      }

      A = MakeDataTree2D(ret);
      B = MakeDataTree2D(pls);
  }
  #endregion
  #region Additional

    public Dictionary<Plane, List<Curve>> MakeHashCurvePlane(List < Curve > crv)
    {
      Dictionary<Plane, List<Curve>> dictPlane = new Dictionary<Plane, List<Curve>>();
      List<Line> lines = new List<Line>();
      for (int i = 0; i < crv.Count; i++)
      {
        Curve now = crv[i];
        Plane p;

        if (now.IsLinear())
        {
          lines.Add(new Line(now.PointAtStart, now.PointAtEnd));
        }
        else if (now.IsPlanar())
        {
          if (FindPlane(dictPlane.Keys.ToList(), now, out p))
          {
            dictPlane[p].Add(now);
          }
          else
          {
            now.TryGetPlane(out p);
            dictPlane[p] = new List<Curve> { now };
          }
        }
      }

      List<bool> vis = Enumerable.Repeat(false, lines.Count).ToList();
      for (int i = 0; i < lines.Count; i++)
      {
        if (vis[i] == true) continue;
        for (int j = 0; j < lines.Count; j++)
        {
          if (vis[j] == true || (i == j)) continue;
          var ccw = Intersection.CurveCurve(lines[i].ToNurbsCurve(), lines[j].ToNurbsCurve(), 0.1, 0.0);
          if (ccw.Count > 0)
          {
            Plane p;
            if (FindPlane(dictPlane.Keys.ToList(), lines[i].ToNurbsCurve(), out p))
            {
              dictPlane[p].Add(lines[i].ToNurbsCurve());
              dictPlane[p].Add(lines[j].ToNurbsCurve());
            }
            else
            {
              p = new Plane(lines[i].From, lines[i].Direction, lines[j].Direction);
              dictPlane[p] = new List<Curve> { lines[i].ToNurbsCurve(), lines[j].ToNurbsCurve() };
            }

            vis[j] = true;
            vis[i] = true;
            break;
          }
        }
      }

      for (int i = 0; i < lines.Count; i++)
      {
        Print(vis[i].ToString());
        if (vis[i] == false)
        {
          Plane p;
          if (FindPlane(dictPlane.Keys.ToList(), lines[i].ToNurbsCurve(), out p))
          {
            dictPlane[p].Add(lines[i].ToNurbsCurve());
          }
        }
      }

      return dictPlane;
    }

    public bool FindPlane(List < Plane > planes, Curve crv, out Plane p)
    {
      bool find = false;
      p = Plane.Unset;
      foreach (var pl in planes)
      {
        double dist1 = Math.Abs(pl.DistanceTo(crv.PointAtStart));
        double dist2 = Math.Abs(pl.DistanceTo(crv.PointAtEnd));
        if (dist1 < 0.01 && dist2 < 0.01)
        {
          find = true;
          p = pl;
          break;
        }
      }

      return find;
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
  #endregion
}