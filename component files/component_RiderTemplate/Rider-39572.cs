using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.IO;
using System.Diagnostics;

public class ScriptUtilities
{
  public static string GetCurrentFolder(GH_Document doc)
  {
    string filePath = doc.FilePath;
    return Path.GetDirectoryName(filePath);
  }

  public static string[] GetCsFiles(string folderPath)
  {
    return Directory.GetFiles(folderPath, "*.cs");
  }

  public static void OpenFileWithRider(string filePath, Action<string> print)
  {
    try
    {
      // Define the path to the JetBrains Rider executable
      string riderPath = @"C:\Program Files\JetBrains\JetBrains Rider 2024.1.4\bin\rider64.exe";

      if (!File.Exists(riderPath))
      {
        print("Rider executable not found at: " + riderPath);
        return;
      }

      // Start the process to open the file with Rider
      ProcessStartInfo startInfo = new ProcessStartInfo
        {
          FileName = riderPath,
          Arguments = "\"" + filePath + "\"", // Enclose in quotes in case of spaces
          UseShellExecute = true
          };
      Process.Start(startInfo);
    }
    catch (Exception ex)
    {
      print("Error opening file with Rider: " + ex.Message);
    }
  }

  public static void RunScript(bool openFile, ref object Here, IGH_Component owner, Action<string> print)
  {
    // Get the current folder path
    GH_Document doc = owner.OnPingDocument();
    string currentFolder = GetCurrentFolder(doc);
    Here = currentFolder;
    // Get .cs files in the current folder
    string[] csFiles = GetCsFiles(currentFolder);
    if (csFiles.Length > 0)
    {
      string csFilePath = csFiles[0];

      if (openFile)
      {
        OpenFileWithRider(csFilePath, print);
      }
    }
    else
    {
      print("No .cs files found in the directory.");
    }
  }
}


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_39572 : GH_ScriptInstance
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
  private void RunScript(object x, object y, bool openFile, ref object Folder, ref object A)
  {
    ScriptUtilities.RunScript(openFile, ref Folder, Component, Print);
    List<int> li = new List<int>();
    

  }
  #endregion
  #region Additional

  #endregion
}