using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using mikity.LinearAlgebra;
namespace mikity.NumericalMethodHelper
{
    public class particleSystem
    {
        public matrix particles;
        public matrix particlesToThePlane;
        public vector dx;
        public vectorINT globalIndex;     //マージする際の行き先リスト
        public matrix Jacobian;
        public vector Residual;
        public int numCond = 0;
        public List<objects.iObject> objList;
        public bool initialized = false;
        public int requestConditionNumber()
        {
            numCond++;
            return numCond - 1;

        }
        public int __dim
        {
            get;
            private set;
        }
        /// <summary>
        /// 粒子の数
        /// </summary>
        public int __N
        {
            get;
            private set;
        }
        public int DOF
        {
            get
            {
                return __N * __dim;
            }
        }
        public particleSystem(int N)
        {
            initialize(N);
        }

        public particleSystem(particle[] ps)
        {
            initialize(ps.Length);
            for (int i = 0; i < ps.Length; i++)
            {
                for (int k = 0; k < __dim; k++)
                {
                    particles[i, k] = ps[i][k];
                }
            }
        }
        public vector this[int i]
        {
            set
            {
                if (value.nElem != this.__dim) return;
                for (int j = 0; j < this.__dim; j++)
                {
                    this.particles[i, j] = value[j];
                }
            }
            get
            {
                vector v = new vector(__dim);
                for (int j = 0; j < this.__dim; j++)
                {
                    v[j] = this.particles[i, j];
                }
                return v;

            }
        }
        private void initialize(int N)
        {
            __dim = FriedChiken.dimension;
            __N = N;
            particles = new matrix(N, __dim).zeros();
            particlesToThePlane = new matrix(N, __dim).zeros();
            objList = new List<objects.iObject>();
            globalIndex = new vectorINT(N).zeros();
        }

        public void addObject(objects.iObject o)
        {
            this.objList.Add(o);
        }
        public void addObjects(params objects.iObject[] os)
        {
            this.objList.AddRange(os);
        }

        internal void begin()
        {
            if (FriedChiken.isInitilizing)
            {
                foreach (objects.iObject o in this.objList)
                {
                    o.begin(this);
                }
            }
        }
        public void Update()
        {
            if (FriedChiken.isInitialized)
            {
                foreach (objects.iObject o in this.objList)
                {
                    o.update(this);
                }
            }
        }
        unsafe internal void copy(matrix m)
        {
            fixed (double* _ptr1 = &this.particles.rawData[0, 0],_ptr3=&m.rawData[0,0])
            {
                fixed (int* _ptr2 = &this.globalIndex.rawData[0])
                {
                    double* ptr1 = _ptr1;
                    int* ptr2 = _ptr2;
                    for (int i = 0; i < this.__N; i++)
                    {
                        double* ptr3 = _ptr3 + *ptr2 * __dim;
                        for (int k = 0; k < __dim; k++)
                        {
                            *ptr1 = *ptr3;
                            ptr1++;
                            ptr3++;
                        }
                        ptr2++;
                    }
                }
            }
        }
        private vector p1=null;
        private vector p2=null;
        public void simplyfy(int[][] el)
        {
            int nElements = el.Length;
            for (int i = 0; i < nElements; i++)
            {
                findFirst(el[i]);
            }
        }
        public void simplyfy(List<int[]> el)
        {
            int nElements = el.Count;
            for (int i = 0; i < nElements; i++)
            {
                findFirst(el[i]);
            }
        }
        public void findFirst(int[] el)
        {
            int nNodes = el.Length;
            if (p1 == null)
                p1 = new vector(__dim);
            if (p2 == null)
                p2 = new vector(__dim);
            
            int t;
            for (int j = 0; j < nNodes; j++)
            {
                t = el[j];
                for (int k = 0; k < __dim; k++)
                {
                    p1[k] = this.particles[el[j], k];
                }
                for (int s = 0; s <= el[j]; s++)
                {
                    for (int k = 0; k < __dim; k++)
                    {
                        p2[k] = this.particles[s, k];
                    }
                    if (vector.isSame(p1, p2))
                    {
                        t = s;
                        break;
                    }
                }
                el[j] = t;
            }
        }
    }
}