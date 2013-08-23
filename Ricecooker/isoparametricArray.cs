using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mikity;
using mikity.LinearAlgebra;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using mikity.NumericalMethodHelper;
namespace mikity.ghComponents
{
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    /*
    public class isoparametricPointArray : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.icon3.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }

        public isoparametricPointArray()
            : base("isoparametricPointArray", "PointArray", "PointArray", "Ricecooker", "Primitives")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddPointParameter("Points", "P", "End Nodes", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddGenericParameter("Number", "N", "Number of Nodes on the edges. vectorINT is also acceptable in order to set different numbers for u,v,w directions.",Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("Points", "P", "Points", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_GenericParam("Quads", "QE", "Element definitions", Grasshopper.Kernel.GH_ParamAccess.list);
        }
        List<Rhino.Geometry.Point3d> newPointList = null;
        List<GH_vectorINT> el = null;

        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                newPointList = new List<Rhino.Geometry.Point3d>();
                el = new List<GH_vectorINT>();
                new List<GH_vectorINT>();
                List<GH_Point> pointList = new List<GH_Point>();
                List<GH_Point> tmpPointList = new List<GH_Point>();
                GH_Structure<GH_Point> f = new GH_Structure<GH_Point>();
                List<int> nIntMed = new List<int>(); //端点から端点までの節点数。2以上
                GH_vectorINT _nIntMed = null;
                if (!DA.GetDataList(0, pointList)) { return; }
                if (!DA.GetData(1, ref _nIntMed)) { return; }
                int nNodes = pointList.Count;
                int m = 0;
                if (nNodes == 2) m = 1;
                else if (nNodes == 4) m = 2;
                else if (nNodes == 8) m = 3;
                else
                {
                    AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "Only 2, 4, or 8 nodes are accepted.");
                    return;
                }
                if (_nIntMed.Value.nElem == 1)
                {
                    for (int i = 0; i < m; i++)
                    {
                        nIntMed.Add(_nIntMed[0]);
                    }
                }
                else
                {
                    for (int i = 0; i < m; i++)
                    {
                        nIntMed.Add(_nIntMed[i]);
                    }
                }
                if (nIntMed.Count == m)
                {
                    for (int i = 0; i < m; i++)
                    {
                        if (nIntMed[i] < 2)
                        {
                            AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "Integers must be greater than or equal to 2");
                            return;
                        }
                    }
                }
                else
                {
                    AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Warning, String.Format("one or {0} integers required.", m));
                    return;
                }

                //形状関数作成用数列
                matrixINT psi = mikity.MathUtil.generate(m, 2);
                matrixINT phi = -2 * psi + 1;
                //要素内座標生成用数列
                matrixINT psi2 = mikity.MathUtil.generate(m, nIntMed);
                int nNewNodes = psi2.nRow;
                for (int i = 0; i < nNewNodes; i++)
                {
                    newPointList.Add(new Rhino.Geometry.Point3d(0, 0, 0));
                }
                //要素内座標生成元
                double[][] __u = new double[m][];
                for (int i = 0; i < m; i++)
                {
                    __u[i] = new double[nIntMed[i]];
                }
                //後ほど、外部から入力できるようにする。
                for (int j = 0; j < m; j++)
                {
                    for (int i = 0; i < __u[j].Count(); i++)
                    {
                        __u[j][i] = i * (1.0d / (nIntMed[j] - 1));
                    }
                }
                //要素内座標
                matrix co = matrix.zeros(nNewNodes, m);
                for (int i = 0; i < nNewNodes; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        co[i, j] = __u[j][psi2[i, j]];
                    }
                }
                //位置ベクトルに負荷する重み
                matrix wt = matrix.zeros(nNewNodes, nNodes);
                for (int i = 0; i < nNewNodes; i++)
                {
                    for (int j = 0; j < nNodes; j++)
                    {
                        wt[i, j] = 1.0;

                        for (int k = 0; k < m; k++)
                        {
                            wt[i, j] = wt[i, j] * (co[i, k] * phi[j, k] + psi[j, k]);
                        }
                    }
                }
                for (int i = 0; i < nNewNodes; i++)
                {
                    for (int j = 0; j < nNodes; j++)
                    {
                        newPointList[i] += pointList[j].Value * wt[i, j];
                    }
                }

                //int mm = nIntMed - 1;
                List<int> mm = new List<int>();
                for (int i = 0; i < m; i++)
                {
                    mm.Add(nIntMed[i] - 1);
                }
                //要素数
                int nE = 1;
                for (int i = 0; i < m; i++)
                {
                    nE *= mm[i];
                }
                //int nE = MathUtil.__pow_INT_INT(mm, m);
                int nN = MathUtil.__pow_INT_INT(2, m);
                //要素生成用数列（オフセット値）
                matrixINT psi3 = mikity.MathUtil.generate(m, mm);
                //もととなる要素
                matrixINT psi4 = mikity.MathUtil.generate(m, 2);
                int[] ss = new int[m];
                ss[0] = 1;
                for (int k = 1; k < m; k++)
                {
                    ss[k] = ss[k - 1] * nIntMed[k - 1];
                }
                GH_vectorINT v;
                for (int i = 0; i < nE; i++)
                {
                    int s = 0;
                    for (int k = 0; k < m; k++)
                    {
                        s += psi3[i, k] * ss[k];
                    }
                    v = new GH_vectorINT();
                    v.Value = new vectorINT(nN);
                    for (int j = 0; j < nN; j++)
                    {
                        int tt = s;
                        for (int k = 0; k < m; k++)
                        {
                            tt += psi4[j, k] * ss[k];
                        }
                        v.Value[j] = tt;
                    }
                    el.Add(v);
                }
            }
            DA.SetDataList(0, newPointList);
            DA.SetDataList(1, el);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("e2d20eb5-ac6e-4202-93a8-6bc9fb80a8b4"); }
        }

    }*/
}

    