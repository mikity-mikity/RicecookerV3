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
    public class two_nodes_each_length : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.two-nodes-each_length.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
                
        public two_nodes_each_length()
            : base("2nodes->eachLength [Constraint Condition]", "2nodes->eachLength", "2nodes->eachLength (Constraint condition)", "Ricecooker", "Convenience store")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point1", "P1", "First point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point2", "P2", "Second point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item,2);
            pManager.AddNumberParameter("Length", "L", "Prescribed value for the length", Grasshopper.Kernel.GH_ParamAccess.item);
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
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;
        int[][] el = null;
        int nElements = 0;
        List<mikity.NumericalMethodHelper.objects.constrainVolumeObject> cV=new List<mikity.NumericalMethodHelper.objects.constrainVolumeObject>();
//        List<Rhino.Geometry.Line> lGeometry=new List<Rhino.Geometry.Line>();
        Rhino.Geometry.Polyline lGeometry = new Rhino.Geometry.Polyline();
        Rhino.Geometry.Polyline lGeometry2 = new Rhino.Geometry.Polyline();
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            double v = -1.0;
            DA.GetData(3, ref v);
            if (!FriedChiken.isInitialized)
            {
                GH_Point[] pointList = new GH_Point[2];
                List<GH_Point> tmpPointList = new List<GH_Point>();

                int[] nEdgeNodes = new int[_dim];
                DA.GetData(0, ref pointList[0]);
                DA.GetData(1, ref pointList[1]);
                DA.GetData(2, ref nEdgeNodes[0]);
                //cV = new constrainVolumeObject(v);
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
                nElements = el.Length;
                pS = new GH_particleSystem(particles);
                List<mikity.NumericalMethodHelper.elements.isoparametricElement> e = new List<mikity.NumericalMethodHelper.elements.isoparametricElement>();
                for (int i = 0; i < nElements; i++)
                {
                    e.Add(new mikity.NumericalMethodHelper.elements.isoparametricElement(el[i]));
                }
                cV.Clear();
                for (int i = 0; i < e.Count; i++)
                {
                    cV.Add(new constrainVolumeObject(v/nElements));
                    cV[i].addElement(e[i]);
                    pS.Value.addObject(cV[i]);
                }
                lGeometry.Clear();
                for (int i = 0; i < nNewNodes; i++)
                {
                    lGeometry.Add(pS.Value.particles[i, 0], pS.Value.particles[i, 1], pS.Value.particles[i, 2]);
                }
                this.DVPW = GetDVPW(lGeometry);
                pS.DVPW = GetDVPW(lGeometry2);
                pS.UPGR = GetUPGR(lGeometry2);
                pS.BKGT = GetBKGT(lGeometry2);
            }
            else
            {
                if (cV != null&&v>0)
                {
                    for (int i = 0; i < cV.Count; i++)
                    {
                        cV[i].refVolume = v/nElements;
                    }
                }
            }

            DA.SetData(0, pS);
        }
        public BakeGeometry GetBKGT(Rhino.Geometry.Polyline m)
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 2;
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
                    args.Display.DrawPolyline(m, System.Drawing.Color.Red, 3);
                }
                else
                {
                    args.Display.DrawPolyline(m, System.Drawing.Color.DarkMagenta, 3);
                }
                
            });
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("d99a73e9-77c5-447b-aee0-feae8c0800fb"); }
        }

    }
}
