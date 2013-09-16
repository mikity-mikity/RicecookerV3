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
    public class surface_nofal_Forces : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.surface-nodal_forces.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }

        public surface_nofal_Forces()
            : base("surface->nodalForces [Nodal Force]", "surface->nodalForces", "surface->nodalForces", "Ricecooker", "Super market")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "s", "Surface", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nU", "", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "nV", "", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddVectorParameter("Force", "Force", "Force (Vector3d)", Grasshopper.Kernel.GH_ParamAccess.item, new Rhino.Geometry.Vector3d(0, 0, -1));
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particle System", "pS", "Particle System", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        public override void DrawViewportWires(Grasshopper.Kernel.IGH_PreviewArgs args)
        {
            if (Hidden)
            {
                return;
            }
            if (this.Attributes.Selected)
            {
                for (int i = 0; i < lGeometry.Length; i++)
                {
                    args.Display.DrawDirectionArrow(lGeometry[i], v, System.Drawing.Color.LightBlue);
                }
            }
            else
            {
                for (int i = 0; i < lGeometry.Length; i++)
                {
                    args.Display.DrawDirectionArrow(lGeometry[i], v, System.Drawing.Color.DarkBlue);
                }
            }
            base.DrawViewportWires(args);
        }
        private const int _nNodes = 4;
        private const int _dim = 2;

        GH_particleSystem pS=null;
        List<Rhino.Geometry.Point3d> newNodes = new List<Rhino.Geometry.Point3d>();
        Rhino.Geometry.Point3d[] lGeometry;
        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d(0, 0, -1);
        nodalForce nF;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!DA.GetData(3, ref v)) return;
            if (!FriedChiken.isInitialized)
            {
                Rhino.Geometry.Surface s = null;
                if (!DA.GetData(0, ref s)) return;
                Rhino.Geometry.Interval uDomain = s.Domain(0);
                Rhino.Geometry.Interval vDomain = s.Domain(1);
                int[] nEdgeNodes = new int[_dim];
                DA.GetData(1, ref nEdgeNodes[0]);
                DA.GetData(2, ref nEdgeNodes[1]);
                for (int i = 0; i < _dim; i++)
                {
                    if (nEdgeNodes[i] < 2)
                    {
                        AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "Integers must be greater than or equal to 2");
                        return;
                    }
                }
                newNodes.Clear();
                for (int i = 0; i < nEdgeNodes[1]; i++)
                {
                    for (int j = 0; j < nEdgeNodes[0]; j++)
                    {
                        newNodes.Add(s.PointAt(uDomain.T0 + (uDomain.T1 - uDomain.T0) / (nEdgeNodes[0]-1) * j, vDomain.T0 + (vDomain.T1 - vDomain.T0) / (nEdgeNodes[1]-1) * i));
                    
                    }
                }
                int nNewNodes = newNodes.Count;

                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    particles[i] = new mikity.NumericalMethodHelper.particle(newNodes[i][0], newNodes[i][1], newNodes[i][2]);
                }
                pS = new GH_particleSystem(particles);

                node[] lNodes = new node[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    lNodes[i] = new node(i);
                    lNodes[i].copyFrom(pS.Value.particles);
                }
                nF = new nodalForce(v.X, v.Y, v.Z);
                for (int i = 0; i < nNewNodes; i++)
                {
                    nF.addNode(lNodes[i]);
                }
                pS.Value.addObject(nF);
                lGeometry = new Rhino.Geometry.Point3d[nNewNodes];
                for (int i = 0; i < nNewNodes; i++)
                {
                    lGeometry[i] = new Rhino.Geometry.Point3d(particles[i][0], particles[i][1], particles[i][2]);
                }
            }
            else
            {
                nF.forceX = v.X;
                nF.forceY = v.Y;
                nF.forceZ = v.Z;
            }

            DA.SetData(0, pS);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("a6ba61b5-1601-49f1-ac9c-85b8aaae36f9"); }
        }

    }
}
