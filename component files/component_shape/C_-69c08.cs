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
using System.Threading;
using Rhino.Geometry.Intersect;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_69c08 : GH_ScriptInstance
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
  private void RunScript(DataTree<Curve> buildingLine, DataTree<double> buildingHeight, Surface topo3D, bool Run, ref object A, ref object B, ref object C, ref object D)
  {
    if (!Run) return; 
    List<List<Curve>> buildingLines = ConvertTreeToNestedList(buildingLine);
    List<List<double>> buildingHeights = ConvertTreeToNestedList(buildingHeight);
    
    List<List<Curve>> pureLines = new List<List<Curve>>();
    List<List<Point3d>> purePoints = new List<List<Point3d>>();
    List<List<Point3d>> nxtPurePoints = new List<List<Point3d>>();
    
    var raySrf = new List<GeometryBase>() { topo3D.ToBrep() };
    List<List<double>> pureHeights = new List<List<double>>();
    List<List<double>> deem = new List<List<double>>(); 
    List<List<Curve>> parapets = new List<List<Curve>>();
    
    
    for (int i = 0; i < buildingLines.Count; i++)
    {
      var now = buildingLines[i][0];
      var disconNow = Discontinuity(now);
      List<Point3d> rayPts = new List<Point3d>(); 
      
      foreach (var disconPts in disconNow)
      {
        Ray3d ray = new Ray3d(new Point3d(disconPts.X, disconPts.Y, disconPts.Z - 100.0), Vector3d.ZAxis); 
        var pts = Intersection.RayShoot(ray, raySrf, 1).ToList();
        if (pts.Count != 0) rayPts.Add(pts[0]); 
      }

      if (rayPts.Count != disconNow.Count) continue;
      double minDist = 1e9, maxDist = -1e9; 
      int id = -1; 
      for (int j = 0; j < rayPts.Count; j++)
      {
        var dist = rayPts[j].DistanceTo(disconNow[j]);
        if (dist < minDist)
        {
          minDist = dist;
          id = j; 
        }

        if (dist > maxDist)
        {
          maxDist = dist; 
        }
      }

      nxtPurePoints.Add(new List<Point3d>() { rayPts[id] });
      purePoints.Add(new List<Point3d>(){disconNow[id]});
      
      
  
 
      
      
      
      pureLines.Add(buildingLines[i]); 
      pureHeights.Add(buildingHeights[i]);
      deem.Add(new List<double>(){maxDist - minDist}); 
    }
    
    List<List<Curve>> moveLines = new List<List<Curve>>();
    List<List<Brep>> makeBuildings = new List<List<Brep>>();
    List<List<Brep>> makeBuildingsParapet = new List<List<Brep>>(); 
    
    for (int i = 0; i < pureLines.Count; i++)
    {
      var now = pureLines[i][0];
      Plane pl; 
      now.TryGetPlane(out pl);
      if(pl.ZAxis.Z < 0) now.Reverse();
      var nxt = MoveOrientPoint(now, purePoints[i][0], nxtPurePoints[i][0]); 
      moveLines.Add(new List<Curve>(){nxt});
      makeBuildings.Add(new List<Brep>(){Extrude(nxt, deem[i][0] + 3.5 * pureHeights[i][0]).ToBrep()});
      
      List<Brep> faces = new List<Brep>();
      Curve nxtUpper = nxt.DuplicateCurve();
      nxtUpper = MoveOrientPoint(nxtUpper, nxtPurePoints[i][0],
        MovePt(nxtPurePoints[i][0], Vector3d.ZAxis, deem[i][0] + 3.5 * pureHeights[i][0])); 
      //brep1
      faces.Add(NurbsSurface.CreateRuledSurface(nxt, nxtUpper).ToBrep());
      
      
      nxtUpper.TryGetPlane(out pl);
      var nxtUpperParapet = OffsetCurve(nxtUpper, pl, -0.3);
      if(nxtUpperParapet == null) continue;
      faces.Add(nxtUpperParapet);
      
      // Curve lowerParapet = nxtUpperParapetDepth.DuplicateCurve(); 
      // lowerParapet = MoveOrientPoint(lowerParapet, nxtUpperParapetDepth.PointAtEnd,
      //   MovePt(nxtUpperParapetDepth.PointAtEnd, -Vector3d.ZAxis, 1.3)); 
      //


      makeBuildingsParapet.Add(faces);
    }

    
    
    B = MakeDataTree2D(nxtPurePoints);
    A = MakeDataTree2D(makeBuildingsParapet);
    D = MakeDataTree2D(makeBuildings); 
    C = MakeDataTree2D(parapets);
  }
  #endregion
  #region Additional

 
  public Point3d MovePt(Point3d p, Vector3d v, double amp)
  {
    v.Unitize();
    Transform move = Transform.Translation(v * amp);
    p.Transform(move);
    return p;
  }

  public Brep PlanarSrf(Curve c)
  {
    string log;
    Brep ret = null;
    if (c.IsValidWithLog(out log) && c.IsPlanar() && c.IsClosed)
    {
      var tmp = Brep.CreatePlanarBreps(c, 0.01);
      ret = tmp[0]; 
      return ret;
    }

    return ret;
  }
  public Brep OffsetCurve(Curve c, Plane p, double interval)
  {
    var seg = c.DuplicateSegments();
    var joinseg = Curve.JoinCurves(seg);
    List<Curve> outLines = new List<Curve>();
    for (int i = 0; i < joinseg.Length; i++)
    {
      var offset = joinseg[i].Offset(p, interval, 0.001, CurveOffsetCornerStyle.Sharp);
      if (offset == null || offset.Length == 0) continue;
      outLines.AddRange(offset);
    }
    var ret = Curve.JoinCurves(outLines);
    if(ret == null || ret.Length == 0) return null;
    var boundary = PlanarSrf(c);
    var outBrep = boundary.Split(ret, 0.001).ToList();
    if(outBrep.Count == 0) return null;
    return outBrep[0];
  }
  public static Extrusion Extrude(Curve curve, double height)
  {
    Extrusion extrusion = Extrusion.Create(curve, height, true);
    return extrusion;
  }
  public List<Point3d> Discontinuity(Curve x)
  {
    var seg = x.DuplicateSegments();
    List<Point3d> pts = new List<Point3d>();
    for (int i = 0; i < seg.Length; i++)
    {
      if (i == 0) pts.Add(seg[i].PointAtStart);
      pts.Add(seg[i].PointAtEnd);
    }

    if (x.IsClosed) pts.RemoveAt(pts.Count - 1);
    return pts;
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
  public static DataTree<T> MakeDataTree2D<T>(List<List<T>> ret)
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
  public List<List<T>> ConvertTreeToNestedList<T>(DataTree<T> tree)
  {
    List<List<T>> nestedList = new List<List<T>>();
    foreach (GH_Path path in tree.Paths)
    {
      List<T> subList = new List<T>(tree.Branch(path));
      nestedList.Add(subList);
    }

    return nestedList;
    ;
  }
  #endregion
}