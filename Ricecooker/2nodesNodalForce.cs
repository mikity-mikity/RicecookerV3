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
    public class two_nodes_nodal_force : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.two-nodes-nodal_force.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
                
        public two_nodes_nodal_force()
            : base("2nodes->nodalForces [Nodal Force]", "2nodes->nodalForces", "2nodes->nodalForces", "Ricecooker", "Convenience store")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point1", "P1", "First point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point2", "P2", "Second point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item,2);
            pManager.AddVectorParameter("Force", "Force", "Force (Vector3d)", Grasshopper.Kernel.GH_ParamAccess.item, new Rhino.Geometry.Vector3d(0, 0, -1));
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particle System", "pS", "Particle System", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        public override void BakeGeometry(Rhino.RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, List<Guid> obj_ids)
        {
            if (this.BKGT != null)
            {
                this.BKGT(doc, att, obj_ids);
            }
        }

        public override void DrawViewportWires(Grasshopper.Kernel.IGH_PreviewArgs args)
        {
            if (this.DVPW != null)
            {
                this.DVPW(args);
            }
            base.DrawViewportWires(args);
        }
        private const int _nNodes = 2;
        private const int _dim = 1;
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;
        nodalForce nF;
        List<Rhino.Geometry.Point3d> lGeometry=new List<Rhino.Geometry.Point3d>();
        List<Rhino.Geometry.Point3d> lGeometry2=new List<Rhino.Geometry.Point3d>();
        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d(0, 0, -1);
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {

                GH_Point[] pointList = new GH_Point[2];
                List<GH_Point> tmpPointList = new List<GH_Point>();
                int[] nEdgeNodes = new int[_dim];
                DA.GetData(0, ref pointList[0]);
                DA.GetData(1, ref pointList[1]);
                DA.GetData(2, ref nEdgeNodes[0]);
                if (!DA.GetData(3, ref v)) return;

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
                nF = new nodalForce(v.X, v.Y, v.Z);
                for (int i = 0; i < nNewNodes; i++)
                {
                    nF.addNode(lNodes[i]);
                }
                pS.Value.addObject(nF);

                lGeometry.Clear();
                for (int i = 0; i < nNewNodes; i++)
                {
                    lGeometry.Add( new Rhino.Geometry.Point3d(particles[i][0], particles[i][1], particles[i][2]));
                }
                this.DVPW = GetDVPW(lGeometry);
                pS.DVPW = GetDVPW(lGeometry2);
                pS.UPGR = GetUPGR(lGeometry2);
                pS.BKGT = GetBKGT(lGeometry2);
            }else
            {
                nF.forceX = v.X;
                nF.forceY = v.Y;
                nF.forceZ = v.Z;
            }
            DA.SetData(0, pS);
        }
        public BakeGeometry GetBKGT(List<Rhino.Geometry.Point3d> m)
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 2;
                for (int i = 0; i < m.Count; i++)
                {
                    Guid id = d.Objects.AddLine(new Rhino.Geometry.Line(m[i],m[i]+v), a2);
                    o.Add(id);
                }

            });
        }

        public UpdateGeometry GetUPGR(List<Rhino.Geometry.Point3d> m)
        {
            return new UpdateGeometry((x, y, z) =>
            {
                m.Clear();
                for (int i = 0; i < pS.Value.__N; i++)
                {
                    m.Add(new Rhino.Geometry.Point3d(pS.Value.particles[i, 0] + x, pS.Value.particles[i, 1] + y, pS.Value.particles[i, 2] + z));
                }
            });
        }
        public DrawViewPortWire GetDVPW(List<Rhino.Geometry.Point3d> m)
        {
            return new DrawViewPortWire((args) =>
            {

                if (Hidden)
                {
                    return;
                }
                if (this.Attributes.Selected)
                {
                    for (int i = 0; i < lGeometry.Count; i++)
                    {
                        args.Display.DrawDirectionArrow(m[i], v, System.Drawing.Color.Red);
                    }
                }
                else
                {
                    for (int i = 0; i < lGeometry.Count; i++)
                    {
                        args.Display.DrawDirectionArrow(m[i], v, System.Drawing.Color.DarkBlue);
                    }
                }
            });
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("d2014337-6d02-40d9-a50b-84ae6625b919"); }
        }

    }
}
