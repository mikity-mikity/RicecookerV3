using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mikity;
using mikity.LinearAlgebra;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using mikity.NumericalMethodHelper.objects;
using mikity.NumericalMethodHelper.materials;
namespace mikity.ghComponents
{
    public class GH_gravity : GH_Goo<iGravity>
    {
        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override bool IsValid
        {
            get
            {
                if (this.Value != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public override string ToString()
        {
            return "A gravity definition";
        }

        public override string TypeDescription
        {
            get
            {
                return "A gravity definition that can be combined with general spring";
            }
        }

        public override string TypeName
        {
            get { return "GH_gravity"; }
        }
    }
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class zeroGravity : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.zeroGravity.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }

        public zeroGravity()
                    : base("zeroGravity [Gravity]", "zeroGravity", "produces no gravity", "Ricecooker", "Gravities")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Gravity", "Gvt", "Gravity", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        mikity.NumericalMethodHelper.materials.zeroGravity zG = null;
        GH_gravity m_gvt = null;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (mikity.NumericalMethodHelper.FriedChiken.isInitialized)
            {
            }
            else
            {
                zG = new mikity.NumericalMethodHelper.materials.zeroGravity();
                m_gvt = new GH_gravity();
                m_gvt.Value = zG;
            }
            DA.SetData(0, m_gvt);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("76df3188-92a7-48e2-8480-dfd7a60815ff"); }
        }

    }
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class normalGravity : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.normalGravity.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }

        public normalGravity()
            : base("normalGravity [Gravity]", "normalGravity", "Produces normal gravity", "Ricecooker", "Gravities")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Density", "p", "Weight per unit volume, area or length. G=9.8 will automatically be multiplied. For two or one dimensional elements, thickness or sectional area must be included in Density.", Grasshopper.Kernel.GH_ParamAccess.item,1.0);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Gravity", "Gvt", "Gravity", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        mikity.NumericalMethodHelper.materials.normalGravity nG = null;
        GH_gravity m_gvt = null;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (mikity.NumericalMethodHelper.FriedChiken.isInitialized)
            {
                double p = 0;
                if (!DA.GetData(0, ref p)) return;
                nG.Density = p;
            }
            else
            {
                double p = 0;
                if (!DA.GetData(0, ref p)) return;
                nG = new mikity.NumericalMethodHelper.materials.normalGravity();
                m_gvt = new GH_gravity();
                m_gvt.Value = nG;
                nG.Density = p;
            }
            DA.SetData(0, m_gvt);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("71cc5777-6515-46f2-ab61-0e0495eef4a1"); }
        }

    }
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class formfindGravity : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.formfindGravity.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }

        public formfindGravity()
            : base("formfindGravity [Gravity]", "formfindGravity", "Provides special treatment of gravity for form-finding analyses", "Ricecooker", "Gravities")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Density", "p", "Weight per unit volume, area or length. G=9.8 will automatically be multiplied. For two or one dimensional elements, thickness or sectional area must be included in Density.", Grasshopper.Kernel.GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Gravity", "Gvt", "Gravity", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        mikity.NumericalMethodHelper.materials.formfindGravity nG = null;
        GH_gravity m_gvt = null;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (mikity.NumericalMethodHelper.FriedChiken.isInitialized)
            {
                double p = 0;
                if (!DA.GetData(0, ref p)) return;
                nG.Density = p;
            }
            else
            {
                double p = 0;
                if (!DA.GetData(0, ref p)) return;
                nG = new mikity.NumericalMethodHelper.materials.formfindGravity();
                m_gvt = new GH_gravity();
                m_gvt.Value = nG;
                nG.Density = p;
            }
            DA.SetData(0, m_gvt);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("c679516a-c1a4-4d4a-85ed-3b5e09ffc587"); }
        }

    }
}
