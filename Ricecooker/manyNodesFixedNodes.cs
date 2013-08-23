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
    public class many_nodes_fixed_nodes : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.many-nodes-fixed_nodes.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
                
        public many_nodes_fixed_nodes()
            : base("manyNodes->fixedNodes [Fixed Node]", "manyNodes->fixedNodes", "manyNodes->fixedNodes", "Ricecooker", "Convenience store")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Ps", "First point", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddBooleanParameter("fixX", "X", "fixX", Grasshopper.Kernel.GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("fixY", "Y", "fixY", Grasshopper.Kernel.GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("fixZ", "Z", "fixZ", Grasshopper.Kernel.GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("group", "g", "if true, one group of points will be created, otherwise, you can manipulate each point separately", Grasshopper.Kernel.GH_ParamAccess.item, true);
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
            base.BakeGeometry(doc, att, obj_ids);
        }

        public override void DrawViewportWires(Grasshopper.Kernel.IGH_PreviewArgs args)
        {
            if (this.DVPW != null)
            {
                this.DVPW(args);
            }
            base.DrawViewportWires(args);
        }
        private int _nNodes=0;
        GH_particleSystem pS;
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;
        Rhino.Geometry.Point3d[] lGeometry;
        Rhino.Geometry.Point3d[] lGeometry2;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {

                List<GH_Point> pointList = new List<GH_Point>();
                DA.GetDataList(0, pointList);
                bool x = true, y = true, z = true;
                if (!DA.GetData(1, ref x)) return;
                if (!DA.GetData(2, ref y)) return;
                if (!DA.GetData(3, ref z)) return;
                bool isGroup = true;
                if (!DA.GetData(4, ref isGroup)) return;
                _nNodes = pointList.Count;
                mikity.NumericalMethodHelper.particle[] particles = new mikity.NumericalMethodHelper.particle[_nNodes];
                for (int i = 0; i < _nNodes; i++)
                {
                    particles[i] = new particle(pointList[i].Value.X, pointList[i].Value.Y, pointList[i].Value.Z);
                }
                pS = new GH_particleSystem(particles);
                node[] lNodes = new node[_nNodes];
                for (int i = 0; i < _nNodes; i++)
                {
                    lNodes[i] = new node(i);
                    lNodes[i].copyFrom(pS.Value.particles);
                }
                if (isGroup)
                {
                    fixedNodes fN = new fixedNodes(x, y, z);

                    for (int i = 0; i < _nNodes; i++)
                    {
                        fN.addNode(lNodes[i]);
                    }
                    pS.Value.addObject(fN);
                }
                else
                {
                    for (int i = 0; i < _nNodes; i++)
                    {
                        fixedNodes fN = new fixedNodes(x, y, z);
                        fN.addNode(lNodes[i]);
                        pS.Value.addObject(fN);
                    }
                }
                lGeometry = new Rhino.Geometry.Point3d[_nNodes];
                lGeometry2 = new Rhino.Geometry.Point3d[_nNodes];
                for (int i = 0; i < _nNodes; i++)
                {
                    lGeometry[i] = new Rhino.Geometry.Point3d(particles[i][0], particles[i][1], particles[i][2]);
                }
                this.DVPW = GetDVPW(lGeometry);
                this.BKGT = GetBKGT(lGeometry);
                pS.DVPW = GetDVPW(lGeometry2);
                pS.UPGR = GetUPGR(lGeometry2);
                pS.BKGT = GetBKGT(lGeometry2);
            }
            DA.SetData(0, pS);

        }
        public BakeGeometry GetBKGT(Rhino.Geometry.Point3d[] m)
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 3;
                Rhino.Collections.RhinoList<Guid> id  =d.Objects.AddPoints(m, a2);
                o.AddRange(id);
            });
        }

        public UpdateGeometry GetUPGR(Rhino.Geometry.Point3d[] m)
        {
            return new UpdateGeometry((x, y, z) =>
            {
                for (int i = 0; i < pS.Value.__N; i++)
                {
                    m[i]=new Rhino.Geometry.Point3d(pS.Value.particles[i, 0] + x, pS.Value.particles[i, 1] + y, pS.Value.particles[i, 2] + z);
                }
            });
        }
        public DrawViewPortWire GetDVPW(Rhino.Geometry.Point3d[] m)
        {
            return new DrawViewPortWire((args) =>
            {
                if (Hidden)
                {
                    return;
                }
                if (this.Attributes.Selected)
                {
                    args.Display.DrawPoints(m, Rhino.Display.PointStyle.Simple, 8, System.Drawing.Color.Red);
                  
                }
                else
                {
                    args.Display.DrawPoints(m, Rhino.Display.PointStyle.Simple, 8, System.Drawing.Color.DarkRed);
                }

            });
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("736f35e5-328f-4955-b17d-3e343bc869b9"); }
        }

    }
}
