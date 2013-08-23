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
    /// 
    /// </summary>
    public class GH_constrainVolumeObject : GH_Goo<constrainVolumeObject>
    {
        public GH_constrainVolumeObject()
        {
            this.Value = new constrainVolumeObject();
        }
        public GH_constrainVolumeObject(double refVolume)
        {
            this.Value = new constrainVolumeObject(refVolume);
        }
        public GH_constrainVolumeObject(params mikity.NumericalMethodHelper.elements.element[] el)
        {
            this.Value = new constrainVolumeObject(el);
        }
        public GH_constrainVolumeObject(double refVolume,params mikity.NumericalMethodHelper.elements.element[] el)
        {
            this.Value = new constrainVolumeObject(el,refVolume);
        }

        public void addElement(mikity.NumericalMethodHelper.elements.element el)
        {
            this.Value.addElement(el);
        }
        public void addElements(params mikity.NumericalMethodHelper.elements.element[] el)
        {
            this.Value.addElements(el);
        }
        public int nElem
        {
            get
            {
                return Value.nElem;
            }
        }
        public double refVolume
        {
            set
            {
                this.Value.refVolume = value;
            }
            get
            {
                return this.Value.refVolume;
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
            return String.Format("Number of elements={0},targetVolume={1},currentVolume={2}", this.Value.nElem, this.Value.refVolume,this.Value.currentVolume);
        }

        public override string TypeDescription
        {
            get { return "Sum of element lengths, areas, or volumes are constrained to the specified value"; }
        }

        public override string TypeName
        {
            get { return "constrainVolume"; }
        }

    }
 
}
