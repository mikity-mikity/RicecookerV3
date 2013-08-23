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
    public class node_pairs_fixed_nodes : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.node-pairs-fixed_nodes.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
                
        public node_pairs_fixed_nodes()
            : base("nodePairs->fixedNodes [Fixed Node]", "nodePairs->fixedNodes", "nodePairs->FixedNodes", "Ricecooker", "Convenience store")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("PointList1", "listP1", "First point list", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddPointParameter("PointList2", "listP2", "Second point list", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddBooleanParameter("fixX", "X", "fixX", Grasshopper.Kernel.GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("fixY", "Y", "fixY", Grasshopper.Kernel.GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("fixZ", "Z", "fixZ", Grasshopper.Kernel.GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("group", "g", "if true, one group of points will be created, otherwise, you can manipulate each point separately", Grasshopper.Kernel.GH_ParamAccess.item, true);

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
            base.BakeGeometry(doc, att, obj_ids);
        }

        public override void DrawViewportWires(Grasshopper.Kernel.IGH_PreviewArgs args)
        {
            if (this.DVPW != null)
            {
                this.DVPW(args);
            }
            base.DrawViewportWires(args);
        }
        private const int _dim = 1;
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;
        List<Rhino.Geometry.Point3d> lGeometry=new List<Rhino.Geometry.Point3d>();
        List<Rhino.Geometry.Point3d> lGeometry2=new List<Rhino.Geometry.Point3d>();
        List<mikity.NumericalMethodHelper.objects.constrainVolumeObject> lCV=new List<constrainVolumeObject>();
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                List<GH_Point> pointList1 = new List<GH_Point>();
                List<GH_Point> pointList2 = new List<GH_Point>();
                if (!DA.GetDataList(0, pointList1)) return;
                if (!DA.GetDataList(1, pointList2)) return;
                if (pointList1.Count != pointList2.Count)
                {
                    AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "The first and second lists must have the same elements");
                    return;
                }
                int __n = pointList1.Count;
                int nU = 0;
                if (!DA.GetData(2, ref nU)) return;
                List<GH_Point[]> pointLists = new List<GH_Point[]>();
                for (int i = 0; i < __n; i++)
                {
                    pointLists.Add(new GH_Point[nU]);
                }
                bool x = true, y = true, z = true;
                if (!DA.GetData(3, ref x)) return;
                if (!DA.GetData(4, ref y)) return;
                if (!DA.GetData(5, ref z)) return;
                bool isGroup = true;
                if (!DA.GetData(6, ref isGroup)) return;
                //点群生成
                double[,] wt = mikity.MathUtil.bicubic(1, new int[1] { nU });
                int nNewNodes = wt.GetLength(0);
                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[nNewNodes * __n];
                for (int j = 0; j < __n; j++)
                {
                    for (int i = 0; i < nNewNodes; i++)
                    {
                        particles[i + j * nNewNodes] = new particle(0, 0, 0);
                        particles[i + j * nNewNodes][0] += pointList1[j].Value.X * wt[i, 0];
                        particles[i + j * nNewNodes][1] += pointList1[j].Value.Y * wt[i, 0];
                        particles[i + j * nNewNodes][2] += pointList1[j].Value.Z * wt[i, 0];
                        particles[i + j * nNewNodes][0] += pointList2[j].Value.X * wt[i, 1];
                        particles[i + j * nNewNodes][1] += pointList2[j].Value.Y * wt[i, 1];
                        particles[i + j * nNewNodes][2] += pointList2[j].Value.Z * wt[i, 1];
                    }
                }
                pS = new GH_particleSystem(particles);
                node[] lNodes = new node[__n*nNewNodes];
                for (int i = 0; i < __n * nNewNodes; i++)
                {
                    lNodes[i] = new node(i);
                    lNodes[i].copyFrom(pS.Value.particles);
                }
                if (isGroup)
                {
                    fixedNodes fN = new fixedNodes(x, y, z);

                    for (int i = 0; i < __n * nNewNodes; i++)
                    {
                        fN.addNode(lNodes[i]);
                    }
                    pS.Value.addObject(fN);
                }
                else
                {
                    for (int i = 0; i < __n; i++)
                    {
                        fixedNodes fN = new fixedNodes(x, y, z);
                        for (int j = 0; j < nNewNodes; j++)
                        {
                            fN.addNode(lNodes[j+nNewNodes*i]);
                        }
                        pS.Value.addObject(fN);
                    }
                }


                if (isGroup)
                {
                }
                else
                {
                }
                lGeometry.Clear();
                lGeometry2.Clear();
                for (int i = 0; i < __n * nNewNodes; i++)
                {
                    lGeometry.Add( new Rhino.Geometry.Point3d(particles[i][0], particles[i][1], particles[i][2]));
                }
                this.DVPW = GetDVPW(lGeometry);
                this.BKGT = GetBKGT(lGeometry);
                pS.DVPW = GetDVPW(lGeometry2);
                pS.UPGR = GetUPGR(lGeometry2);
                pS.BKGT = GetBKGT(lGeometry2);
            }
            DA.SetData(0, pS);
        }
        public BakeGeometry GetBKGT(List<Rhino.Geometry.Point3d> m)
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 3;
                Rhino.Collections.RhinoList<Guid> id = d.Objects.AddPoints(m, a2);
                o.AddRange(id);
            });
        }

        public UpdateGeometry GetUPGR(List<Rhino.Geometry.Point3d> m)
        {
            return new UpdateGeometry((x, y, z) =>
            {
                m.Clear();
                for (int i = 0; i < pS.Value.__N; i++)
                {
                    m.Add( new Rhino.Geometry.Point3d(pS.Value.particles[i, 0] + x, pS.Value.particles[i, 1] + y, pS.Value.particles[i, 2] + z));
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
            get { return new Guid("ca789c4e-3aca-4f54-8449-f051109aa18c"); }
        }

    }
}
