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
    public delegate void DrawViewPortWire(Grasshopper.Kernel.IGH_PreviewArgs args);
    public delegate void UpdateGeometry(double x,double y,double z);
    public delegate void BakeGeometry(Rhino.RhinoDoc doc,Rhino.DocObjects.ObjectAttributes att, List<Guid> obj_ids);
    /// <summary>
    /// 
    /// </summary>
    public class GH_particleSystem : GH_Goo<particleSystem>
    {
        public DrawViewPortWire DVPW = null;
        public UpdateGeometry UPGR = null;
        public BakeGeometry BKGT = null;
        public GH_particleSystem()
        {
            this.Value = null;
        }
        public GH_particleSystem(int N)
        {
            this.Value = new particleSystem(N);
        }
        public GH_particleSystem(particle[] ps)
        {
            this.Value = new particleSystem(ps);
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
            return String.Format("Number of particles={0},Number of objects={1};", this.Value.__N, this.Value.objList.Count);
        }

        public override string TypeDescription
        {
            get { return "particle System, containing particles and elements"; }
        }

        public override string TypeName
        {
            get { return "particleSystem"; }
        }

        internal void simplify(int[][] el)
        {
            this.Value.simplyfy(el);

        }
        internal void simplify(List<int[]> el)
        {
            this.Value.simplyfy(el);

        }
        internal void findFirst(int[] el)
        {
            this.Value.findFirst(el);

        }
    }
   
}
