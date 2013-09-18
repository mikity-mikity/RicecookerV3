using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using mikity.LinearAlgebra;
namespace mikity.NumericalMethodHelper
{

    public class integralPoint:point
    {
        //節点
        private matrix nodes;
        private double[] loCo;
        //重み算出用の補助数列
        public double[,] Tij;
        //積分用の重み
        public double w;
        //位置算出用の重み
        public double[] wp;
        //基底算出用の重み
        public double[,] wg;
        //リーマン計量拘束時のインデクス
        public matrixINT number;
        public void setLocalCoordinates(double[] _lc)
        {
            this.loCo = _lc;
        }
        public double localCoordinates(int N)
        {
            return this.loCo[N];
        }
        public void setHelperNumber(double[,] _Tij)
        {
            Tij = _Tij;
        }
        public void setIntegralWeights(double _w)
        {
            this.w = _w;
        }
        public void setNodeWeights(double[] _wp)
        {
            wp = _wp;
        }
        public void setBaseWeights(double[,] _wg)
        {
            wg = _wg;
        }
        //計量、計量の逆行列、参照計量、応力(混合成分)、応力(反変成分),歪
        public matrix metric, invMetric,refMetric, refInvMetric,stress,stress2,strain;
        public vector gravity;
        //計量の微分
        public matrixVector difMetric;
        public matrix covariantBases;
        public int dof;
        public int __N
        {
            set;
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="N">要素の次元</param>
        /// <param name="dim">１節点の自由度、3か6が普通</param>
        public integralPoint(int N,matrix _nodes,int dim):base()
        {

            //親要素の節点
            this.nodes = _nodes;
            __N = N;
            this.dof = __dim * _nodes.nRow;
            metric = new matrix(N, N).eye();
            refMetric = new matrix(N, N).zeros();
            invMetric = new matrix(N, N).eye();
            refInvMetric = new matrix(N, N).eye();
            stress = new matrix(N, N).eye();
            stress2 = new matrix(N, N).eye();
            strain = new matrix(N, N).zeros();
            difMetric = new matrixVector(N,N,dof);
            covariantBases = new matrix(N,__dim).zeros();
            number = new matrixINT(N, N).zeros();
            gravity = new vector(dim).zeros();
            gravity[dim - 1] = 1.0;
        }
        unsafe public void Update()
        {
            int nNodes = nodes.nRow;
            fixed (double* _ptr1 = &nodes.rawData[0, 0])
            {
                //積分点の位置算出
                //position.zeros();
                //for (int i = 0; i < nodes.nCol; i++)
                //{
                //    //重み付き足しあわせ
                //    for (int j = 0; j < dim; j++)
                //    {
                //        position[j] += nodes[i, j] * wp[i];
                //    }
                //}
                fixed (double* _ptr2 = &rawData[0], _ptr3 = &wp[0])
                {
                    double* ptr1 = _ptr1;
                    double* ptr2 = _ptr2;
                    double* ptr3 = _ptr3;
                    for (int i = 0; i < __dim; i++)
                    {
                        *ptr2 = 0;
                        ptr2++;
                    }
                    ptr3 = _ptr3;
                    for (int i = 0; i < nNodes; i++)
                    {
                        ptr2 = _ptr2;
                        for (int j = 0; j < __dim; j++)
                        {
                            *ptr2 += *ptr1 * *ptr3;
                            ptr2++;
                            ptr1++;
                        }
                        ptr3++;
                    }

                }
                //積分点における基底ベクトル算出
                /*covariantBases.zeros();
                for (int j = 0; j < nodes.nCol; j++)
                {
                    for (int i = 0; i < __N; i++)
                    {
                        for (int k = 0; k < dim; k++)
                        {
                            covariantBases[i, k] += nodes[j, k] * wg[j, i];
                        }
                    }
                }
                */
                fixed (double* _ptr2 = &covariantBases.rawData[0, 0], _ptr3 = &wg[0, 0])
                {
                    double* ptr1 = _ptr1;//nodes[0,0];
                    double* ptr3 = _ptr3;//wg[0,0];
                    double* ptr4 = _ptr1;//nodes[0,0];
                    double* ptr2 = _ptr2;//covariantBases[0,0]
                    for (int i = 0; i < __N; i++)
                    {
                        for (int k = 0; k < __dim; k++)
                        {
                            *ptr2 = 0;
                            ptr2++;
                        }
                    }
                    for (int j = 0; j < nNodes; j++)
                    {
                        ptr2 = _ptr2;//OK
                        for (int i = 0; i < __N; i++)
                        {
                            ptr4 = ptr1;
                            for (int k = 0; k < __dim; k++)
                            {
                                *ptr2 += *ptr1 * *ptr3;//nodes[j,k]*wg[j,i]
                                ptr2++;//OK
                                ptr1++;
                            }
                            ptr1 = ptr4;
                            ptr3++;//OK
                        }
                        ptr1 += __dim;
                    }


                    //積分点における計量行列算出
                    /*for (int i = 0; i < __N; i++)
                    {
                        for (int j = 0; j < __N; j++)
                        {
                            double s = 0;
                            for (int k = 0; k < dim; k++)
                            {
                                s += covariantBases[i, k] * covariantBases[j, k];
                            }
                            metric[i, j] = s;
                        }

                    }*/
                    fixed (double* _ptr5 = &metric.rawData[0, 0])
                    {
                        double* ptr5 = _ptr5;//metrix
                        double* ptI = _ptr2;
                        double* ptJ = _ptr2;
                        double* ptr6 = _ptr2;
                        double s = 0;
                        for (int i = 0; i < __N; i++)
                        {
                            ptJ = _ptr2;
                            for (int j = 0; j < __N; j++)
                            {
                                s = 0;
                                ptr6 = ptI;
                                for (int k = 0; k < __dim; k++)
                                {
                                    s += *ptI * *ptJ;
                                    ptJ++;
                                    ptI++;
                                }
                                *ptr5=s;
                                ptr5++;
                                ptI = ptr6;
                            }
                            ptI += __dim;
                        }
                    }
                    //計量行列の逆行列の計算
                    invMetric.asInverse(metric);
                }
                //歪の計算
                matrix.Subtract(this.metric, this.refMetric, this.strain);
                /*
                //計量行列の微分列の生成
                for (int i = 0; i < nodes.nCol; i++)
                {
                    for (int u = 0; u < dim; u++)
                    {
                        int _index = index[i][u];
                        for (int j = 0; j < __N; j++)
                        {
                            for (int k = 0; k < __N; k++)
                            {
                                double s = 0;
                                for (int t = 0; t < nodes.nCol; t++)
                                {
                                    s += wg[i, j] * nodes[t, u] * wg[t, k];
                                    s += wg[i, k] * nodes[t, u] * wg[t, j];
                                }
                                difMetric[_index][j, k] = s;
                            }
                        }
                    }
                }*/
                //_ptr1:node[0,0]
                fixed (double* _ptr2 = &wg[0, 0])
                {
                    int _index = 0;
                    double* ptr4 = _ptr2;
                    for (int i = 0; i < nNodes; i++)
                    {
                        for (int u = 0; u < __dim; u++)
                        {
                            fixed (double* _ptr3 = &difMetric[_index].rawData[0, 0])
                            {
                                double* ptr3 = _ptr3;
                                for (int j = 0; j < __N; j++)
                                {
                                    for (int k = 0; k < __N; k++)
                                    {
                                        double* ptr1 = _ptr1 + u;
                                        double s = 0;
                                        double* ptr5 = _ptr2;
                                        for (int t = 0; t < nNodes; t++)
                                        {
                                            //s += wg[i, j] * nodes[t, u] * wg[t, k];
                                            //s += wg[i, k] * nodes[t, u] * wg[t, j];
                                            s += *(ptr4 + j) * *ptr1 * *(ptr5 + k);// wg[t, k];
                                            s += *(ptr4 + k) * *ptr1 * *(ptr5 + j);// wg[t, j];
                                            ptr1 += __dim;
                                            ptr5+=__N;
                                        }
                                        *ptr3 = s;
                                        ptr3++;
                                    }
                                }
                            }
                            _index++;
                        }
                        ptr4+=__N;
                    }
                }
            }
        }


        internal void memoryMetric()
        {
            this.refMetric.CopyFrom(this.metric);
            this.refInvMetric.CopyFrom(this.invMetric);
        }
    }
}

