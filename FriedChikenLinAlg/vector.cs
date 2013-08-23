using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace mikity.LinearAlgebra
{
   
    public class vector:__vector<double>,IBasicOperations<vector>
    {
        private static double m_tolerance=0.1;
        public static double tolerance
        {
            set
            {
                m_tolerance = value;
            }
            get
            {
                if (m_tolerance == -1.0)
                {
                    throw new mikity.Exceptions.NotInitializedException("tolerance of vectors not set.");
                }
                else
                {
                    return m_tolerance;
                }
            }
        }
        public static vector V3 = new vector(3,0);
        public override string ToString()
        {
            string S = "";
            string F = "";
            int max = 0;
            for (int i = 0; i < nElem; i++)
            {
                F = this[i].ToString("g3");
                if (max < F.Length)
                {
                    max = F.Length;
                }
            }
            max += 2;
            for (int i = 0; i < nElem; i++)
            {
//                S += "|";
                F = this[i].ToString("g3");
                F = F.PadRight(max, ' ');
                S += F;
//                S+="|\n";
                
            }
            return S;
        }

        public vector()
            : base()
        {
        }
        public vector(int N)
            : base(N,0)
        {
        }
        public vector(int N, double def)
            : base(N, def)
        {
        }
        public vector(double[] p)
            : base(p)
        {
        }
        /// <summary>
        /// this=x-yA
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="A"></param>
        /// <returns></returns>
        unsafe public vector xminusyA(vector x, vector y, matrix A)
        {
            fixed (double* _ptr1 = &this.rawData[0], _ptr2 = &x.rawData[0], _ptr3 = &y.rawData[0], _ptr4 = &A.rawData[0, 0])
            {
                double* ptr1 = _ptr1, ptr2 = _ptr2, ptr3 = _ptr3, ptr4 = _ptr4;
                for (int i = 0; i < this.nElem; i++)
                {
                    ptr3 = _ptr3;
                    ptr4 = _ptr4 + i;
                    double v = 0;
                    for (int j = 0; j < A.nRow; j++)
                    {
                        v += *ptr3 * *ptr4;

                        ptr4+=A.nCol;
                        ptr3++;
                    }
                    *ptr1 = *ptr2 - v;
                    ptr1++;
                    ptr2++;
                }
            }
            return this;
        }
        public vector MinusThis()
        {
            
            return MinusTo(this);
        }
        unsafe public vector MinusTo(vector v)
        {
            fixed (double* _ptr1 = &this.rawData[0],_ptr2=&v.rawData[0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                for (int i = 0; i < nElem; i++)
                {
                    *ptr2 = -*ptr1;
                    ptr1++;
                    ptr2++;
                }
            }
            return v;
        }
        /// <summary>
        /// x+=pv
        /// </summary>
        /// <param name="p">Scalar</param>
        /// <param name="v">Vector</param>
        /// <returns>x</returns>
        unsafe public vector Add(double p, vector v)
        {
            //            for(int i=0;i<this.nElem;i++)
            //            {
            //                this[i]+=v[i]*p;
            //            }
            int S = nElem;
            fixed (double* _ptr1 = &this.rawData[0], _ptr2 = &v.rawData[0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                for (int i = 0; i < S; i++)
                {
                    *ptr1 += *ptr2 * p;
                    ptr1++;
                    ptr2++;
                }
            }
            return this;
        }
        /// <summary>
        /// a=x+yA
        /// </summary>
        /// <param name="x">vector</param>
        /// <param name="y">vector</param>
        /// <param name="A">matrix</param>
        /// <returns>a</returns>
        unsafe public vector xplusyA(vector x, vector y, matrix A)
        {
            fixed (double* _ptr1 = &this.rawData[0], _ptr2 = &x.rawData[0], _ptr3 = &y.rawData[0], _ptr4 = &A.rawData[0, 0])
            {
                double* ptr1 = _ptr1, ptr2 = _ptr2, ptr3 = _ptr3, ptr4 = _ptr4;
                for (int i = 0; i < this.nElem; i++)
                {
                    ptr3 = _ptr3;
                    ptr4 = _ptr4 + i;
                    double v = 0;
                    
                    for (int j = 0; j < A.nRow; j++)
                    {
                        v += *ptr3 * *ptr4;

                        ptr4+=A.nCol;
                        ptr3++;
                    }
                    *ptr1 = *ptr2 + v;
                    ptr1++;
                    ptr2++;
                }
            }
            return this;
        }
        unsafe public vector zeros()
        {
            //double[] p = this.rowData;
            //for (int i = 0; i < this.nElem; i++)
            //{
            //    p[i] = 0;
            //}
            int S = this.nElem;
            fixed (double* _ptr = &this.rawData[0])
            {
                double* ptr = _ptr;
                for (int i = 0; i < S; i++)
                {
                    *ptr = 0;
                    ptr++;
                }
            }
            return this;
        }
        public static vector zeros(int N)
        {
            return new vector(N).zeros();
        }
        unsafe public vector ones()
        {
            //double[] p = this.rowData;
            //for (int i = 0; i < this.nElem; i++)
            //{
            //    p[i] = 0;
            //}
            int S = this.nElem;
            fixed (double* _ptr = &this.rawData[0])
            {
                double* ptr = _ptr;
                for (int i = 0; i < S; i++)
                {
                    *ptr = 1.0;
                    ptr++;
                }
            }
            return this;
        }
        public static vector ones(int N)
        {
            return new vector(N).ones();
        }
        public double norm
        {
            get
            {
                return Math.Sqrt(vector.dot(this, this));
            }
        }
        public vector Add(vector v)
        {
            for(int i=0;i<this.nElem;i++)
            {
                this[i]+=v[i];
            }
            return this;
        }
        unsafe public static double dot(vector v1, vector v2)
        {
            double res = 0;
            //if (!isCompatible(v1, v2))
            //{
            //    throw new mikity.Exceptions.SizeOfVectorsNotCompatibleException();
            //}
            //for (int i = 0; i < v1.nElem; i++)
            //{
            //    res += v1[i] * v2[i];
            //}
            int S = v1.nElem;
            fixed (double* _ptr1 = &v1.rawData[0], _ptr2 = &v2.rawData[0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                for (int i = 0; i < S; i++)
                {
                    res += *ptr1 * *ptr2;
                    ptr1++;
                    ptr2++;
                }
            }
            return res;
        }
        unsafe public vector dividedby(double v)
        {
/*            for (int i = 0; i < nElem; i++)
            {
                this[i] /= v;
            }*/
            fixed (double* _ptr1 = &this.rawData[0])
            {
                double* ptr1=_ptr1;
                for (int i = 0; i < nElem; i++)
                {
                    *ptr1 /= v;
                    ptr1++;
                }
            }
            return this;
        }
        unsafe public vector times(double v)
        {
            /*for (int i = 0; i < nElem; i++)
            {
                this[i] *= v;
            }*/
            fixed (double* _ptr1 = &this.rawData[0])
            {
                double* ptr1 = _ptr1;
                for (int i = 0; i < nElem; i++)
                {
                    *ptr1 *= v;
                    ptr1++;
                }
            }
            return this;
        }
        public vector Add(double v)
        {
            for (int i = 0; i < nElem; i++)
            {
                this[i] += v;
            }
            return this;
        }
        public vector Subtract(double v)
        {
            for (int i = 0; i < nElem; i++)
            {
                this[i] -= v;
            }
            return this;
        }
        unsafe public vector Subtract(vector v)
        {
/*            for (int i = 0; i < nElem; i++)
            {
                this[i] -= v[i];
            }
 * */
            fixed (double* _p1 = &v.rawData[0],_p2=&this.rawData[0])
            {
                double* p1 = _p1;
                double* p2 = _p2;
                for (int i = 0; i < nElem; i++)
                {
                    *p2 -= *p1;
                    p1++;
                    p2++;
                }

            }
            return this;
        }
        unsafe public vector Subtract(vector a, vector b)
        {
            fixed (double* _p1 = &a.rawData[0], _p2 = &b.rawData[0],_p3=&this.rawData[0])
            {
                double* p1 = _p1;
                double* p2 = _p2;
                double* p3 = _p3;
                for (int i = 0; i < nElem; i++)
                {
                    *p3 = *p1 - *p2;
                    p1++;
                    p2++;
                    p3++;
                }

            }
            return this;
        }
        unsafe public vector Subtract(double a, vector v)
        {
            fixed (double* _p1 = &v.rawData[0], _p2 = &this.rawData[0])
            {
                double* p1 = _p1;
                double* p2 = _p2;
                for (int i = 0; i < nElem; i++)
                {
                    *p2 -= a* *p1;
                    p1++;
                    p2++;
                }

            }
            return this;
        }
        public static matrix tensorProduct(vector v1, vector v2)
        {
            matrix res = new matrix(v1.nElem,v2.nElem).zeros();
            for (int i = 0; i < v1.nElem; i++)
            {
                for (int j = 0; j < v2.nElem; j++)
                {
                    res[i,j]= v1[i] * v2[j];
                }
            }
            return res;
        }
        public vector crossProduct(vector v1,vector v2)
        {
            if (!isCompatible(v1, v2, this,vector.V3))
            {
                throw new mikity.Exceptions.SizeOfVectorsNotCompatibleException();
            }
            this[0] = v1[1] * v2[2] - v1[2] * v2[1];
            this[1] = v1[2] * v2[0] - v1[0] * v2[2];
            this[2] = v1[0] * v2[1] - v1[1] * v2[0];
            return this;
        }
        public static vector crossProduct(vector v1, vector v2,vector v3)
        {
            if (!isCompatible(v1, v2, v3,vector.V3))
            {
                throw new mikity.Exceptions.SizeOfVectorsNotCompatibleException();
            }
            v3[0] = v1[1] * v2[2] - v1[2] * v2[1];
            v3[1] = v1[2] * v2[0] - v1[0] * v2[2];
            v3[2] = v1[0] * v2[1] - v1[1] * v2[0];
            return v3;
        }
        public static vector range(double start, double end)
        {
            return range(start, end, 1);
        }
        public static vector range(double start, double end, double step)
        {
            vector res = new vector((int)((end - start) / step + 1));
            int j = 0;
            for (double i = start; i < end;i += step)
            {
                res[j] = i;
                j++;
            }
            return res;
        }
        public static vector divideRange(double start, double end,int N)
        {
            vector res = new vector(N);
            double step = (end - start) / (N - 1);
            double cur = start;
            for(int i=0;i<N;i++)
            {
                res[i] = cur;
                cur += step;
            }
            return res;
        }
        public static implicit operator vector(double[] v)
        {
            return new vector(v);
        }
        public static bool isSame(vector v1,vector v2)
        {
            double s = 0;
            double t=0;
            if (isCompatible(v1, v2))
            {
                for (int i = 0; i < v1.nElem; i++)
                {
                    t=v1[i]-v2[i];
                    s += t * t;
                }
                s = Math.Sqrt(s);
            }
            else
            {
                return false;
            }
            if (s <= vector.tolerance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        unsafe public void mask(vector mask)
        {
            fixed (double* _ptr1 = &this.rawData[0], _ptr2 = &mask.rawData[0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                for (int i = 0; i < this.nElem; i++)
                {
                    *ptr1 *= *ptr2;
                    ptr1++;
                    ptr2++;
                }
            }
        }
        unsafe public void copyFrom(vector v)
        {
            if (isCompatible(this, v))
            {
                fixed (double* _ptr1 = &this.rawData[0], _ptr2 = &v.rawData[0])
                {
                    double* ptr1 = _ptr1;
                    double* ptr2 = _ptr2;
                    for (int i = 0; i < this.nElem; i++)
                    {
                        *ptr1 = *ptr2;
                        ptr1++;
                        ptr2++;
                    }
                }
            }
            else
            {
                throw new mikity.Exceptions.SizeOfVectorsNotCompatibleException();
            }
        }


        unsafe public void dotMult(vector a,vector b)
        {
            vector m = b;
            vector o = a;
            vector r = this;
            int N = this.nElem;

            /*            for (int i = 0; i < DOF; i++)
                        {
                            r[i] = o[i] * m[i];
                        }
            */
            fixed (double* _ptr1 = &o.rawData[0], _ptr2 = &m.rawData[0], _ptr3 = &r.rawData[0])
            {
                double* ptr1 = _ptr1, ptr2 = _ptr2, ptr3 = _ptr3;
                for (int i = 0; i < N; i++)
                {
                    *ptr3 = *ptr1 * *ptr2;
                    ptr1++;
                    ptr2++;
                    ptr3++;
                }
            }

        }



        unsafe public void Decompose(matrix m)
        {
            fixed (double* _ptr1 = &this.rawData[0], _ptr2 = &m.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                for (int i = 0; i < m.nRow*m.nCol; i++)
                {
                    *ptr2 = *ptr1;
                    ptr1++;
                    ptr2++;
                }
            }
        }
    }
}
