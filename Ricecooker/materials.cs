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
    public class GH_material : GH_Goo<iMaterial>
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
            return "A material definition";
        }

        public override string TypeDescription
        {
            get
            {
                return "A material that can be combined with general spring";
            }
        }

        public override string TypeName
        {
            get { return "GH_material"; }
        }
    }

    
    /// <summary>
    /// 
    /// </summary>
    public class GH_generalSpring : GH_Goo<generalSpring>
    {
        public GH_generalSpring()
        {
            this.Value = new generalSpring();
        }
        public GH_generalSpring(params mikity.NumericalMethodHelper.elements.element[] el)
        {
            this.Value = new generalSpring(el);
        }

        public void addElement(mikity.NumericalMethodHelper.elements.element el)
        {
            this.Value.addElement(el);
        }
        public void addElements(params mikity.NumericalMethodHelper.elements.element[] el)
        {
            this.Value.addElements(el);
        }
        public void setMaterial(GH_material mat,GH_gravity gvt)
        {
            this.Value.setMaterial(mat.Value, gvt.Value);
        }
        public int nElem
        {
            get
            {
                return Value.nElem;
            }
        }
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
            return String.Format("Number of elements={0}", this.Value.nElem);
        }

        public override string TypeDescription
        {
            get { return "general spring that can take various types of materials"; }
        }

        public override string TypeName
        {
            get { return "generalSpring"; }
        }

    
    }
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class stVenantComponent : Grasshopper.Kernel.GH_Component
    {
                protected override System.Drawing.Bitmap Icon
                {
                    get
                    {
                        //現在のコードを実行しているAssemblyを取得
                        System.Reflection.Assembly myAssembly =
                            System.Reflection.Assembly.GetExecutingAssembly();

                        System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.stV.bmp");
                        //指定されたマニフェストリソースを読み込む
                        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                        return bmp;
                    }
                }
                
        public stVenantComponent()
                    : base("stVenant [Material]", "stVenant", "st. Venant material", "Ricecooker", "Materials")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Young", "Y", "Young's modulus", Grasshopper.Kernel.GH_ParamAccess.item, 2000);
            pManager.AddNumberParameter("Poisson", "γ", "Poisson's ratio", Grasshopper.Kernel.GH_ParamAccess.item, 0.2);
//            pManager.AddNumberParameter("Density", "p", "Density", Grasshopper.Kernel.GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "Material", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        stVenantMaterial stVen = null;
        GH_material m_mat = null;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (mikity.NumericalMethodHelper.FriedChiken.isInitialized)
            {
                double Y = 0, g = 0;
                if (!DA.GetData(0, ref Y)) return;
                if (!DA.GetData(1, ref g)) return;
                stVen.Young = Y;
                stVen.Poisson = g;
            }
            else
            {
                double Y=0, g=0;
                if(!DA.GetData(0, ref Y))return;
                if(!DA.GetData(1, ref g))return;
                stVen=new stVenantMaterial();
                m_mat = new GH_material();
                m_mat.Value = stVen;
                stVen.Young = Y;
                stVen.Poisson = g;                
            }
            DA.SetData(0, m_mat);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("475bd838-33d4-470a-9ef9-422df2232fdb"); }
        }

    }
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class neoHookeanComponent : Grasshopper.Kernel.GH_Component
    {
                protected override System.Drawing.Bitmap Icon
                {
                    get
                    {
                        //現在のコードを実行しているAssemblyを取得
                        System.Reflection.Assembly myAssembly =
                            System.Reflection.Assembly.GetExecutingAssembly();

                        System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.neoH.bmp");
                        //指定されたマニフェストリソースを読み込む
                        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                        return bmp;
                    }
                }
                
        public neoHookeanComponent()
                    : base("neoHookean [Material]", "neoHookean", "neoHookean material", "Ricecooker", "Materials")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Coefficient1", "u1", "First coefficient", Grasshopper.Kernel.GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Pressure", "K", "Hydro pressure to make the material imcompressive. This should be sufficiently greater than u1 and u2.", Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "Material", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        neoHookeanMaterial neoHook = null;
        GH_material m_mat = null;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (mikity.NumericalMethodHelper.FriedChiken.isInitialized)
            {
                double  u1 = 0,K=0;
                if (!DA.GetData(0, ref u1)) return;
                if (!DA.GetData(1, ref K)) return;
                
                neoHook.mu1 = u1;
                neoHook.K = K;
            }
            else
            {
                double u1 = 0,K=0;
                if (!DA.GetData(0, ref u1)) return;
                if (!DA.GetData(1, ref K)) return;
                neoHook = new neoHookeanMaterial();
                m_mat = new GH_material();
                m_mat.Value = neoHook;
                neoHook.mu1 = u1;
                neoHook.K = K;
            }
            DA.SetData(0, m_mat);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("7dc1e19a-3d78-4ee7-ab1e-21c62fac0bbe"); }
        }

    }
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class mooneyRivlinComponent : Grasshopper.Kernel.GH_Component
    {
                protected override System.Drawing.Bitmap Icon
                {
                    get
                    {
                        //現在のコードを実行しているAssemblyを取得
                        System.Reflection.Assembly myAssembly =
                            System.Reflection.Assembly.GetExecutingAssembly();

                        System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.mR.bmp");
                        //指定されたマニフェストリソースを読み込む
                        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                        return bmp;
                    }
                }
                
        public mooneyRivlinComponent()
                    : base("mooneyRivlin [Material]", "MooneeRivlin", "MooneyRivlin material", "Ricecooker", "Materials")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Coefficient1", "u1", "First coefficient", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("Coefficient2", "u2", "First coefficient", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("Pressure", "K", "Hydro pressure to make the material imcompressive. This should be sufficiently greater than u1 and u2.", Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "Material", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        mooneyRivlinMaterial mooRiv = null;
        GH_material m_mat = null;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (mikity.NumericalMethodHelper.FriedChiken.isInitialized)
            {
                double mu1=0,mu2=0,K = 0;
                if (!DA.GetData(0, ref mu1)) return;
                if (!DA.GetData(1, ref mu2)) return;
                if (!DA.GetData(2, ref K)) return;

                mooRiv.u1 = mu1;
                mooRiv.u2 = mu2;
                mooRiv.K = K;
            }
            else
            {
                double mu1 = 0, mu2 = 0, K = 0;
                if (!DA.GetData(0, ref mu1)) return;
                if (!DA.GetData(1, ref mu2)) return;
                if (!DA.GetData(2, ref K)) return;
                mooRiv = new mooneyRivlinMaterial();
                m_mat = new GH_material();
                m_mat.Value = mooRiv;
                mooRiv.u1 = mu1;
                mooRiv.u2 = mu2;
                mooRiv.K = K;
            }
            DA.SetData(0, m_mat);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("c13048fd-81c3-410f-b5ac-0848614cb971"); }
        }

    }
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class formFindingComponent : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.fF.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }

        public formFindingComponent()
            : base("formFinding [Material]", "formFinding", "special material for formfinding", "Ricecooker", "Materials")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Power", "p", "Power number", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager.AddNumberParameter("Weight", "w", "Weighting factor", Grasshopper.Kernel.GH_ParamAccess.item, 1.0);
            //pManager.AddNumberParameter("Density", "d", "Mass Density", Grasshopper.Kernel.GH_ParamAccess.item, 0.0);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "Material", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        formFindingMaterial formFind = null;
        GH_material m_mat = null;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            if (mikity.NumericalMethodHelper.FriedChiken.isInitialized)
            {
                double p = 0, w = 0;
                if (!DA.GetData(0, ref p)) return;
                if (!DA.GetData(1, ref w)) return;
                formFind.Power = p;
                formFind.Weight = w;
            }
            else
            {
                double p = 0, w = 0;
                if (!DA.GetData(0, ref p)) return;
                if (!DA.GetData(1, ref w)) return;
                formFind = new formFindingMaterial();
                m_mat = new GH_material();
                m_mat.Value = formFind;
                formFind.Power = p;
                formFind.Weight = w;
            }
            DA.SetData(0, m_mat);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("d9d3d0ae-a8ba-485e-9b90-1d1e5c4a036b"); }
        }

    }
}
