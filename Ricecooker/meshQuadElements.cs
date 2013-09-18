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
    public class mesh_quad_Elements : Grasshopper.Kernel.GH_Component
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

        public mesh_quad_Elements()
            : base("mesh->quad/triElements [General Spring]", "mesh->quad/triElements", "mesh->quad/triElements", "Ricecooker", "Super market")
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
        Rhino.Geometry.Mesh lGeometry = null;
        Rhino.Geometry.Mesh lGeometry2 = null;
        List<Rhino.Geometry.Point3d> newNodes = new List<Rhino.Geometry.Point3d>();
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                Rhino.Geometry.Mesh m = null;
                if (!DA.GetData(0, ref m)) return;
                lGeometry = m.DuplicateMesh();
                lGeometry2 = m.DuplicateMesh();
                newNodes.Clear();
                newNodes.AddRange(m.Vertices.ToPoint3dArray());
                int nNewNodes = newNodes.Count;
                el = new List<int[]>();
                for (int i = 0; i < m.Faces.Count; i++)
                {
                    if (m.Faces[i].IsQuad)
                    {
                        int[] f = new int[4] { m.Faces[i].A, m.Faces[i].B, m.Faces[i].D, m.Faces[i].C };
                        el.Add(f);
                    }
                    if (m.Faces[i].IsTriangle)
                    {
                        int[] f = new int[3] { m.Faces[i].A, m.Faces[i].B, m.Faces[i].C};
                        el.Add(f);
                    }
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
                lGeometry.Faces.Clear();
                lGeometry2.Faces.Clear();
                for (int i = 0; i < el.Count; i++)
                {
                    if (el[i].Length == 4)
                    {
                        mikity.NumericalMethodHelper.elements.isoparametricElement e = new NumericalMethodHelper.elements.isoparametricElement(el[i]);
                        eM.addElement(e);
                        lGeometry.Faces.AddFace(e.el[0], e.el[1], e.el[3], e.el[2]);
                        lGeometry2.Faces.AddFace(e.el[0], e.el[1], e.el[3], e.el[2]);
                    }
                    else if (el[i].Length == 3)
                    {
                        mikity.NumericalMethodHelper.elements.simplexElement e = new NumericalMethodHelper.elements.simplexElement(el[i]);
                        eM.addElement(e);
                        lGeometry.Faces.AddFace(e.el[0], e.el[1], e.el[2]);
                        lGeometry2.Faces.AddFace(e.el[0], e.el[1], e.el[2]);
                    }
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
        public BakeGeometry GetBKGT(Rhino.Geometry.Mesh m)
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 1;
                Guid id = d.Objects.AddMesh(m, a2);
                o.Add(id);
            });
        }

        public UpdateGeometry GetUPGR(Rhino.Geometry.Mesh m)
        {
            return new UpdateGeometry((x, y, z) =>
            {
                m.Vertices.Clear();
                for (int i = 0; i < pS.Value.__N; i++)
                {
                    m.Vertices.Add(pS.Value.particles[i, 0] +x, pS.Value.particles[i, 1] + y, +pS.Value.particles[i, 2] + z);
                }
            });
        }
        public DrawViewPortWire GetDVPW(Rhino.Geometry.Mesh m)
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
                            args.Display.DrawMeshShaded(m, new Rhino.Display.DisplayMaterial(System.Drawing.Color.FromArgb(255,System.Drawing.Color.Red), 0.2));
                            args.Display.DrawMeshWires(m, System.Drawing.Color.White);
                        }
                        else
                        {
                            args.Display.DrawMeshShaded(m, new Rhino.Display.DisplayMaterial(System.Drawing.Color.FromArgb(255, System.Drawing.Color.Lime), 0.2));
                            args.Display.DrawMeshWires(m, System.Drawing.Color.LightGreen);
                        }
                    }
                });
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("f7ee232a-ba65-41b7-95e5-6c8f32b555a2"); }
        }

    }
}
