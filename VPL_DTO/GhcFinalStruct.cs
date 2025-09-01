using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace VPL_DTO
{
    public class GhcFinalStruct : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcFinalStruct class.
        /// </summary>
        public GhcFinalStruct()
          : base("FinalStruct",
                 "FinalStruct",
                 "Run FinalStruct",
                 "VPLDynaTop",
                 "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Limit", "Limit", "Limit Density", GH_ParamAccess.item);
            pManager.AddNumberParameter("Densities", "Densities", "Densities of final structure", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Indexes", "Indexes", "Indexes of existing elements", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double limit = new double();
            DA.GetData("Limit", ref limit);
            List<double> densities = new List<double>();
            DA.GetDataList("Densities", densities);
            List<int> indexes = new List<int>();
            for (int i = 0; i < densities.Count; i++)
            {
                if (densities[i] >= limit)
                {
                    indexes.Add(i + 1);
                }
            }

            DA.SetDataList("Indexes", indexes);
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
            get { return new Guid("67eb018b-49f7-4f58-be13-8ec6b25257aa"); }
        }
    }
}