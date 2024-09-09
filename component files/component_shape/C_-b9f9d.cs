using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;



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
 
    // List<Brep> breps = new List<Brep>(); 
    // foreach (var crv in regionCurve)
    // {
    //   Plane pl; 
    //   crv.TryGetPlane(out pl);
    //   if(pl.ZAxis.Z < 0) crv.Reverse();
    //   var brep = Extrude(crv, 300).ToBrep();
    //   breps.Add(brep); 
    // }
    
    regionCurve = regionCurve.Where(x => AreaMassProperties.Compute(x).Area > 100).ToList();
    regionCurve.Sort((x, y) => AreaMassProperties.Compute(x).Area.CompareTo(AreaMassProperties.Compute(y).Area));
    List<Brep> breps = new List<Brep>();
    
    Plane pl;
    var crv = regionCurve[0]; 
    crv.TryGetPlane(out pl); 
    if(pl.ZAxis.Z < 0) crv.Reverse(); 
    var brep = Extrude(crv, 300).ToBrep();
    var splitBreps = topo3D.Split(brep, 0.001); 
    
    foreach (var curve in regionCurve)
    {
      double area = AreaMassProperties.Compute(curve).Area;
      Print(area.ToString()); 
      
    }

    
    A = regionCurve;
    B = splitBreps; 
  }
  #endregion
  #region Additional
  public static Extrusion Extrude(Curve curve, double height)
  {
    return Extrusion.Create(curve, height, false); // extrusion 생성 및 반환
  }
  #endregion
}