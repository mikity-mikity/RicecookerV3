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
    public class two_nodes_line_elements : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.two-nodes-line_elements.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
                
        public two_nodes_line_elements()
            : base("2nodes->lineElements [General Spring]", "2nodes->lineElements", "2nodes->lineElements", "Ricecooker", "Convenience store")
        {            
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point1", "P1", "First point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point2", "P2", "Second point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item,2);
            pManager.AddGenericParameter("Material", "mat", "Material", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddGenericParameter("Gravity", "gvt", "Gravity", Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particle System", "pS", "Particle System", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        public override void BakeGeometry(Rhino.RhinoDoc doc, List<Guid> obj_ids)
        {
            base.BakeGeometry(doc, obj_ids);
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
        int[][] el = null;
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;
        mikity.NumericalMethodHelper.objects.generalSpring eM = null;
        Rhino.Geometry.Polyline lGeometry;
        Rhino.Geometry.Polyline lGeometry2;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                GH_Point[] pointList = new GH_Point[2];
                List<GH_Point> tmpPointList = new List<GH_Point>();

                int[] nEdgeNodes = new int[_dim];
                eM = new generalSpring();
                DA.GetData(0, ref pointList[0]);
                DA.GetData(1, ref pointList[1]);
                DA.GetData(2, ref nEdgeNodes[0]);
                GH_material mat = null;
                if (!DA.GetData(3, ref mat)) { return; }
                GH_gravity gvt = null;
                if (!DA.GetData(4, ref gvt)) { return; }
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
                el = MathUtil.isoparametricElements(nEdgeNodes);
                int nElements = el.Length;
                pS = new GH_particleSystem(particles);
                for (int i = 0; i < nElements; i++)
                {
                    eM.addElement(new mikity.NumericalMethodHelper.elements.isoparametricElement(el[i]));
                }
                eM.setMaterial(mat.Value,gvt.Value);
                pS.Value.addObject(eM);
                lGeometry = new Rhino.Geometry.Polyline();
                lGeometry2 = new Rhino.Geometry.Polyline();

                for (int i = 0; i < pS.Value.__N; i++)
                {
                    lGeometry.Add(particles[i][0], particles[i][1], particles[i][2]);
                }

                this.DVPW = GetDVPW(lGeometry);
                pS.DVPW = GetDVPW(lGeometry2);
                pS.UPGR = GetUPGR(lGeometry2);
                pS.BKGT = GetBKGT(lGeometry2);
            }

            DA.SetData(0, pS);
        }
        public BakeGeometry GetBKGT(Rhino.Geometry.Polyline m)
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 3;
                Guid id = d.Objects.AddPolyline(m, a2);
                o.Add(id);
            });
        }
        public UpdateGeometry GetUPGR(Rhino.Geometry.Polyline m)
        {
            return new UpdateGeometry((x, y, z) =>
            {
                m.Clear();
                for (int i = 0; i < pS.Value.__N; i++)
                {
                    m.Add(pS.Value.particles[i, 0] + x, pS.Value.particles[i, 1] + y, pS.Value.particles[i, 2] + z);
                }
            });
        }
        public DrawViewPortWire GetDVPW(Rhino.Geometry.Polyline m)
        {
            return new DrawViewPortWire((args) =>
            {
                    if (Hidden)
                    {
                        return;
                    }
                    if (this.Attributes.Selected)
                    {
                        args.Display.DrawPolyline(m, System.Drawing.Color.Red, 2);
                        args.Display.DrawPoints(m, Rhino.Display.PointStyle.Simple,4, System.Drawing.Color.Yellow);
                    }
                    else
                    {
                        args.Display.DrawPolyline(m, System.Drawing.Color.Blue, 2);
                        args.Display.DrawPoints(m, Rhino.Display.PointStyle.Simple, 4, System.Drawing.Color.Yellow);
                    }
                
            });
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("b8ed770c-4d62-4e29-a29b-21e7d80fc756"); }
        }
    }
}
