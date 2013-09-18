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
    public class polyline_total_length : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.polyline-total_length.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }

        public polyline_total_length()
            : base("polyline->totalLength [Constraint Condition]", "polyline->totalLength", "polyline->totalLength", "Ricecooker", "Super market")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "c", "Curve", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "L", "Prescribed value for the length", Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particle System", "pS", "Particle System", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.Register_PointParam("Points", "P", "Particle System", Grasshopper.Kernel.GH_ParamAccess.list);
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
        }
        private const int _nNodes = 2;
        private const int _dim = 1;
        DrawViewPortWire DVPW = null;
        Rhino.Geometry.Polyline lGeometry = new Rhino.Geometry.Polyline();
        Rhino.Geometry.Polyline lGeometry2 = new Rhino.Geometry.Polyline();
        BakeGeometry BKGT = null;
        GH_particleSystem pS;
        int nNewNodes = 0;
        int nElements = 0;
        mikity.NumericalMethodHelper.objects.constrainVolumeObject cV = null;
        List<Rhino.Geometry.Point3d> newNodes = new List<Rhino.Geometry.Point3d>();
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            double v = -1.0;
            DA.GetData(1, ref v);
            if (!FriedChiken.isInitialized)
            {
                Rhino.Geometry.Curve c = null;
                if (!DA.GetData(0, ref c)) return;
                if (c.IsPolyline())
                {
                    Rhino.Geometry.Polyline pl = null;
                    if (c.TryGetPolyline(out pl))
                    {
                        nNewNodes = pl.Count();
                        nElements = nNewNodes - 1;
                        newNodes.Clear();
                        newNodes.AddRange(pl);
                        lGeometry.Clear();

                        cV = new constrainVolumeObject(v);
                        mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[nNewNodes];
                        for (int i = 0; i < nNewNodes; i++)
                        {
                            particles[i] = new mikity.NumericalMethodHelper.particle(newNodes[i][0], newNodes[i][1], newNodes[i][2]);
                        }
                        pS = new GH_particleSystem(particles);
                        List<mikity.NumericalMethodHelper.elements.isoparametricElement> e = new List<mikity.NumericalMethodHelper.elements.isoparametricElement>();
                        for (int i = 0; i < nElements; i++)
                        {
                            e.Add(new mikity.NumericalMethodHelper.elements.isoparametricElement(i, i + 1));
                        }
                        for (int i = 0; i < e.Count; i++)
                        {
                            cV.addElement(e[i]);
                        }
                        pS.Value.addObject(cV);
                        lGeometry.Clear();
                        for (int i = 0; i < pS.Value.__N; i++)
                        {
                            lGeometry.Add(particles[i][0], particles[i][1], particles[i][2]);
                        }
                    }
                }
                else
                {
                    AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "Only polyline is accepted");
                    return;
                }

            }
            else
            {
                if (cV != null && v > 0)
                {
                    cV.refVolume = v / nElements;
                }
            }
            this.DVPW = GetDVPW(lGeometry);
            pS.DVPW = GetDVPW(lGeometry2);
            pS.UPGR = GetUPGR(lGeometry2);
            pS.BKGT = GetBKGT(lGeometry2);
            this.BKGT = GetBKGT(lGeometry);

            DA.SetData(0, pS);
            DA.SetDataList(1, newNodes);
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
                    args.Display.DrawPoints(m, Rhino.Display.PointStyle.Simple, 2, System.Drawing.Color.Yellow);
                }
                else
                {
                    args.Display.DrawPolyline(m, System.Drawing.Color.Cyan, 3);
                    args.Display.DrawPoints(m, Rhino.Display.PointStyle.Simple, 2, System.Drawing.Color.Yellow);
                }

            });
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("bef9b72c-a4a0-4edf-82fa-34e4452bdfc7"); }
        }

    }
}

