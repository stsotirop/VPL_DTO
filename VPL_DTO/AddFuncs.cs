using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace VPL_DTO
{
    class DistinctDoubleArrayComparer : IEqualityComparer<double[]>
    {
        public bool Equals(double[] x, double[] y)
        {
            if (x.Length != y.Length) { return false; }
            else if (x.Length != 3 || y.Length != 3) { return false; }

            return x[0] == y[0] && x[1] == y[1] && x[2] == y[2];
        }

        public int GetHashCode(double[] obj)
        {
            return -1;
        }
    }
    public class HelperFunctions
    {
        public void RetrieveConnectivity(GH_Structure<GH_Point> iConn, Dictionary<int, double[]> total_nodes, ref List<int[]> my_conn)
        {
            for (int i = 0; i < iConn.Branches.Count; i++)
            {
                List<GH_Point> branch = iConn.Branches[i];
                double[,] pointsArray = new double[branch.Count, 3];
                int[] pointsID = new int[branch.Count];
                double[] tempPoint;
                for (int j = 0; j < branch.Count; j++)
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
                }
                Array.Reverse(pointsID);
                my_conn.Add(pointsID);
            }
        }

        public void Gra2MatConn(Dictionary<int, int[]> my_conn, List<int[]> my_pass_conn, ref int[] matlab_pass_conn)
        {
            for (int i = 0; i < my_pass_conn.Count; i++)
            {
                int[] temp = my_pass_conn[i];
                int myKey = 0;
                foreach (var item in my_conn)
                {
                    if (temp.SequenceEqual(item.Value))
                    {
                        myKey = item.Key;
                        break;
                    }
                }
                matlab_pass_conn[i] = myKey;
            }
        }
    }
}
