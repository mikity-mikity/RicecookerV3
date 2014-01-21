using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mikity.LinearAlgebra;
using System.Threading;
//using System.Reactive.Linq;
//using System.Reactive;
using mikity.NumericalMethodHelper.materials;
namespace mikity.NumericalMethodHelper.objects
{
    public class constrainVolumeObject : iObject
    {
        public double refVolume = -1;
        public double currentVolume = 0;
        public int number;
        //Weight coefficients
        private double m_lambda = 1.0;
        public int nElem
        {
            get
            {
                return this.elemList.Count;
            }
        }
        public double Lambda
        {
            get
            {
                return m_lambda;
            }
        }

        public List<elements.element> elemList;
        private material em;
        private gravity gvt;
        private void initialize()
        {
            elemList = new List<elements.element>();
            em = new material((_e, _i) => { _i.energyDensity = 0; return _i.stress2.zeros().plus_xA(1.0, _i.invMetric); });
            gvt = new materials.zeroGravity().getGravity();
        }
        private void initialize(double objVol)
        {
            this.refVolume = objVol;
            elemList = new List<elements.element>();
            em = new material((_e, _i) => { _i.energyDensity = 0; return _i.stress2.zeros().plus_xA(1.0, _i.invMetric); });
            gvt = new materials.zeroGravity().getGravity();
        }
        public constrainVolumeObject()
        {
            initialize();
        }
        public constrainVolumeObject(elements.element e)
        {
            initialize();
            this.elemList.Add(e);
        }
        public constrainVolumeObject(elements.element[] el)
        {
            initialize();
            this.elemList.AddRange(el);
        }
        public constrainVolumeObject(double objectVolume)
        {
            initialize(objectVolume);
        }
        public constrainVolumeObject(elements.element e, double objectVolume)
        {
            initialize(objectVolume);
            this.elemList.Add(e);
        }
        public constrainVolumeObject(elements.element[] el, double objectVolume)
        {
            initialize(objectVolume);
            this.elemList.AddRange(el);
        }
        public void addIsoparametricElement(params int[] el)
        {
            elements.isoparametricElement e = new elements.isoparametricElement(el);
            elemList.Add(e);
        }
        public void addSimplexElement(params int[] el)
        {
            elements.simplexElement e = new elements.simplexElement(el);
            elemList.Add(e);
        }
        public void addElement(elements.element e)
        {
            elemList.Add(e);
        }
        public void addElements(elements.element[] el)
        {
            this.elemList.AddRange(el);
        }

        unsafe public void begin(particleSystem pS)
        {
            foreach (elements.element e in this.elemList)
            {
                e.copyFrom(pS.particles);
                e.setMaterial(em, gvt);
                e.Update();
                e.memoryMetric();
            }
            if (refVolume < 0)
            {
                refVolume = 0;
                foreach (elements.element e in this.elemList)
                {
                    refVolume += e.Volume;
                }
            }
            this.number=FriedChiken.requestConditionNumber();
            makeBoundary();
        }
        public List<vectorINT> boundary;
        protected void makeBoundary()
        {
            boundary = new List<vectorINT>();
            for (int i = 0; i < this.nElem; i++)
            {
                for (int j = 0; j < this.elemList[i].boundary.nRow; j++)
                {
                    vectorINT newFace = this.elemList[i].boundary.getRow(j);
                    bool add = true;
                    for (int k = 0; k < boundary.Count; k++)
                    {
                        if (vectorINT.compare(newFace, boundary[k]))
                        {
                            boundary.RemoveAt(k);
                            add = false;
                            break;
                        }
                    }
                    if (add == true)
                    {
                        boundary.Add(newFace);
                    }
                }
            }
        }
        public void update(particleSystem pS)
        {
            vector r = FriedChiken.getResidual();
            matrix J = FriedChiken.getJacobian();
            currentVolume = 0;
            for (int i = 0; i < J.nCol; i++)
            {
                J[number, i] = 0;
            }
            for (int i = 0; i < (int)elemList.Count; i++)
            {
                elements.element e = elemList[i];
                e.copyFrom(pS.particles);
                e.Update();
                e.Merge(pS, J, this.number);
                currentVolume += e.Volume;
            }
            r[number] = currentVolume - refVolume;
        }

        public int __number
        {
            get;
            set;
        }
        public int __number2
        {
            get;
            set;
        }
        public List<int> __number3
        {
            get;
            set;
        }
    }


}
