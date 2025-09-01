using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MathWorks.MATLAB.NET.Arrays;
using VPL_DTO_mat;

namespace VPL_DTO
{
    public class GhcDynaTop : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcDynaTop class.
        /// </summary>
        public GhcDynaTop()
          : base("DTO_MATLAB",
                 "DTO_MATLAB",
                 "Run VPL Dynamic Topology",
                 "VPLDynaTop",
                 "Utilities")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Node", "Node", "Node", GH_ParamAccess.list);
            pManager.AddPointParameter("Conn", "Conn", "Conn", GH_ParamAccess.tree);
            pManager.AddPointParameter("SupportNode", "SupportNode", "SupportNode", GH_ParamAccess.list);
            pManager.AddPointParameter("MassNode", "MassNode", "MassNode", GH_ParamAccess.list);
            pManager.AddNumberParameter("FEM", "FEM", "Parameters to define the FEM problem", GH_ParamAccess.list);
            pManager.AddNumberParameter("OPT", "OPT", "Parameters to define the OPT problem", GH_ParamAccess.list);
            pManager.AddTextParameter("ag_FilePath", "ag_FilePath", "ag_FilePath", GH_ParamAccess.item);
            pManager.AddTextParameter("FilePath", "FilePath", "FilePath", GH_ParamAccess.item);
            pManager.AddBooleanParameter("RunTop", "RunTop", "RunTop", GH_ParamAccess.item);
            pManager.AddPointParameter("PassiveEl", "PassiveEl", "PassiveEl", GH_ParamAccess.tree);
            pManager[9].Optional = true;
            pManager.AddPointParameter("G1", "G1", "G1", GH_ParamAccess.tree);
            pManager[10].Optional = true;
            pManager.AddPointParameter("G2", "G2", "G2", GH_ParamAccess.tree);
            pManager[11].Optional = true;
            pManager.AddPointParameter("G3", "G3", "G3", GH_ParamAccess.tree);
            pManager[12].Optional = true;
            pManager.AddPointParameter("G4", "G4", "G4", GH_ParamAccess.tree);
            pManager[13].Optional = true;
            pManager.AddPointParameter("G5", "G5", "G5", GH_ParamAccess.tree);
            pManager[14].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Densities", "Densities", "Densities of final structure", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool runTop = false;
            DA.GetData("RunTop", ref runTop);
            HelperFunctions tempHelper = new HelperFunctions();

            if (runTop)
            {
                string folderpath = null;
                DA.GetData("FilePath", ref folderpath);
                List<double> FEM = new List<double>();
                DA.GetDataList("FEM", FEM);
                List<double> OPT = new List<double>();
                DA.GetDataList("OPT", OPT);

                // Get ag ground acceleration
                string ag_filename = null;
                DA.GetData("ag_FilePath", ref ag_filename);
                String ag_file = File.ReadAllText(ag_filename);
                List<double> ag_list = new List<double>();
                foreach (var row in ag_file.Split('\n'))
                {
                    foreach (var col in row.Trim().Split('\t'))// '\t' tab, ' ' keno
                    {
                        ag_list.Add(double.Parse(col.Trim()));
                    }
                }

                // Get All nodes
                List<double[]> total_nodes_temp = new List<double[]>();
                List<Point3d> iNode = new List<Point3d>();
                DA.GetDataList("Node", iNode);
                for (int i = 0; i < iNode.Count; i++)
                {
                    double[] tempArrow1 = new double[3] { Math.Round(iNode[i].X, 6), Math.Round(iNode[i].Y, 6), Math.Round(iNode[i].Z, 6) };
                    total_nodes_temp.Add(tempArrow1);
                }

                // Eliminate doubles in nodes
                Dictionary<int, double[]> total_nodes = new Dictionary<int, double[]>();
                var distinct = total_nodes_temp.Distinct(new DistinctDoubleArrayComparer());
                int node_id = 1;
                foreach (var item in distinct)
                {
                    double[] tempArrow = new double[3] { item[0], item[1], item[2] };
                    total_nodes.Add(node_id, tempArrow);
                    node_id++;
                }

                // Write help function
                string pathFolder = Path.GetDirectoryName(folderpath);
                string helpPath = pathFolder + "\\log.txt";
                StreamWriter sw = new StreamWriter(helpPath);

                // Get all connectivity
                GH_Structure<GH_Point> iConn;
                if (!DA.GetDataTree("Conn", out iConn)) return;
                Dictionary<int, int[]> my_conn = new Dictionary<int, int[]>();
                int max_num_of_points = 0;
                int myFlag = 0;
                for (int i = 0; i < iConn.Branches.Count; i++)
                {
                    List<GH_Point> branch = iConn.Branches[i];
                    int num_of_points = branch.Count;
                    if (num_of_points > max_num_of_points)
                    {
                        max_num_of_points = num_of_points;
                    }
                    double[,] pointsArray = new double[num_of_points, 3];
                    int[] pointsID = new int[num_of_points];
                    double[] tempPoint;
                    for (int j = 0; j < num_of_points; j++)
                    {
                        Point3d thisPoint = branch[j].Value;
                        pointsArray[j, 0] = Math.Round(thisPoint.X, 6);
                        pointsArray[j, 1] = Math.Round(thisPoint.Y, 6);
                        pointsArray[j, 2] = Math.Round(thisPoint.Z, 6);
                        tempPoint = new double[3] { pointsArray[j, 0], pointsArray[j, 1], pointsArray[j, 2] };
                        int myKey = 0;
                        foreach (var item in total_nodes)
                        {
                            if (tempPoint.SequenceEqual(item.Value))
                            {
                                myKey = item.Key;
                                break;
                            }
                        }
                        pointsID[j] = myKey;
                        if (myKey == 0)
                        {
                            sw.WriteLine("In the face " + i.ToString() + " the node " + j.ToString() + " is not in the list of the total nodes");

                            myFlag = 1;
                        }
                    }
                    Array.Reverse(pointsID);
                    my_conn.Add(i + 1, pointsID);
                }
                sw.Close();
                if (myFlag == 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Some nodes in the connectivity doesn't exist in the nodes array");
                    return;
                }

                // Get Support nodes
                List<double[]> support_nodes = new List<double[]>();
                List<Point3d> iSupNode = new List<Point3d>();
                DA.GetDataList("SupportNode", iSupNode);
                for (int i = 0; i < iSupNode.Count; i++)
                {
                    double[] tempArrow1 = new double[3] { Math.Round(iSupNode[i].X, 6), Math.Round(iSupNode[i].Y, 6), Math.Round(iSupNode[i].Z, 6) };
                    support_nodes.Add(tempArrow1);
                }

                // Get Mass nodes
                List<double[]> mass_nodes = new List<double[]>();
                List<Point3d> iMassNode = new List<Point3d>();
                DA.GetDataList("MassNode", iMassNode);
                for (int i = 0; i < iMassNode.Count; i++)
                {
                    double[] tempArrow1 = new double[3] { Math.Round(iMassNode[i].X, 6), Math.Round(iMassNode[i].Y, 6), Math.Round(iMassNode[i].Z, 6) };
                    mass_nodes.Add(tempArrow1);
                }

                // Get Passive connectivity
                GH_Structure<GH_Point> iPassiveConn;
                List<int[]> my_pass_conn = new List<int[]>();
                if (!DA.GetDataTree("PassiveEl", out iPassiveConn))
                {

                }
                else
                {
                    tempHelper.RetrieveConnectivity(iPassiveConn, total_nodes, ref my_pass_conn);
                }

                // Get G1 connectivity
                GH_Structure<GH_Point> iG1Conn;
                List<int[]> my_g1_conn = new List<int[]>();
                if (!DA.GetDataTree("G1", out iG1Conn))
                {

                }
                else
                {
                    tempHelper.RetrieveConnectivity(iG1Conn, total_nodes, ref my_g1_conn);
                }

                // Get G2 connectivity
                GH_Structure<GH_Point> iG2Conn;
                List<int[]> my_g2_conn = new List<int[]>();
                if (!DA.GetDataTree("G2", out iG2Conn))
                {

                }
                else
                {
                    tempHelper.RetrieveConnectivity(iG2Conn, total_nodes, ref my_g2_conn);
                }

                // Get G3 connectivity
                GH_Structure<GH_Point> iG3Conn;
                List<int[]> my_g3_conn = new List<int[]>();
                if (!DA.GetDataTree("G3", out iG3Conn))
                {

                }
                else
                {
                    tempHelper.RetrieveConnectivity(iG3Conn, total_nodes, ref my_g3_conn);
                }

                // Get G4 connectivity
                GH_Structure<GH_Point> iG4Conn;
                List<int[]> my_g4_conn = new List<int[]>();
                if (!DA.GetDataTree("G4", out iG4Conn))
                {

                }
                else
                {
                    tempHelper.RetrieveConnectivity(iG4Conn, total_nodes, ref my_g4_conn);
                }

                // Get G5 connectivity
                GH_Structure<GH_Point> iG5Conn;
                List<int[]> my_g5_conn = new List<int[]>();
                if (!DA.GetDataTree("G5", out iG5Conn))
                {

                }
                else
                {
                    tempHelper.RetrieveConnectivity(iG5Conn, total_nodes, ref my_g5_conn);
                }

                ///////////////////   Pass to MATLAB   //////////////////////////// 
                // Create matrix of nodes to pass to matlab
                double[,] matlab_nodes = new double[total_nodes.Count, 2];
                foreach (var item in total_nodes)
                {
                    int temp_ID = item.Key;
                    double[] temp_point = item.Value;

                    matlab_nodes[temp_ID - 1, 0] = temp_point[0];
                    matlab_nodes[temp_ID - 1, 1] = temp_point[2];
                }
                MWNumericArray arr1 = matlab_nodes;

                // Create array of support nodes ID to pass to matlab
                int[] mat_supp_nodes = new int[support_nodes.Count];
                for (int i = 0; i < support_nodes.Count; i++)
                {
                    double[] node = support_nodes[i];
                    int myKey = 0;
                    foreach (var item in total_nodes)
                    {
                        if (node.SequenceEqual(item.Value))
                        {
                            myKey = item.Key;
                            break;
                        }
                    }
                    mat_supp_nodes[i] = myKey;
                }
                MWNumericArray arr2 = mat_supp_nodes;

                // Create array of mass nodes ID to pass to matlab
                int[] mat_mass_nodes = new int[mass_nodes.Count];
                for (int i = 0; i < mass_nodes.Count; i++)
                {
                    double[] node = mass_nodes[i];
                    int myKey = 0;
                    foreach (var item in total_nodes)
                    {
                        if (node.SequenceEqual(item.Value))
                        {
                            myKey = item.Key;
                            break;
                        }
                    }
                    mat_mass_nodes[i] = myKey;
                }
                MWNumericArray arr3 = mat_mass_nodes;

                // Create matrix of elements to pass to matlab
                int[,] matlab_conn = new int[my_conn.Count, max_num_of_points];
                foreach (var item in my_conn)
                {
                    int temp_ID = item.Key;
                    int[] temp_point = item.Value;
                    for (int i = 0; i < temp_point.Length; i++)
                    {
                        matlab_conn[temp_ID - 1, i] = temp_point[i];
                    }
                }
                MWNumericArray arr4 = matlab_conn;

                MWNumericArray arr5 = new MWNumericArray();
                MWNumericArray arr6 = new MWNumericArray();
                MWNumericArray arr7 = new MWNumericArray();
                MWNumericArray arr8 = new MWNumericArray();
                MWNumericArray arr9 = new MWNumericArray();
                MWNumericArray arr10 = new MWNumericArray();

                // Create array of passive element ID to pass to matlab
                if (my_pass_conn.Count > 0)
                {
                    int[] matlab_pass_conn = new int[my_pass_conn.Count];
                    tempHelper.Gra2MatConn(my_conn, my_pass_conn, ref matlab_pass_conn);
                    arr5 = matlab_pass_conn;
                }
                if (my_g1_conn.Count > 0)
                {
                    int[] matlab_g1_conn = new int[my_g1_conn.Count];
                    tempHelper.Gra2MatConn(my_conn, my_g1_conn, ref matlab_g1_conn);
                    arr6 = matlab_g1_conn;
                }
                if (my_g2_conn.Count > 0)
                {
                    int[] matlab_g2_conn = new int[my_g2_conn.Count];
                    tempHelper.Gra2MatConn(my_conn, my_g2_conn, ref matlab_g2_conn);
                    arr7 = matlab_g2_conn;
                }
                if (my_g3_conn.Count > 0)
                {
                    int[] matlab_g3_conn = new int[my_g3_conn.Count];
                    tempHelper.Gra2MatConn(my_conn, my_g3_conn, ref matlab_g3_conn);
                    arr8 = matlab_g3_conn;
                }
                if (my_g4_conn.Count > 0)
                {
                    int[] matlab_g4_conn = new int[my_g4_conn.Count];
                    tempHelper.Gra2MatConn(my_conn, my_g4_conn, ref matlab_g4_conn);
                    arr9 = matlab_g4_conn;
                }
                if (my_g5_conn.Count > 0)
                {
                    int[] matlab_g5_conn = new int[my_g5_conn.Count];
                    tempHelper.Gra2MatConn(my_conn, my_g5_conn, ref matlab_g5_conn);
                    arr10 = matlab_g5_conn;
                }

                // Last 3 arguements 
                MWNumericArray arr11 = new MWNumericArray();
                MWNumericArray arr12 = new MWNumericArray();
                MWNumericArray arr13 = new MWNumericArray();
                double[] ag_array = new double[ag_list.Count];
                for (int i = 0; i < ag_list.Count; i++)
                {
                    ag_array[i] = ag_list[i];
                }
                arr11 = ag_array;
                double[] FEM_array = new double[FEM.Count];
                for (int i = 0; i < FEM.Count; i++)
                {
                    FEM_array[i] = FEM[i];
                }
                arr12 = FEM_array;
                double[] OPT_array = new double[OPT.Count];
                for (int i = 0; i < OPT.Count; i++)
                {
                    OPT_array[i] = OPT[i];
                }
                arr13 = OPT_array;

                // Results from matlab and run
                VPL_DTO_matClass test1 = null;
                test1 = new VPL_DTO_matClass();
                MWArray[] result = { null, null };
                result = test1.VPL_DTO_mat(1, arr1, arr2, arr3, arr4, arr5, arr6, arr7, arr8, arr9, arr10, folderpath, arr11, arr12, arr13);

                MWNumericArray res1 = null;
                res1 = (MWNumericArray)result[0];

                double[,] res11 = (double[,])((MWNumericArray)res1).ToArray(MWArrayComponent.Real);

                // Output in Grasshopper
                List<double> Densities = res11.OfType<double>().ToList();
                DA.SetDataList("Densities", Densities);
            }
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
            get { return new Guid("96f7a555-4bb3-43da-8f4a-7ec62bdf3e3c"); }
        }
    }
}