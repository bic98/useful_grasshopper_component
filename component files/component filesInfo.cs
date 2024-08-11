using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace component_files
{
    public class component_filesInfo : GH_AssemblyInfo
    {
        public override string Name => "component_files";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("8863b73c-bef1-4dc6-b18e-c3db44b330e4");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}