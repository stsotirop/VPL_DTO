using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace VPL_DTO
{
    public class GhcFEMProperties : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhsFEMProperties class.
        /// </summary>
        public GhcFEMProperties()
          : base("FEM Prop", "FEM Prop",
              "FEM Properties",
              "VPLDynaTop", "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("ObjFunc", "ObjFunc", "Objective Function", GH_ParamAccess.item);
            pManager.AddNumberParameter("T", "T", "Simulation Time", GH_ParamAccess.item);
            pManager.AddNumberParameter("M", "M", "Mass Magnitude", GH_ParamAccess.item);
            pManager.AddNumberParameter("E", "E", "Young's modulus", GH_ParamAccess.item);
            pManager.AddNumberParameter("n", "n", "Poisson ration", GH_ParamAccess.item);
            pManager.AddNumberParameter("p", "p", "Mass density", GH_ParamAccess.item);
            pManager.AddNumberParameter("th", "th", "Element's thickness", GH_ParamAccess.item);
            pManager.AddNumberParameter("ar", "ar", "Rayleigh damping ar", GH_ParamAccess.item);
            pManager.AddNumberParameter("br", "br", "Rayleigh damping br", GH_ParamAccess.item);
            pManager.AddNumberParameter("Reg", "Reg", "Tag for regular meshes, 0-no 1-yes", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("FEM", "FEM", "Parameters to define the FEM problem", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double ObjFunc = new double();
            DA.GetData("ObjFunc", ref ObjFunc);
            double T = new double();
            DA.GetData("T", ref T);
            double M = new double();
            DA.GetData("M", ref M);
            double E = new double();
            DA.GetData("E", ref E);
            double n = new double();
            DA.GetData("n", ref n);
            double p = new double();
            DA.GetData("p", ref p);
            double th = new double();
            DA.GetData("th", ref th);
            double ar = new double();
            DA.GetData("ar", ref ar);
            double br = new double();
            DA.GetData("br", ref br);
            double Reg = new double();
            DA.GetData("Reg", ref Reg);

            List<double> FEM = new List<double>();
            FEM.Add(ObjFunc); FEM.Add(T); FEM.Add(M); FEM.Add(E); FEM.Add(n);
            FEM.Add(p); FEM.Add(th); FEM.Add(ar); FEM.Add(br); FEM.Add(Reg);
            DA.SetDataList("FEM", FEM);
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
            get { return new Guid("4b78111f-c751-4ea0-a1fc-8b7878b5d8e7"); }
        }
    }
}