using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mikity;
using mikity.LinearAlgebra;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using mikity.NumericalMethodHelper;
using mikity.NumericalMethodHelper.elements;
using mikity.NumericalMethodHelper.objects;
using mikity.NumericalMethodHelper.materials;
namespace mikity.ghComponents
{
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class four_nodes_fixed_nodes : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.four-nodes-fixed_nodes.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
                
        public four_nodes_fixed_nodes()
            : base("4nodes->fixedNodes [Fixed Node]", "4nodes->fixedNodes", "4nodes->fixedNodes", "Ricecooker", "Convenience store")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point1", "P1", "First point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point2", "P2", "Second point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point3", "P3", "Third point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point4", "P4", "Fourth point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Number", "nV", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddBooleanParameter("fixX", "X", "fixX", Grasshopper.Kernel.GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("fixY", "Y", "fixY", Grasshopper.Kernel.GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("fixZ", "Z", "fixZ", Grasshopper.Kernel.GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particle System", "pS", "Particle System", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        public override void DrawViewportWires(Grasshopper.Kernel.IGH_PreviewArgs args)
        {
            if (this.DVPW != null)
            {
                this.DVPW(args);
            }
            base.DrawViewportWires(args);
        }
        private const int _nNodes = 4;
        private const int _dim = 2;
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        Rhino.Geometry.Point3d[] lGeometry;
        Rhino.Geometry.Point3d[] lGeometry2;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {

                GH_Point[] pointList = new GH_Point[4];
                List<GH_Point> tmpPointList = new List<GH_Point>();
                int[] nEdgeNodes = new int[_dim];
                DA.GetData(0, ref pointList[0]);
                DA.GetData(1, ref pointList[1]);
                DA.GetData(2, ref pointList[2]);
                DA.GetData(3, ref pointList[3]);
                DA.GetData(4, ref nEdgeNodes[0]);
                DA.GetData(5, ref nEdgeNodes[1]);
                bool x = true, y = true, z = true;
                if (!DA.GetData(6, ref x)) return;
                if (!DA.GetData(7, ref y)) return;
                if (!DA.GetData(8, ref z)) return;

                for (int i = 0; i < _dim; i++)
                {
                    if (nEdgeNodes[i] < 2)
                    {
                        AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "Integers must be greater than or equal to 2");
                        return;
                    }
                }
                //点群生成
                double[,] wt = mikity.MathUtil.bicubic(_dim, nEdgeNodes);
                int nNewNodes = wt.GetLength(0);
                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    particles[i] = new particle(0, 0, 0);
                    for (int j = 0; j < _nNodes; j++)
                    {
                        particles[i][0] += pointList[j].Value.X * wt[i, j];
                        particles[i][1] += pointList[j].Value.Y * wt[i, j];
                        particles[i][2] += pointList[j].Value.Z * wt[i, j];
                    }
                }
                pS = new GH_particleSystem(particles);
                node[] lNodes = new node[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    lNodes[i] = new node(i);
                    lNodes[i].copyFrom(pS.Value.particles);
                }
                fixedNodes fN = new fixedNodes(x, y, z);

                for (int i = 0; i < nNewNodes; i++)
                {
                    fN.addNode(lNodes[i]);
                }
                pS.Value.addObject(fN);
                lGeometry = new Rhino.Geometry.Point3d[nNewNodes];
                lGeometry2 = new Rhino.Geometry.Point3d[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    lGeometry[i] = new Rhino.Geometry.Point3d(particles[i][0], particles[i][1], particles[i][2]);
                }
                this.DVPW = GetDVPW(lGeometry);
                pS.DVPW = GetDVPW(lGeometry2);
                pS.UPGR = GetUPGR(lGeometry2);
            }
            DA.SetData(0, pS);
        }
        public UpdateGeometry GetUPGR(Rhino.Geometry.Point3d[] m)
        {
            return new UpdateGeometry((x, y, z) =>
            {
                for (int i = 0; i < pS.Value.__N; i++)
                {
                    m[i] = new Rhino.Geometry.Point3d(pS.Value.particles[i, 0] + x, pS.Value.particles[i, 1] + y, pS.Value.particles[i, 2] + z);
                }
            });
        }
        public DrawViewPortWire GetDVPW(Rhino.Geometry.Point3d[] m)
        {
            return new DrawViewPortWire((args) =>
            {
                if (Hidden)
                {
                    return;
                }
                if (this.Attributes.Selected)
                {
                    args.Display.DrawPoints(m, Rhino.Display.PointStyle.Simple, 8, System.Drawing.Color.Red);

                }
                else
                {
                    args.Display.DrawPoints(m, Rhino.Display.PointStyle.Simple, 8, System.Drawing.Color.DarkRed);
                }

            });
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("1251cca4-09c8-409c-b76a-28d7aabaa9f0"); }
        }

    }
}
