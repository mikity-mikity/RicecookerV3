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
    public class two_nodes_bar : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.two-nodes-bar.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
        
        public two_nodes_bar()
            : base("2nodes->rigidBar [Constraint Condition]", "2nodes->rigidBar", "2nodes->rigidBar (Constraint condition)", "Ricecooker", "Convenience store")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point1", "P1", "First point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point2", "P2", "Second point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "L", "Prescribed value for the length", Grasshopper.Kernel.GH_ParamAccess.item);
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
        public override void BakeGeometry(Rhino.RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, List<Guid> obj_ids)
        {
            if (this.BKGT != null)
            {
                this.BKGT(doc, att, obj_ids);
            }
            base.BakeGeometry(doc, att, obj_ids);
        }
        private const int _nNodes = 2;
        private const int _dim = 1;
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;
        mikity.NumericalMethodHelper.objects.constrainVolumeObject cV;
        Rhino.Geometry.Polyline lGeometry=new Rhino.Geometry.Polyline();
        Rhino.Geometry.Polyline lGeometry2 = new Rhino.Geometry.Polyline();
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            double v = -1.0;
            DA.GetData(2, ref v);
            if (!FriedChiken.isInitialized)
            {

                GH_Point[] pointList = new GH_Point[2];
                DA.GetData(0, ref pointList[0]);
                DA.GetData(1, ref pointList[1]);
                cV = new constrainVolumeObject(v);
                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[_nNodes];
                for (int i = 0; i < _nNodes; i++)
                {
                    particles[i] = new particle(pointList[i].Value.X, pointList[i].Value.Y, pointList[i].Value.Z);
                }
                pS = new GH_particleSystem(particles);
                cV.addElement(new isoparametricElement(0, 1));
                pS.Value.addObject(cV);
                lGeometry.Clear();
                lGeometry2.Clear();
                lGeometry.Add(particles[0][0], particles[0][1], particles[0][2]);
                lGeometry.Add(particles[1][0], particles[1][1], particles[1][2]);
                lGeometry2.Add(particles[0][0], particles[0][1], particles[0][2]);
                lGeometry2.Add(particles[1][0], particles[1][1], particles[1][2]);
                
                this.DVPW = GetDVPW(lGeometry);
                this.BKGT = GetBKGT(lGeometry); 
                pS.DVPW = GetDVPW(lGeometry2);
                pS.UPGR = GetUPGR(lGeometry2);
                pS.BKGT = GetBKGT(lGeometry2);
            }
            else
            {
                if (cV != null)
                {
                    if (v > 0)
                        cV.refVolume = v;
                }
            }

            DA.SetData(0, pS);
        }
        public BakeGeometry GetBKGT(Rhino.Geometry.Polyline m)
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 4;
                o.Add(d.Objects.AddPolyline(m, a2));
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
                    args.Display.DrawPolyline(m, System.Drawing.Color.Red, 5);
                }
                else
                {
                    args.Display.DrawPolyline(m, System.Drawing.Color.DarkMagenta, 5);
                }
                
            });
        }
       
        public override Guid ComponentGuid
        {
            get { return new Guid("faa09133-7685-44cd-be06-15af2ac247e8"); }
        }

    }
}
