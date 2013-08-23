using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mikity;
using mikity.LinearAlgebra;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using mikity.NumericalMethodHelper;
using mikity.NumericalMethodHelper.objects;
namespace mikity.ghComponents
{
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class curve_line_Elements : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.curve-line_elements.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }

        public curve_line_Elements()
            : base("curve->lineElements [General Spring]", "curve->lineElements", "curve->lineElements", "Ricecooker", "Super market")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "c", "Curve", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddGenericParameter("Material", "MAT", "Material", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddGenericParameter("Gravity", "GVT", "Gravity", Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particle System", "pS", "Particle System", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.Register_PointParam("Points", "P", "Particle System",Grasshopper.Kernel.GH_ParamAccess.list);
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
        mikity.NumericalMethodHelper.objects.generalSpring eM = null;
        Rhino.Geometry.Polyline lGeometry=new Rhino.Geometry.Polyline();
        Rhino.Geometry.Polyline lGeometry2=new Rhino.Geometry.Polyline();
        List<Rhino.Geometry.Point3d> newNodes = new List<Rhino.Geometry.Point3d>();
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                Rhino.Geometry.Curve c = null;
                if (!DA.GetData(0, ref c)) return;
                
                Rhino.Geometry.Interval uDomain = c.Domain;
                int[] nEdgeNodes = new int[_dim];
                DA.GetData(1, ref nEdgeNodes[0]);
                for (int i = 0; i < _dim; i++)
                {
                    if (nEdgeNodes[i] < 2)
                    {
                        AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "Integers must be greater than or equal to 2");
                        return;
                    }
                }
                lGeometry.Clear();
                //メッシュノード構築
                newNodes.Clear();
                for (int j = 0; j < nEdgeNodes[0]; j++)
                {
                    newNodes.Add(c.PointAt(uDomain.T0 + (uDomain.T1 - uDomain.T0) / (nEdgeNodes[0]-1) * j));
                    
                }
                int nNewNodes = newNodes.Count;

                GH_material mat = null;
                GH_gravity gvt = null;
                if (!DA.GetData(2, ref mat)) { return; }
                if (!DA.GetData(3, ref gvt)) { return; }
                
                el = MathUtil.isoparametricElements(nEdgeNodes);
                int nElements = el.Length;


                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    particles[i] = new mikity.NumericalMethodHelper.particle(newNodes[i][0], newNodes[i][1], newNodes[i][2]);
                }

                eM = new generalSpring();
                pS = new GH_particleSystem(particles);
                for (int i = 0; i < el.Length; i++)
                {
                    mikity.NumericalMethodHelper.elements.isoparametricElement e = new NumericalMethodHelper.elements.isoparametricElement(el[i]);
                    eM.addElement(e);
                }
                eM.setMaterial(mat.Value, gvt.Value);
                
                
                for (int i = 0; i < pS.Value.__N; i++)
                {
                    lGeometry.Add(particles[i][0], particles[i][1], particles[i][2]);
                }

                this.DVPW = GetDVPW(lGeometry);
                pS.DVPW = GetDVPW(lGeometry2);
                pS.UPGR = GetUPGR(lGeometry2);
                pS.BKGT = GetBKGT(lGeometry2);

                pS.Value.addObject(eM);

                DA.SetData(0, pS);
                DA.SetDataList(1, newNodes);
            }
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
            get { return new Guid("17f79b79-ad33-4687-9f15-4233ac7e6b45"); }
        }

    }
}
