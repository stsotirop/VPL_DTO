using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace VPL_DTO
{
    public class GhcOPTProperties : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhsOPTProperties class.
        /// </summary>
        public GhcOPTProperties()
          : base("OPT Prop", "OPT Prop",
              "OPT Properties",
              "VPLDynaTop", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("R", "R", "Radius filter", GH_ParamAccess.item);
            pManager.AddNumberParameter("V", "V", "Volume fraction", GH_ParamAccess.item);
            pManager.AddNumberParameter("Sym", "Sym", "Axis of Symmetry, 0-no 1-X 2-Y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iter", "Iter", "Maximum number of Iterations", GH_ParamAccess.item);
            pManager.AddNumberParameter("LastIter", "LastIter", "Maximum number of Iterations in the last step", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("OPT", "OPT", "Parameters to define the OPT problem", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double R = new double();
            DA.GetData("R", ref R);
            double V = new double();
            DA.GetData("V", ref V);
            double Sym = new double();
            DA.GetData("Sym", ref Sym);
            double Iter = new double();
            DA.GetData("Iter", ref Iter);
            double LastIter = new double();
            DA.GetData("LastIter", ref LastIter);

            List<double> OPT = new List<double>();
            OPT.Add(R); OPT.Add(V); OPT.Add(Sym); OPT.Add(Iter); OPT.Add(LastIter);
            DA.SetDataList("OPT", OPT);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1d1bbe43-1bd9-40b7-93e8-ca829b8ba635"); }
        }
    }
}