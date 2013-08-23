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
    public class four_nodes_quad_elements : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.four-nodes-quad_elements.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
        
        public four_nodes_quad_elements()
            : base("4nodes->quad/triElements [General Spring]", "4nodes->quad/triElements", "4nodes->quad/triElements", "Ricecooker", "Convenience store")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point1", "P1", "First point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point2", "P2", "Second point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point3", "P3", "Third point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point4", "P4", "Fourth point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Number", "nV", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddGenericParameter("Material", "MAT", "Material", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddGenericParameter("Gravity", "GVT", "Gravity", Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particle System", "pS", "Particle System", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        System.Windows.Forms.ToolStripMenuItem __m1, __m2, __m3, __m4;
        enum subdivide
        {
            quad,triA,triB,triC
        }
        private subdivide _subdv = subdivide.triA;
        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            __m1 = Menu_AppendItem(menu, "quadMesh", Menu_MyCustomItemClicked);
            __m2 = Menu_AppendItem(menu, "triangulation_A", Menu_MyCustomItemClicked);
            __m3 = Menu_AppendItem(menu, "triangulation_B", Menu_MyCustomItemClicked);
            __m4 = Menu_AppendItem(menu, "triangulation_C", Menu_MyCustomItemClicked);
            Menu_AppendSeparator(menu);
            if (_subdv == subdivide.quad)
            {
                __m1.CheckState = System.Windows.Forms.CheckState.Checked;
            }
            else
            {
                __m1.CheckState = System.Windows.Forms.CheckState.Unchecked;
            }
            if (_subdv == subdivide.triA)
            {
                __m2.CheckState = System.Windows.Forms.CheckState.Checked;
            }
            else
            {
                __m2.CheckState = System.Windows.Forms.CheckState.Unchecked;
            }
            if (_subdv == subdivide.triB)
            {
                __m3.CheckState = System.Windows.Forms.CheckState.Checked;
            }
            else
            {
                __m3.CheckState = System.Windows.Forms.CheckState.Unchecked;
            }
            if (_subdv == subdivide.triC)
            {
                __m4.CheckState = System.Windows.Forms.CheckState.Checked;
            }
            else
            {
                __m4.CheckState = System.Windows.Forms.CheckState.Unchecked;
            }
        }
        private void Menu_MyCustomItemClicked(Object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem _m = sender as System.Windows.Forms.ToolStripMenuItem;

            if (_m == __m1)
            {
                _m.CheckState = System.Windows.Forms.CheckState.Checked;
                _subdv = subdivide.quad;
                this.ExpireSolution(true);
            }
            if (_m == __m2)
            {
                _m.CheckState = System.Windows.Forms.CheckState.Checked;
                _subdv = subdivide.triA;
                this.ExpireSolution(true);
            }
            if (_m == __m3)
            {
                _m.CheckState = System.Windows.Forms.CheckState.Checked;
                _subdv = subdivide.triB;
                this.ExpireSolution(true);
            }
            if (_m == __m4)
            {
                _m.CheckState = System.Windows.Forms.CheckState.Checked;
                _subdv = subdivide.triC;
                this.ExpireSolution(true);
            }
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            int f=1;
            reader.TryGetInt32("subdivide", ref f);
            _subdv=(subdivide)f;
            return base.Read(reader);
        }
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            int f = (int)_subdv;
            writer.SetInt32("subdivide", f);
            return base.Write(writer);
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
        private const int _nNodes = 4;
        private const int _dim = 2;
        int[][] el = null;
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;
        mikity.NumericalMethodHelper.objects.generalSpring eM = null;
        Rhino.Geometry.Mesh lGeometry;
        Rhino.Geometry.Mesh lGeometry2;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            
            if (!FriedChiken.isInitialized)
            {
                GH_Point[] pointList = new GH_Point[4];
                List<GH_Point> tmpPointList = new List<GH_Point>();

                int[] nEdgeNodes = new int[_dim];
                eM = new generalSpring();
                DA.GetData(0, ref pointList[0]);
                DA.GetData(1, ref pointList[1]);
                DA.GetData(2, ref pointList[2]);
                DA.GetData(3, ref pointList[3]);
                DA.GetData(4, ref nEdgeNodes[0]);
                DA.GetData(5, ref nEdgeNodes[1]);
                GH_material mat = null;
                GH_gravity gvt = null;
                if (!DA.GetData(6, ref mat)) { return; }
                if (!DA.GetData(7, ref gvt)) { return; }
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
                bool isJoin = true;
                if (isJoin)
                {
                    pS.simplify(el);
                }

                for (int i = 0; i < nElements; i++)
                {
                    if (_subdv == subdivide.quad)
                    {
                        eM.addElement(new mikity.NumericalMethodHelper.elements.isoparametricElement(el[i]));
                    }
                    if (_subdv == subdivide.triA)
                    {
                        eM.addElement(new mikity.NumericalMethodHelper.elements.simplexElement(new int[3] { el[i][0], el[i][1], el[i][3] }));
                        eM.addElement(new mikity.NumericalMethodHelper.elements.simplexElement(new int[3] { el[i][0], el[i][3], el[i][2] }));
                    }
                    if (_subdv == subdivide.triB)
                    {
                        int S = i % (nEdgeNodes[0]-1);
                        int T = (i - S) / (nEdgeNodes[0]-1);
                        if (T % 2 == 1)
                        {
                            S++;
                        }
                        if (S % 2 == 0)
                        {
                            eM.addElement(new mikity.NumericalMethodHelper.elements.simplexElement(new int[3] { el[i][0], el[i][1], el[i][3] }));
                            eM.addElement(new mikity.NumericalMethodHelper.elements.simplexElement(new int[3] { el[i][0], el[i][3], el[i][2] }));
                        }
                        else
                        {
                            eM.addElement(new mikity.NumericalMethodHelper.elements.simplexElement(new int[3] { el[i][0], el[i][1], el[i][2] }));
                            eM.addElement(new mikity.NumericalMethodHelper.elements.simplexElement(new int[3] { el[i][2], el[i][1], el[i][3] }));
                        }
                    }
                    if (_subdv == subdivide.triC)
                    {
                        int S = i % (nEdgeNodes[0]-1);
                        if (S % 2 == 0)
                        {
                            eM.addElement(new mikity.NumericalMethodHelper.elements.simplexElement(new int[3] { el[i][0], el[i][1], el[i][3] }));
                            eM.addElement(new mikity.NumericalMethodHelper.elements.simplexElement(new int[3] { el[i][0], el[i][3], el[i][2] }));
                        }
                        else
                        {
                            eM.addElement(new mikity.NumericalMethodHelper.elements.simplexElement(new int[3] { el[i][0], el[i][1], el[i][2] }));
                            eM.addElement(new mikity.NumericalMethodHelper.elements.simplexElement(new int[3] { el[i][2], el[i][1], el[i][3] }));
                        }
                    }
                }
                if (_subdv == subdivide.quad)
                {
                    nElements *= 1;
                }
                else
                {
                    nElements *= 2;
                }
                eM.setMaterial(mat.Value, gvt.Value);
                pS.Value.addObject(eM);
                lGeometry=new Rhino.Geometry.Mesh();
                lGeometry2=new Rhino.Geometry.Mesh();
                lGeometry.Vertices.Clear();
                lGeometry.Faces.Clear();
                lGeometry2.Faces.Clear();
                for (int i = 0; i < pS.Value.__N; i++)
                {
                    lGeometry.Vertices.Add(particles[i][0], particles[i][1], particles[i][2]);
                }

                for (int i = 0; i < nElements; i++)
                {
                    if (_subdv == subdivide.quad)
                    {
                        lGeometry.Faces.AddFace(eM.elemList[i].el[0], eM.elemList[i].el[1], eM.elemList[i].el[3], eM.elemList[i].el[2]);
                    }
                    else
                    {
                        lGeometry.Faces.AddFace(eM.elemList[i].el[0], eM.elemList[i].el[1], eM.elemList[i].el[2]);
                    }
                }
                for (int i = 0; i < nElements; i++)
                {
                    if (_subdv == subdivide.quad)
                    {
                        lGeometry2.Faces.AddFace(eM.elemList[i].el[0], eM.elemList[i].el[1], eM.elemList[i].el[3], eM.elemList[i].el[2]);
                    }
                    else
                    {
                        lGeometry2.Faces.AddFace(eM.elemList[i].el[0], eM.elemList[i].el[1], eM.elemList[i].el[2]);
                    }
                }
                this.DVPW = GetDVPW(lGeometry);
                pS.DVPW = GetDVPW(lGeometry2);
                pS.UPGR = GetUPGR(lGeometry2);
                pS.BKGT = GetBKGT(lGeometry2);
            }
            
            DA.SetData(0, pS);
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
                        m.Vertices.Add(pS.Value.particles[i, 0] + x, pS.Value.particles[i, 1] + y, pS.Value.particles[i, 2] + z);
                    }

                });
        }
        public DrawViewPortWire GetDVPW(Rhino.Geometry.Mesh m)
        {
            return new DrawViewPortWire((args) =>
            {
                if (this.Hidden)
                {
                    return;
                }
                if (this.Attributes.Selected)
                {
                    args.Display.DrawMeshShaded(m, new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.1));
                    args.Display.DrawMeshWires(m, System.Drawing.Color.FromArgb(0, System.Drawing.Color.White));
                }
                else
                {
                    args.Display.DrawMeshShaded(m, new Rhino.Display.DisplayMaterial(System.Drawing.Color.Lime, 0.1));
                    args.Display.DrawMeshWires(m, System.Drawing.Color.FromArgb(0, System.Drawing.Color.LightGreen));
                }

            });
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("28cb880b-3fa3-4ac3-b593-ea32f47ea34c"); }
        }


    }
}
