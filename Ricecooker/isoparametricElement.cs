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
using mikity.NumericalMethodHelper.materials;
namespace mikity.ghComponents
{
 
    public class GH_isoparametricElement : GH_Goo<mikity.NumericalMethodHelper.elements.isoparametricElement>
    {
        public override IGH_Goo Duplicate()
        {
            throw new NotImplementedException();
        }

        public override bool IsValid
        {
            get { if (Value != null)return true;
            return false;

            }
        }

        public override string ToString()
        {
            return "GH_isoparametricElement";
        }

        public override string TypeDescription
        {
            get { return "Creates isoparametric elements generally used in finite element framework"; }
        }

        public override string TypeName
        {
            get { return "IsoparametricElement"; }
        }
    }
    /*
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class ghIsoParamComponent : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.icon6.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
        public ghIsoParamComponent()
            : base("isoparametricElements", "isoElem", "generates isoparametric elements", "Ricecooker", "Primitives")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Particle System", "pS", "Particle System",Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddGenericParameter("2, 4, or 8 node numbers are required", "Elem", "node indices for element",Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddBooleanParameter("Join", "Join", "If true, join particles that are identical",Grasshopper.Kernel.GH_ParamAccess.item,false);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("IntegralPoints", "iP", "integral points",Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddGenericParameter("Elements", "E", "Elements", Grasshopper.Kernel.GH_ParamAccess.item);
//            pManager.AddVectorParameter("BaseVectors", "bV", "base vectors atatched to the integral points",Grasshopper.Kernel.GH_ParamAccess.tree);
//            pManager.AddPointParameter("Element Nodes", "eN", "element nodes",Grasshopper.Kernel.GH_ParamAccess.list);
//            pManager.AddVectorParameter("ElementGradient", "eG", "equivalent nodal forces yileded from stress distribution",Grasshopper.Kernel.GH_ParamAccess.list);
//            pManager.AddVectorParameter("ElementGradient", "eL", "equivalent nodal forces yileded by volume force",Grasshopper.Kernel.GH_ParamAccess.list);
        }
        GH_isoparametricElement e;
        List<Rhino.Geometry.Point3d> lp;
        List<Rhino.Geometry.Vector3d> q;
        List<Rhino.Geometry.Vector3d> q2;
        List<Rhino.Geometry.Point3d> eN;
        GH_Structure<GH_Vector> f;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                int _f = DA.Iteration;
                int d = DA.ParameterTargetIndex(0);
                //            List<Rhino.Geometry.Point3d> pointList = new List<Rhino.Geometry.Point3d>();
                GH_particleSystem pS = null;
                if (!DA.GetData(0, ref pS)) return;
                GH_vectorINT elem = null;
                if (!DA.GetData(1, ref elem)) return;
                bool isJoin = false;
                if (!DA.GetData(2, ref isJoin)) return;
                vectorINT el = elem.Value;
                int nNodes = el.nElem;
                if (nNodes == 2 || nNodes == 4 || nNodes == 8) { }
                else
                {
                    return;
                }
                e = new GH_isoparametricElement();
                if (isJoin)
                {
                    pS.findFirst(el);
                }
                e.Value = new isoparametricElement(el);
                e.Value.copyFrom(pS.Value.particles);
                material _em = new material((_e, _i) =>
                {
                    _e.gravity.zeros();
                    _e.gravity.rawData[2] = -1;
                    return _i.stress2.zeros().plus_xA(1.0, _i.invMetric);
                });
                e.Value.setMaterial(_em);
                e.Value.Update();
                
                lp = new List<Rhino.Geometry.Point3d>();
                List<Rhino.Geometry.Vector3d> baseList = new List<Rhino.Geometry.Vector3d>();
                f = new GH_Structure<GH_Vector>();
                q = new List<Rhino.Geometry.Vector3d>();
                q2 = new List<Rhino.Geometry.Vector3d>();
                eN = new List<Rhino.Geometry.Point3d>();
                GH_Vector t = null;
                for (int i = 0; i < e.Value.nIntPoints; i++)
                {
                    lp.Add(new Rhino.Geometry.Point3d(e.Value.intPoints[i][0], e.Value.intPoints[i][1], e.Value.intPoints[i][2]));
                    for (int j = 0; j < e.Value.__N; j++)
                    {
                        //int j = 0;
                        t = new GH_Vector(new Rhino.Geometry.Vector3d(e.Value.intPoints[i].covariantBases[j, 0], e.Value.intPoints[i].covariantBases[j, 1], e.Value.intPoints[i].covariantBases[j, 2]));
                        f.Append(t, new GH_Path(new int[2] { _f, i }));
                    }
                }

                matrix vs = e.Value.decomposeElemGrad();
                matrix vs2 = e.Value.decomposeElemLoad();
                matrix nodes = e.Value.getNodes();
                for (int i = 0; i < e.Value.nNodes; i++)
                {
                    q.Add(new Rhino.Geometry.Vector3d(-vs[i, 0], -vs[i, 1], -vs[i, 2]));
                    q2.Add(new Rhino.Geometry.Vector3d(vs2[i, 0], vs2[i, 1], vs2[i, 2]));
                    eN.Add(new Rhino.Geometry.Point3d(nodes[i, 0], nodes[i, 1], nodes[i, 2]));
                }
            }
//            DA.SetData(0,pS);
            DA.SetDataList(0, lp);
            DA.SetData(1, e);
            //DA.SetDataTree(2, f);
            //DA.SetDataList(3, eN);
            //DA.SetDataList(4, q);
            //DA.SetDataList(5, q2);
            //DA.SetDataList(1, timeSequence);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("77f8aeae-b2f1-4476-b62e-f744b7b1ed04"); }
        }

    }*/
 
}
