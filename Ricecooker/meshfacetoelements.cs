using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mikity;
using mikity.LinearAlgebra;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using mikity.NumericalMethodHelper.objects;
namespace mikity.ghComponents
{
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class meshToElements : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.icon34.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
        
        public meshToElements()
            : base("meshToElements", "mesh->Elements", "mesh->Elements", "Ricecooker", "Primitives")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("Points", "P", "Particle System",Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_GenericParam("Elements", "E", "Elements", Grasshopper.Kernel.GH_ParamAccess.list);
        }
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            List<GH_vectorINT> el = new List<GH_vectorINT>();
            List<Rhino.Geometry.Point3d> pl = new List<Rhino.Geometry.Point3d>();
            Rhino.Geometry.Mesh m = null;
            if (!DA.GetData(0,ref m)) return;
            pl.AddRange(m.Vertices.ToPoint3dArray());
            for (int i = 0; i < m.Faces.Count; i++)
            {
                if (m.Faces[i].IsQuad)
                {
                    int[] f = new int[4] { m.Faces[i].A, m.Faces[i].B, m.Faces[i].D, m.Faces[i].C };
                    el.Add(new GH_vectorINT(f));
                }
            }
            DA.SetDataList(0, pl);
            DA.SetDataList(1, el);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("db65be39-8604-4031-ba91-7733a2ddb3cd"); }
        }

    }
}
