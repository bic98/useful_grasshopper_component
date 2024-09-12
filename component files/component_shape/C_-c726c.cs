using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using DotSpatial.Projections; 
using DotSpatial.Topology;
using DotSpatial.Mono;
using DotSpatial.Data;

using System.Linq;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_c726c : GH_ScriptInstance
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
  private void RunScript(List<string> shpFolders, string y, ref object A, ref object B)
  {
// Dictionary to store SHP files with keys extracted from the filename, using HashSet to avoid duplicates
    Dictionary<string, HashSet<string>> shpFileDict = new Dictionary<string, HashSet<string>>();

    foreach (string i in shpFolders)
    {
      string folderPath = i;
      if (folderPath.Length > 2 && folderPath[0] == '\"' && folderPath[folderPath.Length - 1] == '\"')
      {
        folderPath = folderPath.Substring(1, folderPath.Length - 2);
      }

      if (System.IO.Directory.Exists(folderPath))
      {
        // Get all .shp files in the directory
        string[] shpFiles = System.IO.Directory.GetFiles(folderPath, "*.shp");
        foreach (var shpFile in shpFiles)
        {
          // Extract the filename from the path
          string fileName = System.IO.Path.GetFileNameWithoutExtension(shpFile);
          // Split the filename by underscore and convert to a list
          List<string> parts = fileName.Split('_').ToList();

          // The key is the last part of the filename
          if (parts.Count > 0)
          {
            string key = parts[parts.Count - 1]; // Get the last element

            // If the key exists in the dictionary, add to the HashSet; otherwise, create a new entry
            if (shpFileDict.ContainsKey(key))
            {
              shpFileDict[key].Add(shpFile); // HashSet prevents duplicates
            }
            else
            {
              shpFileDict[key] = new HashSet<string> { shpFile };
            }
          }
        }
      }
    }

    var tmp = GetFiles(y, shpFileDict)[0];
    var tmp1 = Shapefile.OpenFile(tmp, null);
    var atr = tmp1.Attributes.Columns;
    B = atr; 
    
    A = GetFiles(y, shpFileDict);

  }
  #endregion
  #region Additional

  static List<string> GetFiles(string key, Dictionary<string, HashSet<string>> hs)
  {
    List<string> resultList;
    if (hs.ContainsKey(key))
    {
      resultList = hs[key].ToList(); // Convert HashSet to List<string>
    }
    else
    {
      resultList = new List<string>(); // Handle the case where the key 'y' is not found
    }

    return resultList; 
  }
  #endregion
}