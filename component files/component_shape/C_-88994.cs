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
using GH_IO.Types;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_88994 : GH_ScriptInstance
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
  private void RunScript(List<Curve> streetLines, Brep topo3D, object information, ref object blockLine, ref object streetLine, ref object streetFlatLine)
  {
    var regionCurve = Curve.CreateBooleanUnion(streetLines, 0.001).ToList();
    var validCurves = new ConcurrentBag<Curve>();
    var maxArea = 0.0;
    Curve largestCurve = null;
    Parallel.ForEach(regionCurve, curve =>
    {
      var area = AreaMassProperties.Compute(curve).Area;
      if (area > 100)
      {
        Plane pl;
        if (curve.TryGetPlane(out pl) && pl.Normal.Z < 0)
        {
          curve.Reverse();
        }

        validCurves.Add(curve);
        lock (regionCurve)
        {
          if (area > maxArea)
          {
            maxArea = area;
            largestCurve = curve;
          }
        }
      }
    });
    regionCurve = validCurves.Where(c => c != largestCurve).ToList(); 
    var boss = Extrude(largestCurve, 500).ToBrep();
    var block = topo3D.Split(boss, 0.01);
    var street = block.Last(); 
    var splitResults = new ConcurrentBag<Brep>();
    var brepList = new ConcurrentBag<Brep>(); 
    
    for (int i = 0; i < block.Length - 1; i++)
    {
      splitResults.Add(block[i]); 
    }
    
    Parallel.ForEach(regionCurve, curve =>
    {
      if(largestCurve.Contains(curve.PointAtStart, Plane.WorldXY, 0.01) == PointContainment.Inside)
      {
        var cutter = Extrude(curve, 1000).ToBrep(); 
        var tmp = topo3D.Split(cutter, 0.001);
        if (tmp != null && tmp.Length > 0)
        {
          splitResults.Add(tmp.Last());
        }

        brepList.Add(cutter); 
      }
    });
    
    foreach (var i in brepList)
    {
      var tmp = street.Split(i, 0.001);
      if (tmp == null || tmp.Length == 0) continue;
      street = street.Split(i, 0.001)[0];
    }

    blockLine = splitResults;
    streetLine = street; 

  }
  #endregion
  #region Additional

  public Point3d MovePt(Point3d p, Vector3d v, double amp)
  {
    var copyPt = new Point3d(p); 
    v.Unitize();
    Transform move = Transform.Translation(v * amp);
    copyPt.Transform(move);
    return copyPt;
  }
  public static Extrusion Extrude(Curve curve, double height)
  {
    return Extrusion.Create(curve, height, false); // extrusion 생성 및 반환
  }

  public T MoveOrientPoint<T>(T obj, Point3d now, Point3d nxt) where T : GeometryBase
  {
    Plane baseNow = Plane.WorldXY;
    Plane st = new Plane(now, baseNow.XAxis, baseNow.YAxis);
    Plane en = new Plane(nxt, baseNow.XAxis, baseNow.YAxis);
    Transform orient = Transform.PlaneToPlane(st, en);
    obj.Transform(orient);
    return obj;
  }
  #endregion
}