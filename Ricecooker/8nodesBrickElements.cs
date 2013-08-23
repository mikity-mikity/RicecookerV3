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
    public class eight_nodes : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.eight-nodes-quad_elements.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }                
        public eight_nodes()
            : base("8nodes->brickElements [General Spring]", "8nodes->brickElements", "8nodes->brickElements", "Ricecooker", "Convenience store")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point1", "P1", "First point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point2", "P2", "Second point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point3", "P3", "Third point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point4", "P4", "Fourth point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point5", "P5", "Fifth point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point6", "P6", "Sixth point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point7", "P7", "Seventh point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point8", "P8", "Eighth point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Number", "nV", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Number", "nW", "", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddGenericParameter("Material", "MAT", "Material", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddGenericParameter("Gravity", "GVT", "Gravity", Grasshopper.Kernel.GH_ParamAccess.item);
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
        private const int _nNodes = 8;
        private const int _dim = 3;
        int[][] el = null;
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        mikity.NumericalMethodHelper.objects.generalSpring eM = null;
        List<Rhino.Geometry.Polyline> lGeometry = new List<Rhino.Geometry.Polyline>();
        List<Rhino.Geometry.Polyline> lGeometry2 = new List<Rhino.Geometry.Polyline>();
        List<Rhino.Geometry.Mesh> lBoundary = new List<Rhino.Geometry.Mesh>();
        List<Rhino.Geometry.Mesh> lBoundary2 = new List<Rhino.Geometry.Mesh>();
        int[] nEdgeNodes = new int[_dim];
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                GH_Point[] pointList = new GH_Point[8];
                List<GH_Point> tmpPointList = new List<GH_Point>();

                eM = new generalSpring();

                DA.GetData(0, ref pointList[0]);
                DA.GetData(1, ref pointList[1]);
                DA.GetData(2, ref pointList[2]);
                DA.GetData(3, ref pointList[3]);
                DA.GetData(4, ref pointList[4]);
                DA.GetData(5, ref pointList[5]);
                DA.GetData(6, ref pointList[6]);
                DA.GetData(7, ref pointList[7]);
                DA.GetData(8, ref nEdgeNodes[0]);
                DA.GetData(9, ref nEdgeNodes[1]);
                DA.GetData(10, ref nEdgeNodes[2]);
                GH_material mat = null;
                GH_gravity gvt = null;
                if (!DA.GetData(11, ref mat)) { return; }
                if (!DA.GetData(12, ref gvt)) { return; }
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
                lGeometry.Clear();
                lGeometry2.Clear();
                for (int k = 0; k < nEdgeNodes[2]; k++)
                {
                    for (int j = 0; j < nEdgeNodes[1]; j++)
                    {
                        Rhino.Geometry.Polyline p = new Rhino.Geometry.Polyline();
                        lGeometry.Add(p);
                        for (int i = 0; i < nEdgeNodes[0]; i++)
                        {
                            p.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                        }
                    }
                }
                for (int i = 0; i < nEdgeNodes[0]; i++)
                {
                    for (int k = 0; k < nEdgeNodes[2]; k++)
                    {
                        Rhino.Geometry.Polyline p = new Rhino.Geometry.Polyline();
                        lGeometry.Add(p);
                        for (int j = 0; j < nEdgeNodes[1]; j++)
                        {
                            p.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                        }
                    }
                }
                for (int i = 0; i < nEdgeNodes[0]; i++)
                {
                    for (int j = 0; j < nEdgeNodes[1]; j++)
                    {
                        Rhino.Geometry.Polyline p = new Rhino.Geometry.Polyline();
                        lGeometry.Add(p);
                        for (int k = 0; k < nEdgeNodes[2]; k++)
                        {
                            
                            p.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                        }
                    }
                }
                for (int i = 0; i < nEdgeNodes[0] * nEdgeNodes[1] + nEdgeNodes[1] * nEdgeNodes[2] + nEdgeNodes[2] * nEdgeNodes[0]; i++)
                {
                    lGeometry2.Add(new Rhino.Geometry.Polyline());
                }
                lBoundary.Clear();
                lBoundary2.Clear();
                ///////////////////////////////////////////////////////////////
                Rhino.Geometry.Mesh m = new Rhino.Geometry.Mesh();
                Rhino.Geometry.Mesh m2 = new Rhino.Geometry.Mesh();
                lBoundary.Add(m);
                lBoundary2.Add(m2);
                for (int k = 0; k < nEdgeNodes[2]; k++)
                {
                    for (int j = 0; j < nEdgeNodes[1]; j++)
                    {
                        int i = 0;
                        m.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                        m2.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                    }
                }
                el = MathUtil.isoparametricElements(new int[2]{nEdgeNodes[1],nEdgeNodes[2]});
                for (int i = 0; i < el.Length; i++)
                {
                    m.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                    m2.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                }
                m = new Rhino.Geometry.Mesh();
                m2 = new Rhino.Geometry.Mesh();
                lBoundary.Add(m);
                lBoundary2.Add(m2);
                for (int k = 0; k < nEdgeNodes[2]; k++)
                {
                    for (int j = 0; j < nEdgeNodes[1]; j++)
                    {
                        int i = nEdgeNodes[0] - 1;

                        m.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                        m2.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                    }
                }
                el = MathUtil.isoparametricElements(new int[2] { nEdgeNodes[1], nEdgeNodes[2] });
                for (int i = 0; i < el.Length; i++)
                {
                    m.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                    m2.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                }

                ///////////////////////////////////////////////////////////////
                m = new Rhino.Geometry.Mesh();
                m2 = new Rhino.Geometry.Mesh();
                lBoundary.Add(m);
                lBoundary2.Add(m2);
                for (int j = 0; j < nEdgeNodes[1]; j++)
                {
                    for (int i = 0; i < nEdgeNodes[0]; i++)
                    {
                        int k = 0;
                        m.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                        m2.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                    }
                }
                el = MathUtil.isoparametricElements(new int[2] { nEdgeNodes[0], nEdgeNodes[1] });
                for (int i = 0; i < el.Length; i++)
                {
                    m.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                    m2.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                }
                m = new Rhino.Geometry.Mesh();
                m2 = new Rhino.Geometry.Mesh();
                lBoundary.Add(m);
                lBoundary2.Add(m2);
                for (int j = 0; j < nEdgeNodes[1]; j++)
                {
                    for (int i = 0; i < nEdgeNodes[0]; i++)
                    {
                        int k = nEdgeNodes[2] - 1;

                        m.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                        m2.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                    }
                }
                el = MathUtil.isoparametricElements(new int[2] { nEdgeNodes[0], nEdgeNodes[1] });
                for (int i = 0; i < el.Length; i++)
                {
                    m.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                    m2.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                }

                ///////////////////////////////////////////////////////////////
                m = new Rhino.Geometry.Mesh();
                m2 = new Rhino.Geometry.Mesh();
                lBoundary.Add(m);
                lBoundary2.Add(m2);
                for (int i = 0; i < nEdgeNodes[0]; i++)
                {
                    for (int k = 0; k < nEdgeNodes[2]; k++)
                    {
                        int j = 0;
                        m.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                        m2.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                    }
                }
                el = MathUtil.isoparametricElements(new int[2] { nEdgeNodes[2], nEdgeNodes[0] });
                for (int i = 0; i < el.Length; i++)
                {
                    m.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                    m2.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                }
                m = new Rhino.Geometry.Mesh();
                m2 = new Rhino.Geometry.Mesh();
                lBoundary.Add(m);
                lBoundary2.Add(m2);
                for (int i = 0; i < nEdgeNodes[0]; i++)
                {
                    for (int k = 0; k < nEdgeNodes[2]; k++)
                    {
                        int j = nEdgeNodes[1] - 1;

                        m.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                        m2.Vertices.Add(particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1], particles[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]);
                    }
                }
                el = MathUtil.isoparametricElements(new int[2] { nEdgeNodes[2], nEdgeNodes[0] });
                for (int i = 0; i < el.Length; i++)
                {
                    m.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                    m2.Faces.AddFace(el[i][0], el[i][1], el[i][3], el[i][2]);
                }
                
                this.DVPW = GetDVPW(lGeometry, lBoundary);
                pS.DVPW = GetDVPW(lGeometry2,lBoundary2);
                pS.UPGR = GetUPGR(lGeometry2,lBoundary2);
                
            }

            DA.SetData(0, pS);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("d7226cc1-2fb8-4640-b5e6-dd31f0f02c24"); }
        }

        public UpdateGeometry GetUPGR(List<Rhino.Geometry.Polyline> m, List<Rhino.Geometry.Mesh> m2)
        {
            return new UpdateGeometry((x, y, z) =>
            {
                for (int k = 0; k < nEdgeNodes[2]; k++)
                {
                    for (int j = 0; j < nEdgeNodes[1]; j++)
                    {
                        Rhino.Geometry.Polyline p = m[j + k * nEdgeNodes[1]];
                        p.Clear();
                        for (int i = 0; i < nEdgeNodes[0]; i++)
                        {
                            p.Add(pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0]+x, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1]+y, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]+z);
                        }
                    }
                }
                for (int i = 0; i < nEdgeNodes[0]; i++)
                {
                    for (int k = 0; k < nEdgeNodes[2]; k++)
                    {
                        Rhino.Geometry.Polyline p = m[k +i * nEdgeNodes[2]+nEdgeNodes[1]*nEdgeNodes[2]];
                        p.Clear();
                        for (int j = 0; j < nEdgeNodes[1]; j++)
                        {
                            p.Add(pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0]+x, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1]+y, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]+z);
                        }
                    }
                }
                for (int i = 0; i < nEdgeNodes[0]; i++)
                {
                    for (int j = 0; j < nEdgeNodes[1]; j++)
                    {
                        Rhino.Geometry.Polyline p = m[j + i * nEdgeNodes[1] + nEdgeNodes[1] * nEdgeNodes[2]+nEdgeNodes[0]*nEdgeNodes[2]];
                        p.Clear();
                        for (int k = 0; k < nEdgeNodes[2]; k++)
                        {

                            p.Add(pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0] + x, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1] + y, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2] + z);
                        }
                    }
                }
                m2[0].Vertices.Clear();
                m2[1].Vertices.Clear();
                m2[2].Vertices.Clear();
                m2[3].Vertices.Clear();
                m2[4].Vertices.Clear();
                m2[5].Vertices.Clear();

                for (int k = 0; k < nEdgeNodes[2]; k++)
                {
                    for (int j = 0; j < nEdgeNodes[1]; j++)
                    {
                        int i = 0;
                        m2[0].Vertices.Add(pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0]+x, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1]+y, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]+z);
                    }
                }
                for (int k = 0; k < nEdgeNodes[2]; k++)
                {
                    for (int j = 0; j < nEdgeNodes[1]; j++)
                    {
                        int i = nEdgeNodes[0] - 1;
                        m2[1].Vertices.Add(pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0]+x, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1]+y, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]+z);
                    }
                }
                for (int j = 0; j < nEdgeNodes[1]; j++)
                {
                    for (int i = 0; i < nEdgeNodes[0]; i++)
                    {
                        int k = 0;
                        m2[2].Vertices.Add(pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0]+x, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1]+y, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]+z);
                    }
                }
                for (int j = 0; j < nEdgeNodes[1]; j++)
                {
                    for (int i = 0; i < nEdgeNodes[0]; i++)
                    {
                        int k = nEdgeNodes[2] - 1;
                        m2[3].Vertices.Add(pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0]+x, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1]+y, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]+z);
                    }
                }
                for (int i = 0; i < nEdgeNodes[0]; i++)
                {
                    for (int k = 0; k < nEdgeNodes[2]; k++)
                    {
                        int j = 0;
                        m2[4].Vertices.Add(pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0]+x, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1]+y, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]+z);
                    }
                }
                for (int i = 0; i < nEdgeNodes[0]; i++)
                {
                    for (int k = 0; k < nEdgeNodes[2]; k++)
                    {
                        int j = nEdgeNodes[1] - 1;
                        m2[5].Vertices.Add(pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][0]+x, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][1]+y, pS.Value[i + j * nEdgeNodes[0] + k * nEdgeNodes[0] * nEdgeNodes[1]][2]+z);
                    }
                }
            });

        }
        public DrawViewPortWire GetDVPW(List<Rhino.Geometry.Polyline> _m,List<Rhino.Geometry.Mesh> _m2)
        {
            return new DrawViewPortWire((args) =>
            {
                if (Hidden)
                {
                    return;
                }
                if (this.Attributes.Selected)
                {
                    foreach (Rhino.Geometry.Polyline p in _m)
                    {
                        args.Display.DrawPolyline(p, System.Drawing.Color.Red);
                        args.Display.DrawPoints(p,Rhino.Display.PointStyle.Simple,3, System.Drawing.Color.Red);
                    }
                    /*foreach (Rhino.Geometry.Mesh f in _m2)
                    {
                        args.Display.DrawMeshShaded(f, new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    }*/
                    args.Display.DrawMeshShaded(_m2[0], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    args.Display.DrawMeshShaded(_m2[1], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    args.Display.DrawMeshShaded(_m2[2], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    args.Display.DrawMeshShaded(_m2[3], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    args.Display.DrawMeshShaded(_m2[4], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    args.Display.DrawMeshShaded(_m2[5], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    
                }
                else
                {
                    foreach (Rhino.Geometry.Polyline p in _m)
                    {
                        args.Display.DrawPolyline(p, System.Drawing.Color.DarkRed);
                        args.Display.DrawPoints(p, Rhino.Display.PointStyle.Simple, 3, System.Drawing.Color.Yellow);
                    }
//                    foreach (Rhino.Geometry.Mesh f in _m2)
//                    {
//                        args.Display.DrawMeshShaded(f, new Rhino.Display.DisplayMaterial(System.Drawing.Color.Violet, 0.3));
//                    }
                    args.Display.DrawMeshShaded(_m2[0], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    args.Display.DrawMeshShaded(_m2[1], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    args.Display.DrawMeshShaded(_m2[2], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    args.Display.DrawMeshShaded(_m2[3], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    args.Display.DrawMeshShaded(_m2[4], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                    args.Display.DrawMeshShaded(_m2[5], new Rhino.Display.DisplayMaterial(System.Drawing.Color.Red, 0.3));
                }

            });
        }

    }
}
