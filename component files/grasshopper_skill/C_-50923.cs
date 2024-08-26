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
public abstract class Script_Instance_50923 : GH_ScriptInstance
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
  private void RunScript(List<Point3d> pts, int near, int st, int end, ref object A, ref object B, ref object C, ref object D, ref object E)
  {
    
    
    
    List<Tuple<double, Point3d>>[] adj = new List<Tuple<double, Point3d>>[pts.Count];
    List<List<Curve>> li = new List<List<Curve>>();
    Dictionary<int, Point3d> dictPt = new Dictionary<int, Point3d>();
    List<Point3d> p = new List<Point3d>();
    int t = 0; 
    
    
    Print(pts.Count.ToString());
    foreach (var now in pts)
    {
      p.Add(now); 
      dictPt[t] = now; 
      List<Point3d> nxtPts = CloPts(pts, now, near); 
      List<Tuple<double, Point3d>> tmp = new List<Tuple<double, Point3d>>();
      List<Curve> tmp2 = new List<Curve>(); 
      for (int j = 0; j < nxtPts.Count; j++)
      {
        var nxtPt = nxtPts[j]; 
        double dist = now.DistanceTo(nxtPt);
        tmp.Add(Tuple.Create(dist, nxtPt));
        tmp2.Add(new Line(now, nxtPt).ToNurbsCurve());
      }

      li.Add(tmp2); 
      adj[t++] = tmp;
    }
    
    Point3d startPoint = dictPt[st];
    Point3d endPoint = dictPt[end];

    Dictionary<Point3d, Point3d> par;
    var distances = Dijkstra(adj, startPoint, pts, out par);

    // Reconstruct the shortest path from endPoint to startPoint
    List<Line> pathLines = new List<Line>();
    Point3d current = endPoint;

    while (current != startPoint)
    {
      Point3d parent = par[current];
      pathLines.Add(new Line(parent, current));
      current = parent;
    }

    // Reverse the path to go from startPoint to endPoint
    pathLines.Reverse();

    // Output the path as a list of lines
    A = pathLines;
    B = MakeDataTree2D(li);
    C = startPoint;
    D = endPoint;
    E = p; 

  }
  #endregion
  #region Additional

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
  public List<Point3d> CloPts(List<Point3d> pts, Point3d now, int cnt)
  {
    pts.Sort((u, v) => u.DistanceTo(now).CompareTo(v.DistanceTo(now)));
    List<Point3d> ans = new List<Point3d>();

    for (int i = 0; i < pts.Count; i++)
    {
      if (ans.Count == cnt) break;
      if (pts[i] == now) continue;
      ans.Add(pts[i]);
    }

    return ans;
  }
  
  public Dictionary<Point3d, double> Dijkstra(List<Tuple<double, Point3d>>[] adj, Point3d p, List<Point3d> pts, out Dictionary<Point3d, Point3d> par)
    {
        Dictionary<Point3d, double> dist = new Dictionary<Point3d, double>();
        par = new Dictionary<Point3d, Point3d>();
        for (int i = 0; i < pts.Count; i++)
        {
            dist[pts[i]] = double.MaxValue; // Initialize all distances as infinity
            par[pts[i]] = Point3d.Unset; // Initialize the parent as an unset point
        }
        dist[p] = 0; // Distance to the source is 0
        par[p] = p; // The parent of the start node is itself
        var pq = new SortedSet<Tuple<double, Point3d>>(Comparer<Tuple<double, Point3d>>.Create((a, b) =>
        {
            int result = a.Item1.CompareTo(b.Item1);
            return result == 0 ? a.Item2.GetHashCode().CompareTo(b.Item2.GetHashCode()) : result;
        }));
        pq.Add(Tuple.Create(0.0, p));

        // Main loop of Dijkstra's algorithm
        while (pq.Count > 0)
        {
            var current = pq.Min;
            pq.Remove(current);

            double currentDist = current.Item1;
            Point3d currentPoint = current.Item2;

            // Get the index of the current point
            int currentIndex = pts.IndexOf(currentPoint);

            // Explore each neighbor of the current point
            foreach (var neighbor in adj[currentIndex])
            {
                double edgeDist = neighbor.Item1;
                Point3d neighborPoint = neighbor.Item2;

                // Calculate the potential new shortest distance
                double newDist = currentDist + edgeDist;

                // If the new distance is shorter, update and enqueue the neighbor
                if (newDist < dist[neighborPoint])
                {
                    pq.Remove(Tuple.Create(dist[neighborPoint], neighborPoint));
                    dist[neighborPoint] = newDist;
                    par[neighborPoint] = currentPoint; // Update the parent to the current point
                    pq.Add(Tuple.Create(newDist, neighborPoint));
                }
            }
        }

        // Return the dictionary of shortest distances to each point
        return dist;
    }
  #endregion
}