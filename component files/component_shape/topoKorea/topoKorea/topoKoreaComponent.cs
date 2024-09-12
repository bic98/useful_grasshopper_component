using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace topoKorea
{
    public class topoKoreaComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public topoKoreaComponent()
          : base("topo3D", "topo3D",
            "Provides 3D representations of shapefiles from the National Geographic Information Institute of Korea.",
            "topo3D", "inputData")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPathParameter("SHP folder", "SHP", "Shp file folder link", GH_ParamAccess.list); 
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("TopoLines", "TL", "topology lines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("BuildingLines", "BL", "building flat lines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("StreetLines", "SL", "street flat lines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Water Lines", "WL", "water flat lines", GH_ParamAccess.tree);
            pManager.AddPointParameter("Park Points", "P", "park pts", GH_ParamAccess.tree); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> shpFiles = new List<string>();
            if (!DA.GetDataList(0, shpFiles)) return;

            // Dictionary to store SHP files with keys starting with 'A'
            Dictionary<string, List<string>> shpFileDict = new Dictionary<string, List<string>>();

            foreach (var shpFile in shpFiles)
            {
                // Extract the filename from the path
                string fileName = System.IO.Path.GetFileNameWithoutExtension(shpFile);

                // Find the part that starts with 'A' and ends before the first underscore
                string key = fileName.Split('_').FirstOrDefault(part => part.StartsWith("A"));

                if (key != null)
                {
                    // If the key exists in the dictionary, add to the list; otherwise, create a new entry
                    if (shpFileDict.ContainsKey(key))
                    {
                        shpFileDict[key].Add(shpFile);
                    }
                    else
                    {
                        shpFileDict[key] = new List<string> { shpFile };
                    }
                }
            }
        }



        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b793de99-78cf-444d-ba4f-93c687523821");
    }
}