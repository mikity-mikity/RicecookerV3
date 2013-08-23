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
    public class curve_each_length : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.curve-each_length.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
        public curve_each_length()
            : base("curve->eachLength [Constraint Condition]", "curve->eachLength", "curve->eachLength (Constraint condition)", "Ricecooker", "Super market")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
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
        int nElements = 0;
        List<mikity.NumericalMethodHelper.objects.constrainVolumeObject> lCV=new List<mikity.NumericalMethodHelper.objects.constrainVolumeObject>();
        //List<Rhino.Geometry.Line> lGeometry=new List<Rhino.Geometry.Line>();
        Rhino.Geometry.Polyline lGeometry = new Rhino.Geometry.Polyline();
        Rhino.Geometry.Polyline lGeometry2 = new Rhino.Geometry.Polyline();
        
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            double v = -1.0;
            DA.GetData(2, ref v);
            if (!FriedChiken.isInitialized)
            {
                GH_Point[] pointList = new GH_Point[2];
                List<GH_Point> tmpPointList = new List<GH_Point>();
                GH_Curve c = new GH_Curve();
                int[] nEdgeNodes = new int[_dim];
                DA.GetData(0, ref c);
                DA.GetData(1, ref nEdgeNodes[0]);
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
                int nNewNodes = nEdgeNodes[0];
                nElements = nNewNodes - 1;
                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    Rhino.Geometry.Point3d p = c.Value.PointAt(c.Value.Domain.T0+(c.Value.Domain.T1-c.Value.Domain.T0) / nElements * i);
                    particles[i] = new particle(p.X, p.Y, p.Z);
                }
                pS = new GH_particleSystem(particles);
                List<mikity.NumericalMethodHelper.elements.isoparametricElement> e = new List<mikity.NumericalMethodHelper.elements.isoparametricElement>();
                for (int i = 0; i < nElements; i++)
                {
                    e.Add(new mikity.NumericalMethodHelper.elements.isoparametricElement(i,i+1));
                }
                lCV.Clear();
                for (int i = 0; i < e.Count; i++)
                {
                    lCV.Add(new constrainVolumeObject(v/nElements));
                    lCV[i].addElement(e[i]);
                    pS.Value.addObject(lCV[i]);
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
                if (lCV != null&&v>0)
                {
                    for (int i = 0; i < lCV.Count; i++)
                    {
                        lCV[i].refVolume = v/nElements;
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
                base.DrawViewportWires(args);
                
            });
        }



        public override Guid ComponentGuid
        {
            get { return new Guid("7f46af3d-f5db-4cd3-80f9-a3909bbdb1bb"); }
        }

    }
}

