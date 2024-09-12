using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace topoKorea
{
    public class topoKoreaInfo : GH_AssemblyInfo
    {
        public override string Name => "topoKorea";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("ffde8e1a-7e7b-414f-a3c1-395be0e63db3");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}