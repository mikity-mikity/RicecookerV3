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
    public class node_pairs_line_elements : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.node_pairs-line_elements.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
                
        public node_pairs_line_elements()
            : base("nodePairs->lineElements [General Spring]", "nodePairs->lineElements", "2nodes->lineElements", "Ricecooker", "Convenience store")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("PointList1", "listP1", "First point list", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddPointParameter("PointList2", "listP2", "Second point list", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "MAT", "Material", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddGenericParameter("Gravity", "GVT", "Gravity", Grasshopper.Kernel.GH_ParamAccess.item);
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
            this.DVPW(args);
            base.DrawViewportWires(args);
        }

        private const int _dim = 1;
        int nNewNodes = 0;
        int nElements = 0;
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;

        mikity.NumericalMethodHelper.objects.generalSpring eM = null;
        List<Rhino.Geometry.Line> lGeometry = new List<Rhino.Geometry.Line>();
        List<Rhino.Geometry.Line> lGeometry2 = new List<Rhino.Geometry.Line>();
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                List<GH_Point> pointList1 = new List<GH_Point>();
                List<GH_Point> pointList2 = new List<GH_Point>();
                List<GH_Point> tmpPointList = new List<GH_Point>();

                eM = new generalSpring();
                if(!DA.GetDataList(0, pointList1))return;
                if(!DA.GetDataList(1, pointList2))return;
                if (pointList1.Count != pointList2.Count)
                {
                    AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "The first and second lists must have the same elements");
                    return;
                }
                tmpPointList.AddRange(pointList1);
                tmpPointList.AddRange(pointList2);
                nNewNodes = tmpPointList.Count();
                nElements = pointList1.Count();
                GH_material mat = null;
                GH_gravity gvt = null;
                if (!DA.GetData(2, ref mat)) { return; }
                if (!DA.GetData(3, ref gvt)) { return; }
                
                //点群生成
                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    particles[i] = new particle(tmpPointList[i].Value.X, tmpPointList[i].Value.Y, tmpPointList[i].Value.Z);
                }
                pS = new GH_particleSystem(particles);
                for (int i = 0; i < nElements; i++)
                {
                    eM.addElement(new mikity.NumericalMethodHelper.elements.isoparametricElement(i, i + nElements));
                }
                eM.setMaterial(mat.Value, gvt.Value);
                pS.Value.addObject(eM);
                lGeometry.Clear();
                lGeometry2.Clear();
                for (int i = 0; i < nElements; i++)
                {
                    lGeometry.Add(new Rhino.Geometry.Line(pS.Value.particles[i, 0], pS.Value.particles[i, 1], pS.Value.particles[i, 2], pS.Value.particles[i+nElements, 0], pS.Value.particles[i+nElements, 1], pS.Value.particles[i+nElements, 2]));
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
                    args.Display.DrawLines(m, System.Drawing.Color.Red, 1);
                }
                else
                {
                    args.Display.DrawLines(m, System.Drawing.Color.Beige, 1);
                }

            });
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("3ef70808-58d2-4553-8aca-9034db7d492e"); }
        }

    }
}
