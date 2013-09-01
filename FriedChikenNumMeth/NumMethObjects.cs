using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mikity.LinearAlgebra;
using System.Threading;
using System.Threading.Tasks;
using mikity.NumericalMethodHelper.materials;
namespace mikity.NumericalMethodHelper.objects
{

    public interface iObject
    {
        int __number
        {
            set;
            get;
        }
        void begin(particleSystem pS);
        void update(particleSystem pS);
    }
    public class generalSpring : iObject
    {

        public int nElem
        {
            get
            {
                return this.elemList.Count;
            }
        }
        public void setMaterial(materials.iMaterial mat,materials.iGravity gvt)
        {
            material m = mat.getMaterial();
            gravity g = gvt.getGravity();
            foreach(elements.element e in this.elemList)
            {
                e.setMaterial(m, g);
            }
        }
        public List<elements.element> elemList;
        private void initialize()
        {
            elemList = new List<elements.element>();
        }
        public generalSpring()
        {
            initialize();
        }
        public generalSpring(elements.element e)
        {
            initialize();
            this.elemList.Add(e);
        }
        public generalSpring(elements.element[] el)
        {
            initialize();
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
            materials.material _em = new material((_e, _i) => { return _i.stress2.zeros().plus_xA(1.0, _i.invMetric); });
            materials.gravity _gvt = new materials.zeroGravity().getGravity();
            materials.gravity gvt2 = null;
            materials.material em2 = null;
            foreach (elements.element e in this.elemList)
            {
                e.copyFrom(pS.particles);
                em2 = e.getMaterial();
                gvt2 = e.getGravity();
                e.setMaterial(_em, _gvt);
                e.Update();
                e.memoryMetric();
                e.setMaterial(em2, gvt2);
                e.Update();
            }
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
        //private IObservable<elements.element> elementObserver;

        public void update(particleSystem pS)
        {
            vector grad = FriedChiken.getGradient();
            vector load = FriedChiken.getLoad();
//            var method1 = Observable.Start(() =>
//            {
            System.Threading.Tasks.Parallel.For(0,elemList.Count,(i)=>
                {
                    elements.element e = elemList[i];
                    e.copyFrom(pS.particles);
                    e.Update();
                }
            );

            for (int i = 0; i < elemList.Count; i++)
            {
                elements.element e = elemList[i];
                e.Merge(pS, grad, load);
            }


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

    public class fixedNodes : iObject
    {
        public List<elements.node> nodeList;
        public int __dim;
        public int nNodes
        {
            get
            {
                return this.nodeList.Count;
            }
        }
        public bool fixX
        {
            set;
            get;
        }
        public bool fixY
        {
            set;
            get;
        }
        public bool fixZ
        {
            set;
            get;
        }
        public void moveX(double val)
        {
            for (int i = 0; i < nNodes; i++)
            {
                nodeList[i].getNodes()[0, 0] = this.initialNodes[i, 0] + val;
            }
        }
        public void moveY(double val)
        {
            for (int i = 0; i < nNodes; i++)
            {
                nodeList[i].getNodes()[0, 1] = this.initialNodes[i, 1] + val;
            }
        }
        public void moveZ(double val)
        {
            for (int i = 0; i < nNodes; i++)
            {
                nodeList[i].getNodes()[0, 2] = this.initialNodes[i, 2] + val;
            }
        }
        public fixedNodes()
        {
            initialize(); 
            this.fixX = true;
            this.fixY = true;
            this.fixZ = true;
        }
        public fixedNodes(bool x,bool y,bool z)
        {
            initialize();
            this.fixX = x;
            this.fixY = y;
            this.fixZ = z;

        }
        private void initialize()
        {
            __dim = FriedChiken.dimension;
            nodeList = new List<elements.node>();
        }
        public void addNode(elements.node n)
        {
            this.nodeList.Add(n);
        }
        public void addNodes(elements.node[] ns)
        {
            this.nodeList.AddRange(ns);
        }
        private matrix initialNodes;
        public void begin(particleSystem pS)
        {
            foreach (elements.node n in nodeList)
            {
                n.copyFrom(pS.particles);
            }
            initialNodes = new matrix(nNodes, __dim);
            for (int i = 0; i < nNodes; i++)
            {
                initialNodes[i, 0] = nodeList[i].getNodes()[0, 0];
                initialNodes[i, 1] = nodeList[i].getNodes()[0, 1];
                initialNodes[i, 2] = nodeList[i].getNodes()[0, 2];
            }
            vector _mask= FriedChiken.getMask();
            foreach (elements.node n in nodeList)
            {
                if (fixX)
                {
                    _mask[FriedChiken.index[pS.globalIndex[n.el[0]], 0]] = 1;
                }
                if (fixY)
                {
                    _mask[FriedChiken.index[pS.globalIndex[n.el[0]], 1]] = 1;
                }
                if (fixZ)
                {
                    _mask[FriedChiken.index[pS.globalIndex[n.el[0]], 2]] = 1;
                }
            }
        }

        public void update(particleSystem pS)
        {
            foreach (elements.node n in nodeList)
            {
                if (fixX)
                {
                    n.copyXTo(pS);
                }else
                {
                    n.copyXFrom(pS.particles);
                }
                if (fixY)
                {
                    n.copyYTo(pS);
                }
                else
                {
                    n.copyYFrom(pS.particles);
                }
                if (fixZ)
                {
                    n.copyZTo(pS);
                }
                else
                {
                    n.copyZFrom(pS.particles);
                }
            }
        }

        public int Number { get; set; }

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
    public class nodalForce : iObject
    {
        public List<elements.node> nodeList;
        public int __dim;
        public int nNodes
        {
            get
            {
                return this.nodeList.Count;
            }
        }
        public double forceX
        {
            set;
            get;
        }
        public double forceY
        {
            set;
            get;
        }
        public double forceZ
        {
            set;
            get;
        }
        public nodalForce()
        {
            initialize();
            this.forceX = 0;
            this.forceY = 0;
            this.forceZ = -1;
        }
        public nodalForce(double x, double y, double z)
        {
            initialize();
            this.forceX = x;
            this.forceY = y;
            this.forceZ = z;

        }
        private void initialize()
        {
            __dim = FriedChiken.dimension;
            nodeList = new List<elements.node>();
        }
        public void addNode(elements.node n)
        {
            this.nodeList.Add(n);
        }
        public void addNodes(elements.node[] ns)
        {
            this.nodeList.AddRange(ns);
        }
        public void begin(particleSystem pS)
        {
            foreach (elements.node n in nodeList)
            {
                n.copyFrom(pS.particles);
            }
        }

        public void update(particleSystem pS)
        {
            vector load = FriedChiken.getLoad();
            foreach (elements.node n in nodeList)
            {
                load[FriedChiken.index[pS.globalIndex[n.el[0]], 0]] += this.forceX;
                load[FriedChiken.index[pS.globalIndex[n.el[0]], 1]] += this.forceY;
                load[FriedChiken.index[pS.globalIndex[n.el[0]], 2]] += this.forceZ;
            }
        }

        public int Number { get; set; }

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
