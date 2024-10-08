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
using System.Net;
using System.Text;
using Newtonsoft.Json;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public abstract class Script_Instance_1014d : GH_ScriptInstance
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
  private void RunScript(string prompt, string apiKey, ref object A)
  {
        // Set up the request URL and headers
    string url = "https://api.openai.com/v1/chat/completions";
    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
    request.Method = "POST";
    request.ContentType = "application/json";
    request.Headers["Authorization"] = "Bearer " + apiKey;

    // System message to instruct the model to produce valid JSON
    var systemMessage = new
      {
        role = "system",
        content = "Your task is to ensure that the incoming JSON-like object is returned as a valid JSON object."
        };

    // Payload with the prompt and system message
    var payload = new
      {
        model = "gpt-3.5-turbo-0125", // Replace with your preferred model
        response_format = new { type = "json_object" },
        messages = new[]
        {
          systemMessage,
          new
          {
            role = "user",
            content = "Please ensure the JSON-like input is correct and return a valid version of it. The JSON-like object: " + prompt
            }
          }
        };

    // Convert the payload to JSON and then to bytes
    string jsonData = JsonConvert.SerializeObject(payload);
    byte[] bytesData = Encoding.UTF8.GetBytes(jsonData);

    // Write the data to the request stream
    using (Stream requestStream = request.GetRequestStream())
    {
      requestStream.Write(bytesData, 0, bytesData.Length);
    }


  }
  #endregion
  #region Additional

  #endregion
}