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
    public class node_pairs_bar : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.node-pairs-rigid-bars.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
                
        public node_pairs_bar()
            : base("nodePairs->rigidBars [Constraint Condition]", "nodePairs->rigidBars", "nodePairs->rigidBars (Constraint conditions)", "Ricecooker", "Convenience store")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("PointList1", "listP1", "First point list", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddPointParameter("PointList2", "listP2", "Second point list", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddNumberParameter("Length", "L", "Prescribed value for the length", Grasshopper.Kernel.GH_ParamAccess.item);
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
        int nNewNodes = 0;
        int nElements = 0;
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;
        List<mikity.NumericalMethodHelper.objects.constrainVolumeObject> lCV=new List<constrainVolumeObject>();
        List<Rhino.Geometry.Line> lGeometry = new List<Rhino.Geometry.Line>();
        List<Rhino.Geometry.Line> lGeometry2 = new List<Rhino.Geometry.Line>();
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            double v = -1.0;
            DA.GetData(2, ref v);
            if (!FriedChiken.isInitialized)
            {

                List<GH_Point> pointList1 = new List<GH_Point>();
                List<GH_Point> pointList2 = new List<GH_Point>();
                List<GH_Point> tmpPointList = new List<GH_Point>();
                if (!DA.GetDataList(0, pointList1)) return;
                if (!DA.GetDataList(1, pointList2)) return;
                if (pointList1.Count != pointList2.Count)
                {
                    AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "The first and second lists must have the same elements");
                    return;
                }
                tmpPointList.AddRange(pointList1);
                tmpPointList.AddRange(pointList2);
                nNewNodes = tmpPointList.Count();
                nElements = pointList1.Count();
                lCV.Clear();
                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    particles[i] = new particle(tmpPointList[i].Value.X, tmpPointList[i].Value.Y, tmpPointList[i].Value.Z);
                }
                pS = new GH_particleSystem(particles);
                for (int i = 0; i < nElements; i++)
                {
                    lCV.Add(new constrainVolumeObject(v));
                    lCV[i].addElement(new isoparametricElement(i,i+nElements));
                }
                pS.Value.addObjects(lCV.ToArray());
                lGeometry.Clear();
                for (int i = 0; i < nElements; i++)
                {
                    lGeometry.Add(new Rhino.Geometry.Line(pS.Value.particles[i, 0], pS.Value.particles[i, 1], pS.Value.particles[i, 2], pS.Value.particles[i+nElements, 0], pS.Value.particles[i+nElements, 1], pS.Value.particles[i+nElements, 2]));
                }            
             }
            else
            {
                if (lCV != null)
                {
                    if (v > 0)
                    {
                        for (int i = 0; i < nElements; i++)
                        {
                            lCV[i].refVolume = v;
                        }
                    }
                }
            }
            this.DVPW = GetDVPW(lGeometry);
            this.BKGT = GetBKGT(lGeometry);
            pS.DVPW = GetDVPW(lGeometry2);
            pS.UPGR = GetUPGR(lGeometry2);
            pS.BKGT = GetBKGT(lGeometry2);
            DA.SetData(0, pS);
        }
        public BakeGeometry GetBKGT(List<Rhino.Geometry.Line> m)
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 3;
                foreach (Rhino.Geometry.Line l in m)
                {
                    o.Add(d.Objects.AddLine(l, a2));
                }
            });
        }

        public UpdateGeometry GetUPGR(List<Rhino.Geometry.Line> m)
        {
            return new UpdateGeometry((x, y, z) =>
            {
                m.Clear();
                for (int i = 0; i < nElements; i++)
                {
                    m.Add(new Rhino.Geometry.Line(pS.Value.particles[i, 0] + x, pS.Value.particles[i, 1] + y, pS.Value.particles[i, 2] + z, pS.Value.particles[i + nElements, 0] + x, pS.Value.particles[i + nElements, 1] + y, pS.Value.particles[i + nElements, 2] + z));
                }
            });
        }
        public DrawViewPortWire GetDVPW(List<Rhino.Geometry.Line> m)
        {
            return new DrawViewPortWire((args) =>
            {
                if (Hidden)
                {
                    return;
                }
                if (this.Attributes.Selected)
                {
                    args.Display.DrawLines(m, System.Drawing.Color.Red, 3);
                }
                else
                {
                    args.Display.DrawLines(m, System.Drawing.Color.Blue, 3);
                }

            });
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("609cf721-4669-4cb6-9c77-5d333cbe1a80"); }
        }

    }
}
