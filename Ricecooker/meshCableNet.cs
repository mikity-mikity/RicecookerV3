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
    public class mesh_cable_Net : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.mesh-quad_elements.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }

        public mesh_cable_Net()
            : base("mesh->cableNet [General Spring]", "mesh->cableNet", "mesh->cableNet", "Ricecooker", "Super market")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "m", "Mesh", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "MAT", "Material", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddGenericParameter("Gravity", "GVT", "Gravity", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddBooleanParameter("isJoin", "Join", "join same particles if true", Grasshopper.Kernel.GH_ParamAccess.item,false);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particle System", "pS", "Particle System", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.Register_PointParam("Points", "P", "Particle System",Grasshopper.Kernel.GH_ParamAccess.list);
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
            this.DVPW(args);
            base.DrawViewportWires(args);
        }

        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;
        List<int[]> el = null;
        mikity.NumericalMethodHelper.objects.generalSpring eM = null;
        List<Rhino.Geometry.Line>lGeometry=new List<Rhino.Geometry.Line>();
        List<Rhino.Geometry.Line> lGeometry2 = new List<Rhino.Geometry.Line>();
        List<Rhino.Geometry.Point3d> newNodes = new List<Rhino.Geometry.Point3d>();
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                Rhino.Geometry.Mesh m = null;
                if (!DA.GetData(0, ref m)) return;
                lGeometry.Clear();
                lGeometry2.Clear();
                var ms = mikity.GeometryProcessing.MeshStructure.CreateFrom(m);
                var edges=ms.edges();

                newNodes.Clear();
                newNodes.AddRange(m.Vertices.ToPoint3dArray());
                int nNewNodes = newNodes.Count;
                el = new List<int[]>();
                foreach (var e in edges)
                {
                    el.Add(new int[2] { e.P.N, e.next.P.N });
                }
                bool isJoin = false;
                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    particles[i] = new mikity.NumericalMethodHelper.particle(newNodes[i][0], newNodes[i][1], newNodes[i][2]);
                }

                eM = new generalSpring();
                pS = new GH_particleSystem(particles);
                if (!DA.GetData(3, ref isJoin)) { return; }
                if(isJoin){
                    pS.simplify(el);
                }
                foreach (var e in el)
                {
                    var ee=new mikity.NumericalMethodHelper.elements.simplexElement(e);
                    eM.addElement(ee);
                    lGeometry.Add(new Rhino.Geometry.Line(pS.Value.particles[e[0], 0], pS.Value.particles[e[0], 1], pS.Value.particles[e[0], 2], pS.Value.particles[e[1], 0], pS.Value.particles[e[1], 1], pS.Value.particles[e[1], 2]));
                }
                pS.Value.addObject(eM);
                this.DVPW = GetDVPW(lGeometry);
                this.BKGT = GetBKGT(lGeometry);
                pS.DVPW = GetDVPW(lGeometry2);
                pS.UPGR = GetUPGR(lGeometry2);
                pS.BKGT = GetBKGT(lGeometry2);
                DA.SetData(0, pS);
                DA.SetDataList(1, newNodes);
                GH_material mat = null;
                GH_gravity gvt = null;
                if (!DA.GetData(1, ref mat)) { return; }
                if (!DA.GetData(2, ref gvt)) { return; }
                eM.setMaterial(mat.Value, gvt.Value);

            }
        }
        public BakeGeometry GetBKGT(List<Rhino.Geometry.Line> m)
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 1;
                foreach (var l in m)
                {
                    Guid id = d.Objects.AddLine(l, a2);
                    o.Add(id);
                }
            });
        }

        public UpdateGeometry GetUPGR(List<Rhino.Geometry.Line> m)
        {
            return new UpdateGeometry((x, y, z) =>
            {
                m.Clear();
                foreach (var e in el)
                {
                    m.Add(new Rhino.Geometry.Line(pS.Value.particles[e[0], 0] + x, pS.Value.particles[e[0], 1] + y, pS.Value.particles[e[0], 2] + z, pS.Value.particles[e[1], 0] + x, pS.Value.particles[e[1], 1] + y, pS.Value.particles[e[1], 2] + z));
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
                    if (m != null)
                    {
                        if (this.Attributes.Selected)
                        {
                            args.Display.DrawLines(m, System.Drawing.Color.Red, 1);
                        }
                        else
                        {
                            args.Display.DrawLines(m, System.Drawing.Color.Aquamarine, 1);
                        }
                    }
                });
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("05cb0bbd-56bb-4083-97fb-8a39e7ef5a0c"); }
        }

    }
}
