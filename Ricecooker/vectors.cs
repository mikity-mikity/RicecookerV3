using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mikity.NumericalMethodHelper;
namespace mikity.ghComponents
{
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class vectorINT2 : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.icon43.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
 
        public vectorINT2()
            : base("vectorINT2", "vectorINT2", "vectorINT2", "Ricecooker", "vectors")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("x", "x", "x", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("y", "y", "y", Grasshopper.Kernel.GH_ParamAccess.item, 2);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("VectorINT2", "V", "VectorINT2", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        GH_vectorINT v = null;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                v = new GH_vectorINT(2);
                int x=0,y=0;
                if(!DA.GetData(0,ref x))return;
                if(!DA.GetData(1,ref y))return;
                v[0] = x;
                v[1] = y;
            }
            else
            {
            }
            DA.SetData(0, v);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("8f82dcce-d563-4c73-a5f4-051cbda9ff9e"); }
        }

    }
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class vectorINT3 : Grasshopper.Kernel.GH_Component
    {
                protected override System.Drawing.Bitmap Icon
                {
                    get
                    {
                        //現在のコードを実行しているAssemblyを取得
                        System.Reflection.Assembly myAssembly =
                            System.Reflection.Assembly.GetExecutingAssembly();

                        System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.icon44.bmp");
                        //指定されたマニフェストリソースを読み込む
                        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                        return bmp;
                    }
                }
         
        public vectorINT3()
            : base("vectorINT3", "vectorINT3", "vectorINT3", "Ricecooker", "vectors")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("x", "x", "x", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("y", "y", "y", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("z", "z", "z", Grasshopper.Kernel.GH_ParamAccess.item, 2);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("VectorINT3", "V", "VectorINT3", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        GH_vectorINT v = null;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (!FriedChiken.isInitialized)
            {
                v = new GH_vectorINT(3);
                int x = 0, y = 0, z = 0;
                if (!DA.GetData(0, ref x)) return;
                if (!DA.GetData(1, ref y)) return;
                if (!DA.GetData(2, ref z)) return;
                v[0] = x;
                v[1] = y;
                v[2] = z;
            }
            else
            {
            }
            DA.SetData(0, v);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("3add8f6b-ecce-4f35-a172-0d4990cb96a0"); }
        }

    }

}
