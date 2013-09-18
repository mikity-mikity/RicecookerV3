using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using mikity.LinearAlgebra;
using mikity.NumericalMethodHelper.materials;

namespace mikity.NumericalMethodHelper.elements
{

    public abstract class __nodeContainer
    {
        //節点座標
        protected matrix nodes;
        //要素の節点のインデクス
        public vectorINT el;
        //節点の数
        public int nNodes;
        public int __dim;

        public abstract void __update();
        public __nodeContainer(int num)
        {
            this.__dim = FriedChiken.dimension;
            this.nNodes = num;
            this.nodes = new matrix(nNodes, __dim).zeros();
            el = new vectorINT(num).zeros();
        }
        public void Update()
        {
            __update();
        }
        public matrix getNodes()
        {
            return this.nodes;
        }



        public void copyTo(particleSystem pS)
        {
        }
        public void copyXTo(particleSystem pS)
        {
            for (int i = 0; i < nNodes; i++)
            {
                pS[el[i]][0] = this.nodes[i, 0];
            }

			for (int i = 0; i < nNodes; i++)
			{
				FriedChiken.x[FriedChiken.index[pS.globalIndex[el[i]], 0]] = this.nodes[i, 0];
			}

        }
        public void copyYTo(particleSystem pS)
        {
            for (int i = 0; i < nNodes; i++)
            {
                 pS[el[i]][1] = this.nodes[i, 1];
            }
            if (FriedChiken.isInitialized)
            {
                for (int i = 0; i < nNodes; i++)
                {
                     FriedChiken.x[FriedChiken.index[pS.globalIndex[el[i]], 1]] = this.nodes[i, 1];
                }
            }
        }
        public void copyZTo(particleSystem pS)
        {
            for (int i = 0; i < nNodes; i++)
            {
                pS[el[i]][2] = this.nodes[i, 2];
            }
            if (FriedChiken.isInitialized)
            {
                for (int i = 0; i < nNodes; i++)
                {
                    FriedChiken.x[FriedChiken.index[pS.globalIndex[el[i]], 2]] = this.nodes[i, 2];
                }
            }
        }
        unsafe public void copyXTo(matrix ps)
        {
            //for (int i = 0; i < nNodes; i++)
            //{
            //    nodes[i] = ps[el[i]];
            //                vector v = ps[el[i]];
            //                fixed (double* _ptr1 = &v.rowData[0],_ptr2=&this.nodes[i].rowData[0])
            //                {
            //                    *_ptr2 = *(_ptr1);
            //                    *(_ptr2+1) = *(_ptr1 + 1);
            //                    *(_ptr2+2) = *(_ptr1 + 2);
            //                }
            //}
            //vector v = null;
            fixed (double* _ptr1 = &nodes.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                for (int i = 0; i < nNodes; i++)
                {
                    fixed (double* _ptr5 = &ps.rawData[el[i], 0])
                    {
                        //   v = ps[el[i]];
                        double* ptr5 = _ptr5;
                        *(ptr5 + 0) = *(ptr1 + 0);// v[j];
                    }
                    ptr1 += __dim;
                }
            }
        }
        unsafe public void copyYTo(matrix ps)
        {
            //for (int i = 0; i < nNodes; i++)
            //{
            //    nodes[i] = ps[el[i]];
            //                vector v = ps[el[i]];
            //                fixed (double* _ptr1 = &v.rowData[0],_ptr2=&this.nodes[i].rowData[0])
            //                {
            //                    *_ptr2 = *(_ptr1);
            //                    *(_ptr2+1) = *(_ptr1 + 1);
            //                    *(_ptr2+2) = *(_ptr1 + 2);
            //                }
            //}
            //vector v = null;
            fixed (double* _ptr1 = &nodes.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                for (int i = 0; i < nNodes; i++)
                {
                    fixed (double* _ptr5 = &ps.rawData[el[i], 0])
                    {
                        //   v = ps[el[i]];
                        double* ptr5 = _ptr5;
                        *(ptr5 + 1) = *(ptr1 + 1);// v[j];
                    }
                    ptr1 += __dim;
                }
            }
        }
        unsafe public void copyZTo(matrix ps)
        {
            //for (int i = 0; i < nNodes; i++)
            //{
            //    nodes[i] = ps[el[i]];
            //                vector v = ps[el[i]];
            //                fixed (double* _ptr1 = &v.rowData[0],_ptr2=&this.nodes[i].rowData[0])
            //                {
            //                    *_ptr2 = *(_ptr1);
            //                    *(_ptr2+1) = *(_ptr1 + 1);
            //                    *(_ptr2+2) = *(_ptr1 + 2);
            //                }
            //}
            //vector v = null;
            fixed (double* _ptr1 = &nodes.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                for (int i = 0; i < nNodes; i++)
                {
                    fixed (double* _ptr5 = &ps.rawData[el[i], 0])
                    {
                        //   v = ps[el[i]];
                        double* ptr5 = _ptr5;
                        *(ptr5+2) = *(ptr1+2);// v[j];
                    }
                    ptr1 += __dim;
                }
            }
        }

        unsafe public void copyXFrom(matrix ps)
        {
            //for (int i = 0; i < nNodes; i++)
            //{
            //    nodes[i] = ps[el[i]];
            //                vector v = ps[el[i]];
            //                fixed (double* _ptr1 = &v.rowData[0],_ptr2=&this.nodes[i].rowData[0])
            //                {
            //                    *_ptr2 = *(_ptr1);
            //                    *(_ptr2+1) = *(_ptr1 + 1);
            //                    *(_ptr2+2) = *(_ptr1 + 2);
            //                }
            //}
            //vector v = null;
            fixed (double* _ptr1 = &nodes.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                for (int i = 0; i < nNodes; i++)
                {
                    fixed (double* _ptr5 = &ps.rawData[el[i], 0])
                    {
                        //   v = ps[el[i]];
                        double* ptr5 = _ptr5;
                        *(ptr1 + 0) = *(ptr5 + 0);// v[j];
                    }
                    ptr1 += __dim;
                }
            }
        }
        unsafe public void copyYFrom(matrix ps)
        {
            //for (int i = 0; i < nNodes; i++)
            //{
            //    nodes[i] = ps[el[i]];
            //                vector v = ps[el[i]];
            //                fixed (double* _ptr1 = &v.rowData[0],_ptr2=&this.nodes[i].rowData[0])
            //                {
            //                    *_ptr2 = *(_ptr1);
            //                    *(_ptr2+1) = *(_ptr1 + 1);
            //                    *(_ptr2+2) = *(_ptr1 + 2);
            //                }
            //}
            //vector v = null;
            fixed (double* _ptr1 = &nodes.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                for (int i = 0; i < nNodes; i++)
                {
                    fixed (double* _ptr5 = &ps.rawData[el[i], 0])
                    {
                        //   v = ps[el[i]];
                        double* ptr5 = _ptr5;
                        *(ptr1 + 1) = *(ptr5 + 1);// v[j];
                    }
                    ptr1+=__dim;
                }
            }
        }
        unsafe public void copyZFrom(matrix ps)
        {
            //for (int i = 0; i < nNodes; i++)
            //{
            //    nodes[i] = ps[el[i]];
            //                vector v = ps[el[i]];
            //                fixed (double* _ptr1 = &v.rowData[0],_ptr2=&this.nodes[i].rowData[0])
            //                {
            //                    *_ptr2 = *(_ptr1);
            //                    *(_ptr2+1) = *(_ptr1 + 1);
            //                    *(_ptr2+2) = *(_ptr1 + 2);
            //                }
            //}
            //vector v = null;
            fixed (double* _ptr1 = &nodes.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                for (int i = 0; i < nNodes; i++)
                {
                    fixed (double* _ptr5 = &ps.rawData[el[i], 0])
                    {
                        //   v = ps[el[i]];
                        double* ptr5 = _ptr5;
                        *(ptr1 + 2) = *(ptr5 + 2);// v[j];
                    }
                    ptr1+=__dim;
                }
            }
        }

        unsafe public void copyTo(matrix ps)
        {
            //for (int i = 0; i < nNodes; i++)
            //{
            //    nodes[i] = ps[el[i]];
            //                vector v = ps[el[i]];
            //                fixed (double* _ptr1 = &v.rowData[0],_ptr2=&this.nodes[i].rowData[0])
            //                {
            //                    *_ptr2 = *(_ptr1);
            //                    *(_ptr2+1) = *(_ptr1 + 1);
            //                    *(_ptr2+2) = *(_ptr1 + 2);
            //                }
            //}
            //vector v = null;
            fixed (double* _ptr1 = &nodes.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                for (int i = 0; i < nNodes; i++)
                {
                    fixed (double* _ptr5 = &ps.rawData[el[i], 0])
                    {
                        //   v = ps[el[i]];
                        double* ptr5 = _ptr5;
                        for (int j = 0; j < __dim; j++)
                        {
                            *ptr5 = *ptr1;// v[j];
                            ptr1++;
                            ptr5++;
                        }
                    }
                }
            }
        }
        public void copyFrom(particleSystem pS)
        {
            copyFrom(pS.particles);
        }

        unsafe public void copyFrom(matrix ps)
        {
            //for (int i = 0; i < nNodes; i++)
            //{
            //    nodes[i] = ps[el[i]];
            //                vector v = ps[el[i]];
            //                fixed (double* _ptr1 = &v.rowData[0],_ptr2=&this.nodes[i].rowData[0])
            //                {
            //                    *_ptr2 = *(_ptr1);
            //                    *(_ptr2+1) = *(_ptr1 + 1);
            //                    *(_ptr2+2) = *(_ptr1 + 2);
            //                }
            //}
            //vector v = null;
            fixed (double* _ptr1 = &nodes.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                for (int i = 0; i < nNodes; i++)
                {
                    fixed (double* _ptr5 = &ps.rawData[el[i], 0])
                    {
                        //   v = ps[el[i]];
                        double* ptr5 = _ptr5;
                        for (int j = 0; j < __dim; j++)
                        {
                            *ptr1 = *ptr5;// v[j];
                            ptr1++;
                            ptr5++;
                        }
                    }
                }
            }
        }
        unsafe public void copyFrom(vectors ps)
        {
            //for (int i = 0; i < nNodes; i++)
            //{
            //    nodes[i] = ps[el[i]];
            //                vector v = ps[el[i]];
            //                fixed (double* _ptr1 = &v.rowData[0],_ptr2=&this.nodes[i].rowData[0])
            //                {
            //                    *_ptr2 = *(_ptr1);
            //                    *(_ptr2+1) = *(_ptr1 + 1);
            //                    *(_ptr2+2) = *(_ptr1 + 2);
            //                }
            //}
            //vector v = null;
            fixed (double* _ptr1 = &nodes.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                for (int i = 0; i < nNodes; i++)
                {
                    fixed (double* _ptr5 = &ps[el[i]].rawData[0])
                    {
                        //   v = ps[el[i]];
                        double* ptr5 = _ptr5;
                        for (int j = 0; j < __dim; j++)
                        {
                            *ptr1 = *ptr5;// v[j];
                            ptr1++;
                            ptr5++;
                        }
                    }
                }
            }
        }

    }
    public class node : __nodeContainer
    {
        //        public vector _node;
        public node(int __n):base(1)
        {
            el.copyFrom(__n);
        }
        public override void __update()
        {
        }

    }
    public class element:__nodeContainer
    {
        protected virtual void makeBoundary()
        {
            boundary = new matrixINT(1,1).zeros();
        }
        //境界
        public matrixINT boundary;
        
        //構成則
        protected material constitutive = new material((_e, _i) => { return _i.stress2.zeros().plus_xA(1.0, _i.invMetric); });
        protected gravity constitutiveG = new materials.zeroGravity().getGravity();
        public void setMaterial(material _s,gravity _g)
        {
            constitutive = _s;
            constitutiveG = _g;
        }
        public materials.material getMaterial()
        {
            return constitutive;
        }
        public materials.gravity getGravity()
        {
            return constitutiveG;
        }
        /// <summary>
        /// 積分点
        /// </summary>
        public integralPoint[] intPoints;

        //Number of coordinate parameters.
        protected double l, u, p;
        //要素の次元
        public int __N;
        //積分点の数
        public int nIntPoints;
        //描画点の数
        public int nVisPoints;
        protected matrix nodalForce;
        protected matrix nodalForce2;
        protected int dof;
        protected double _volume,_refVolume;
        public double Volume
        {
            get
            {
                return _volume;
            }
        }
        public double refVolume
        {
            get
            {
                return _refVolume;
            }
        }
        protected vector elemGrad;
        protected vector elemLoad;
        public element(int[] _el)
            : base(_el.Length)
        {
            el.copyFrom(_el);
            this.dof = this.nNodes * __dim;
            initialize(nNodes);
        }
        public void initialize(int nNode)
        {
            nodalForce = new matrix(nNode, __dim);
            nodalForce2 = new matrix(nNode, __dim);
            elemGrad = new vector(dof).zeros();
            elemLoad = new vector(dof).zeros();
        }
        public vector getElemGrad()
        {
            return this.elemGrad;
        }
        public vector getElemLoad()
        {
            return this.elemLoad;
        }
        public matrix decomposeElemGrad()
        {
//            int index = 0;
            elemGrad.Decompose(nodalForce);
/*            for (int i = 0; i < nNodes; i++)
            {
                for (int j = 0; j < __dim; j++)
                {
                    nodalForce[i, j] = elemGrad[index];
                    index++;
                }
            }
*/            return this.nodalForce;
        }

        public matrix decomposeElemLoad()
        {
            int index = 0;
//            elemLoad.Decompose(nodalForce2);
            for (int i = 0; i < nNodes; i++)
            {
                for (int j = 0; j < __dim; j++)
                {
                    nodalForce2[i, j] = elemLoad[index];
                    index++;
                }
            }

            return this.nodalForce2;
        }
        unsafe public override void __update()
        {
            //積分点のアップデート
            elemGrad.zeros();
            elemLoad.zeros();
            integralPoint _i = null;
            //体積の計算
            double _v = 0;
            fixed (double* _ptr1 = &elemGrad.rawData[0], _ptr2 = &this.elemLoad.rawData[0])
            {
                for (int i = 0; i < nIntPoints; i++)
                {
                    _i = intPoints[i];
                    _i.Update();
                    _v += _i.w * Math.Sqrt(_i.metric.det);
                }
                this._volume = _v;
                for (int i = 0; i < nIntPoints; i++)
                {
                    _i = intPoints[i];
                    _i.stress2 = constitutive(this, _i);
                    _i.gravity = constitutiveG(this, _i);
                    _i.stress.Products(_i.stress2, _i.metric);
                    double* ptr1 = _ptr1;
                    double J = Math.Sqrt(_i.metric.det);
                    for (int k = 0; k < this.dof; k++)
                    {
                        *ptr1 +=
                        0.5 * _i.w * J * matrix.DoubleDot(_i.stress2, _i.difMetric[k]);
                        ptr1++;
                    }
                    double* ptr2 = _ptr2;
                    for (int j = 0; j < nNodes; j++)
                    {
                        for (int k = 0; k < __dim; k++)
                        {
                            *ptr2 += J*_i.w * _i.wp[j] * _i.gravity[k];
                            ptr2++;
                        }
                    }
                }
                
            }
        }

        public mikity.LinearAlgebra.matrix T
        {
            set
            {
                if (mikity.LinearAlgebra.matrix.isCompatible(value, this.intPoints[0].stress))
                {
                    for (int i = 0; i < this.nIntPoints; i++)
                    {
                        intPoints[0].stress.CopyFrom(value);
                    }
                }
            }
        }

        public void memoryMetric()
        {
            this._refVolume = this.Volume;
            foreach (integralPoint p in this.intPoints)
            {
                p.memoryMetric();
            }
        }

        unsafe internal void Merge(particleSystem pS, vector grad,vector load)
        {
            fixed (double* _ptr1 = &elemGrad.rawData[0], _ptr2 = &elemLoad.rawData[0],_ptr3=&grad.rawData[0],_ptr4=&load.rawData[0])
            {
                fixed (int* _ptr5 = &el.rawData[0],_ptr6=&pS.globalIndex.rawData[0],_ptr7=&FriedChiken.index.rawData[0,0])
                {
                    double* ptr1 = _ptr1;
                    double* ptr2 = _ptr2;
                    double* ptr3 = _ptr3;
                    double* ptr4 = _ptr4;
                    
                    int* ptr5 = _ptr5;
                    int* ptr6 = _ptr6;
                    int* ptr7 = _ptr7;
                    for (int i = 0; i < this.nNodes; i++)
                    {
                        int t = *(ptr7 + *(ptr6 + *ptr5) * __dim);
                        //int t = FriedChiken.index[*(ptr6+*ptr5), 0];
                        for (int j = 0; j < this.__dim; j++)
                        {
                            *(ptr3 + t) += *ptr1;// elemGrad[i * __dim + j];
                            *(ptr4 + t) += *ptr2;// elemLoad[i * __dim + j];
                            t++;
                            ptr1++;
                            ptr2++;
                        }
                        ptr5++;
                    }
                }
            }
        }
        /// <summary>
        /// Merge for Jacobian
        /// </summary>
        /// <param name="pS"></param>
        /// <param name="jacobian"></param>
        /// <param name="num"></param>
        unsafe internal void Merge(particleSystem pS, matrix jacobian,int num)
        {
            fixed (double* _ptr1 = &elemGrad.rawData[0], _ptr2 = &elemLoad.rawData[0],_ptr3=&jacobian.rawData[num,0])
            {
                fixed (int* _ptr5 = &el.rawData[0],_ptr6=&pS.globalIndex.rawData[0],_ptr7=&FriedChiken.index.rawData[0,0])
                {
                    double* ptr1 = _ptr1;
                    double* ptr2 = _ptr2;
                    double* ptr3 = _ptr3;
                    
                    int* ptr5 = _ptr5;
                    int* ptr6 = _ptr6;
                    int* ptr7 = _ptr7;
                    for (int i = 0; i < this.nNodes; i++)
                    {
                        int t = *(ptr7 + *(ptr6 + *ptr5) * __dim);
                        //int t = FriedChiken.index[*(ptr6+*ptr5), 0];
                        for (int j = 0; j < this.__dim; j++)
                        {
                            *(ptr3 + t) += *ptr1;// elemGrad[i * __dim + j];
                            t++;
                            ptr1++;
                            ptr2++;
                        }
                        ptr5++;
                    }
                }
            }
        }
    }
    public class simplexElement : element
    {
        protected override void makeBoundary()
        {
            ///The number of the nodes is __N+1
            ///The number of the nodes of the boundary should be __N
            boundary = new matrixINT(this.__N +1,__N);
            for (int i = 0; i < __N; i++)
            {
                for (int k = i; k < __N + i; k++)
                {
                    if (k > __N)
                    {
                        boundary[i, k - 1] = k - __N - 1;
                    }
                    else
                    {
                        boundary[i, k - i] = k;
                    }
                }
            }
        }
                //一辺に並ぶ積分点の数
        public simplexElement(int[] _el)
            : base(_el)
        {
            __N = _el.Length - 1;
            if (__N < 1)
            {
                throw new mikity.Exceptions.NumberOfNodesIncompatibleException("Simplex element only accept 2,3,4 nodes.");
            }
            if (__N > 3)
            {
                throw new mikity.Exceptions.NumberOfNodesIncompatibleException("Simplex element only accept 2,3,4 nodes.");
            }
            
            nIntPoints = 1;
            nVisPoints = 1;
            intPoints = new integralPoint[nIntPoints];
            this.nodes = new matrix(this.nNodes, this.__dim);

            for (int i = 0; i < intPoints.Length; i++)
            {
                intPoints[i] = new integralPoint(__N, this.nodes,this.__dim);
            }
            //積分点要素内座標生成
            for (int i = 0; i < nIntPoints; i++)
            {
                double[] t = new double[__N];
                double F=__N+1;
                for (int j = 0; j < __N; j++)
                {
                    t[j] = (double)j / F;
                }
                intPoints[i].setLocalCoordinates(t);
            }
            //積分点積分用重み生成
            for (int i = 0; i < nIntPoints; i++)
            {
                double w = 1.0d;
                for (int j = 1; j <= __N; j++)
                {
                    w /= (double)j;
                }
                intPoints[i].setIntegralWeights(w);
            }
            //積分点位置ベクトル算出用重み生成
            for (int i = 0; i < nIntPoints; i++)
            {
                mikity.LinearAlgebra.vector tt = new mikity.LinearAlgebra.vector(nNodes);
                if (__N == 1)
                {
                    tt[0] = intPoints[i].localCoordinates(0);
                    tt[1] = -intPoints[i].localCoordinates(0) + 1;
                }
                if (__N == 2)
                {
                    tt[0] = intPoints[i].localCoordinates(0);
                    tt[1] = -intPoints[i].localCoordinates(0) + intPoints[i].localCoordinates(1);
                    tt[2] = -intPoints[i].localCoordinates(1) + 1;
                }
                if (__N == 3)
                {
                    tt[0] = intPoints[i].localCoordinates(0);
                    tt[1] = -intPoints[i].localCoordinates(0) + intPoints[i].localCoordinates(1);
                    tt[1] = -intPoints[i].localCoordinates(1) + intPoints[i].localCoordinates(2);
                    tt[2] = -intPoints[i].localCoordinates(2) + 1;
                }
                intPoints[i].setNodeWeights(tt);
            }
            //積分点基底ベクトル算出用重み生成
            for (int i = 0; i < nIntPoints; i++)
            {
                mikity.LinearAlgebra.matrix ttt = new mikity.LinearAlgebra.matrix(nNodes, __N);

                if (__N == 1)
                {
                    ttt[0, 0] = 1;
                    ttt[1, 0] = -1;
                }
                if (__N == 2)
                {
                    ttt[0, 0] = 1;
                    ttt[1, 0] = -1;
                    ttt[1, 1] = 1;
                    ttt[2, 1] = -1;
                }
                if (__N == 3)
                {
                    ttt[0, 0] = 1;
                    ttt[1, 0] = -1;
                    ttt[1, 1] = 1;
                    ttt[2, 1] = -1;
                    ttt[2, 2] = 1;
                    ttt[3, 2] = -1;
                }
                intPoints[i].setBaseWeights(ttt);
            }
            this.makeBoundary();
        }
        public static implicit operator simplexElement(int[] _el)
        {
            simplexElement e = new simplexElement(_el);
            return e;
        }

        public int dim
        {
            get
            {
                return __N;
            }
        }
    }
    public class isoparametricElement : element
    {
        protected override void makeBoundary()
        {
            boundary = new matrixINT(this.__N*2,mikity.MathUtil.__pow_INT_INT(2,__N-1));
            if (this.__N == 3)
            {
                boundary[0, 0] = this.el[0];
                boundary[0, 1] = this.el[1];
                boundary[0, 2] = this.el[3];
                boundary[0, 3] = this.el[2];

                boundary[1, 0] = this.el[4];
                boundary[1, 1] = this.el[5];
                boundary[1, 2] = this.el[7];
                boundary[1, 3] = this.el[6];

                boundary[2, 0] = this.el[0];
                boundary[2, 1] = this.el[1];
                boundary[2, 2] = this.el[5];
                boundary[2, 3] = this.el[4];

                boundary[3, 0] = this.el[2];
                boundary[3, 1] = this.el[3];
                boundary[3, 2] = this.el[7];
                boundary[3, 3] = this.el[6];

                boundary[4, 0] = this.el[0];
                boundary[4, 1] = this.el[2];
                boundary[4, 2] = this.el[6];
                boundary[4, 3] = this.el[4];

                boundary[5, 0] = this.el[1];
                boundary[5, 1] = this.el[3];
                boundary[5, 2] = this.el[7];
                boundary[5, 3] = this.el[5];

            }
        }
        public static mikity.LinearAlgebra.vector initCoord(int N)
        {
            mikity.LinearAlgebra.vector res = new mikity.LinearAlgebra.vector(N);
            if (N == 3)
            {
                res = new mikity.LinearAlgebra.vector(new double[3] { (-Math.Sqrt(3d / 5d) + 1d) / 2d, 1d / 2d, (Math.Sqrt(3d / 5d) + 1d) / 2d });
            }
            return res;
        }
        public static mikity.LinearAlgebra.vector initWeight(int N)
        {
            mikity.LinearAlgebra.vector res = new mikity.LinearAlgebra.vector(N);
            if (N == 3)
            {
                res = new mikity.LinearAlgebra.vector(new double[3] { 5d / 18d, 8d / 18d, 5d / 18d });
            }
            return res;
        }
        //一辺に並ぶ積分点の数
        private static int nIntMed = 3;
        //一辺に並ぶ描画点の数
        private static int nVisMed = 5;
        //積分点の座標生成数列
        private static mikity.LinearAlgebra.vector __u = initCoord(nIntMed);
        private static mikity.LinearAlgebra.vector __w = initWeight(nIntMed);
        //描画点の座標生成数列
        private static mikity.LinearAlgebra.vector __v = mikity.LinearAlgebra.vector.divideRange(0, 1.0, nVisMed);
        //形状関数生成用数列
        private mikity.LinearAlgebra.matrixINT psi, psi2, psi3;
        private mikity.LinearAlgebra.matrixINT phi;
        public static implicit operator isoparametricElement(int[] _el)
        {
            isoparametricElement e = new isoparametricElement(_el);
            return e;
        }
        public isoparametricElement(params int[] _el)
            : base(_el)
        {

            if (this.nNodes == 2)
            {
                __N = 1;
            }
            else if (this.nNodes == 4)
            {
                __N = 2;
            }
            else if (this.nNodes == 8)
            {
                __N = 3;
            }
            else
            {
                throw new mikity.Exceptions.NumberOfNodesIncompatibleException("Isoparametric element only accept 2,4,8 nodes.");
            }

            nNodes = this.nNodes;
            nIntPoints = mikity.MathUtil.__pow_INT_INT(nIntMed, __N);
            nVisPoints = mikity.MathUtil.__pow_INT_INT(nVisMed, __N);
            intPoints = new integralPoint[nIntPoints];
            this.nodes = new matrix(this.nNodes, this.__dim);

            for (int i = 0; i < intPoints.Length; i++)
            {
                intPoints[i] = new integralPoint(__N, this.nodes,this.__dim);
            }
            //形状関数生成用数列
            psi = mikity.MathUtil.generate(__N, 2);
            phi = -2 * psi + 1;
            psi2 = mikity.MathUtil.generate(__N, nIntMed);
            psi3 = mikity.MathUtil.generate(__N, nVisMed);
            //積分点要素内座標生成
            for (int i = 0; i < nIntPoints; i++)
            {
                double[] t = new double[__N];
                for (int j = 0; j < __N; j++)
                {
                    t[j] = __u[psi2[i, j]];
                }
                intPoints[i].setLocalCoordinates(t);
            }
            //積分点積分用重み生成
            for (int i = 0; i < nIntPoints; i++)
            {
                double w = 1.0d;
                for (int j = 0; j < __N; j++)
                {
                    w *= __w[psi2[i, j]];
                }
                intPoints[i].setIntegralWeights(w);
            }
            //重み算出用補助数列生成
            for (int i = 0; i < nIntPoints; i++)
            {
                mikity.LinearAlgebra.matrix T = new mikity.LinearAlgebra.matrix(nNodes, __N);
                for (int j = 0; j < nNodes; j++)
                {
                    for (int k = 0; k < __N; k++)
                    {
                        T[j, k] = intPoints[i].localCoordinates(k) * phi[j, k] + psi[j, k];
                    }
                }
                intPoints[i].setHelperNumber(T);
            }
            //積分点位置ベクトル算出用重み生成
            for (int i = 0; i < nIntPoints; i++)
            {
                mikity.LinearAlgebra.vector tt = new mikity.LinearAlgebra.vector(nNodes);
                for (int j = 0; j < nNodes; j++)
                {
                    tt[j] = 1.0d;
                    for (int k = 0; k < __N; k++)
                    {
                        tt[j] *= (intPoints[i].Tij[j, k]);
                    }
                }
                intPoints[i].setNodeWeights(tt);
            }
            //積分点基底ベクトル算出用重み生成
            for (int i = 0; i < nIntPoints; i++)
            {
                mikity.LinearAlgebra.matrix ttt = new mikity.LinearAlgebra.matrix(nNodes, __N);
                for (int j = 0; j < nNodes; j++)
                {
                    for (int k = 0; k < __N; k++)
                    {
                        ttt[j, k] = intPoints[i].wp[j] / (intPoints[i].Tij[j, k]) * phi[j, k];
                    }
                }
                intPoints[i].setBaseWeights(ttt);
            }
            this.makeBoundary();
        }

    }
}