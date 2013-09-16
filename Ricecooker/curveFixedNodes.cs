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
    public class curve_fixed_nodes : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.curve-fixed_nodes.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
                
        public curve_fixed_nodes()
            : base("curve->fixedNodes [Fixed Node]", "curve->fixedNode", "curve->fixedNode (Constraint condition)", "Ricecooker", "Super market")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
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
            if (Hidden)
            {
                return;
            }
            if (this.Attributes.Selected)
            {
                for (int i = 0; i < lGeometry.Length; i++)
                {
                    args.Display.DrawPoint(lGeometry[i], Rhino.Display.PointStyle.Simple, 8, System.Drawing.Color.LightBlue);
                }
            }
            else
            {
                for (int i = 0; i < lGeometry.Length; i++)
                {
                    args.Display.DrawPoint(lGeometry[i], Rhino.Display.PointStyle.Simple, 8, System.Drawing.Color.DarkBlue);
                }
            }
            base.DrawViewportWires(args);
        }
        private const int _nNodes = 2;
        private const int _dim = 1;
        GH_particleSystem pS;
        int nElements = 0;
        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d(0, 0, -1);
        Rhino.Geometry.Point3d[] lGeometry;
        
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                GH_Point[] pointList = new GH_Point[2];
                List<GH_Point> tmpPointList = new List<GH_Point>();
                GH_Curve c = new GH_Curve();
                int[] nEdgeNodes = new int[_dim];
                DA.GetData(0, ref c);
                DA.GetData(1, ref nEdgeNodes[0]);
                bool x = true, y = true, z = true;
                if (!DA.GetData(2, ref x)) return;
                if (!DA.GetData(3, ref y)) return;
                if (!DA.GetData(4, ref z)) return;
                for (int i = 0; i < _dim; i++)
                {
                    if (nEdgeNodes[i] < 2)
                    {
                        AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "Integers must be greater than or equal to 2");
                        return;
                    }
                }
                //点群生成
                int nNewNodes = nEdgeNodes[0];
                nElements = nNewNodes - 1;
                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    Rhino.Geometry.Point3d p = c.Value.PointAt(c.Value.Domain.T0 + (c.Value.Domain.T1 - c.Value.Domain.T0) / nElements * i);
                    particles[i] = new particle(p.X, p.Y, p.Z);
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
                for (int i = 0; i < nNewNodes; i++)
                {
                    lGeometry[i] = new Rhino.Geometry.Point3d(particles[i][0], particles[i][1], particles[i][2]);
                }
            }

            DA.SetData(0, pS);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("9e9af110-5465-4670-be09-fc024a685a4c"); }
        }

    }
}
