using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace mikity.LinearAlgebra
{
    public partial class matrix : __matrix<double>/*, IBasicOperations<matrix>*/
    {
        public static matrix M3 = new matrix(3, 3, 0);
        public static matrix M2 = new matrix(2, 2, 0);
        public static matrix M1 = new matrix(1, 1, 0);
        public override string ToString()
        {
            string S = "";
            string F = "";
            int max = 0;
            for (int i = 0; i < nRow; i++)
            {
                for (int j = 0; j < nCol; j++)
                {
                    F = this[i, j].ToString("g3");
                    if (max < F.Length)
                    {
                        max = F.Length;
                    }
                }
            }
            max += 2;
            for (int i = 0; i < nRow; i++)
            {
//                S += "|[";
                for (int j = 0; j < nCol; j++)
                {
                    F = this[i, j].ToString("g3");
                    F=F.PadRight(max,' ');
                    S += F;
                    if (j < nCol - 1)
                    {
                        S += " ";
                    }

                }
                if (i != nRow - 1)
                {
                    S += ";\n";
                }
            }
            return S;
        }

        public matrix()
        {
        }
        public matrix(int n, int m)
            : base(n, m, 0)
        {
        }
        public matrix(int n, int m, double def)
            : base(n, m, def)
        {
        }
        public matrix(double[,] m)
            : base(m)
        {
        }

        unsafe public static void zeros(matrix m)
        {
//            for (int i = 0; i < m.nCol; i++)
//            {
//                for (int j = 0; j < m.nRow; j++)
//                {
//                    m[i, j] = 0;
//                }
//            }
            int S = m.nCol * m.nRow;
            fixed (double* _ptr = &m.rawData[0, 0])
            {
                double* ptr = _ptr;
                for (int i = 0; i < S; i++)
                {
                    *ptr = 0;
                    ptr++;
                }
            }
        }
        public static matrix zeros(int nRow, int nCol)
        {
            matrix res = new matrix(nRow, nCol, 0);
            return res;
        }

        public matrix eye()
        {
            for (int i = 0; i < this.nRow; i++)
            {
                for (int j = 0; j < this.nCol; j++)
                {
                    if (i == j)
                        this.rawData[i, j] = 1;
                    else
                        this.rawData[i, j] = 0;
                }
            }
            return this;
        }
        public static void eye(matrix m)
        {
            for (int i = 0; i < m.nRow; i++)
            {
                for (int j = 0; j < m.nCol; j++)
                {
                    if (i == j)
                        m.rawData[i, j] = 1;
                    else
                        m.rawData[i, j] = 0;
                }
            }
        }
        public static matrix eye(int nRow, int nCol)
        {
            return (new matrix(nRow, nCol, 0).eye());
        }
        public static matrix eye(int N)
        {
            return (new matrix(N, N).eye());
        }
        
        public void transpose(matrix m)
        {
            for (int i = 0; i < this.nRow; i++)
            {
                for (int j = 0; j < this.nCol; j++)
                {
                    this[i, j] = m[j, i];
                }
            }
        }
        public void transpose()
        {
            double[,] res=new double[this.nCol,this.nRow];
            for(int i=0;i<this.nCol;i++)
            {
                for(int j=0;j<this.nRow;j++)
                {
                    res[i,j]=this[j,i];
                }
            }
            this._data = res;
        }
        /// <summary>
        /// Replace all the elements by the inverse of given matrix.
        /// </summary>
        /// <remarks>matrix m must has same size of objective matrix</remarks>
        /// <param name="m">A matrix to be inversed.</param>
        public void asInverse(matrix m)
        {
            if (!isCompatible(m, this))
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException();
            }
            if (m.nCol != m.nRow)
            {
                return;
            }
            int N = m.nCol;
            switch(N){
                case 1:
                    this.inv1(m);
                    break;
                case 2:
                    this.inv2(m);
                    break;
                case 3:
                    this.inv3(m);
                    break;
            }
        }
        private void inv1(matrix m)
        {
            this[0, 0] = 1 / m[0, 0];
        }
        unsafe private void inv2(matrix m)
        {
            double __det = m.det2();
            fixed (double* _ptr1=&this.rawData[0,0],_ptr2 = &m.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                *(ptr1 + 0) = *(ptr2 + 3) / __det;
                *(ptr1 + 3) = *(ptr2 + 0) / __det;
                *(ptr1 + 1) = -*(ptr2 + 1) / __det;
                *(ptr1 + 2) = -*(ptr2 + 2) / __det;

                /*this[0, 0] = m[1, 1] / __det;
                this[1, 1] = m[0, 0] / __det;
                this[0, 1] = -m[0, 1] / __det;
                this[1, 0] = -m[1, 0] / __det;*/
            }
        }
        private static vector _a = new vector(3);
        private static vector _b = new vector(3);
        private static vector _c = new vector(3);
        private static vector _f = new vector(3);
        unsafe private void inv3(matrix m)
        {
            double _d = m.det3();
/*            _a[0] = m[0, 0];
            _a[1] = m[1, 0];
            _a[2] = m[2, 0];
            _b[0] = m[0, 1];
            _b[1] = m[1, 1];
            _b[2] = m[2, 1];
            _c[0] = m[0, 2];
            _c[1] = m[1, 2];
            _c[2] = m[2, 2];
            _f.crossProduct(_b, _c);
            _f.div(_d);
            this[0, 0] = _f[0];
            this[1, 0] = _f[1];
            this[2, 0] = _f[2];
            _f.crossProduct(_c, _a);
            _f.div(_d);
            this[0, 1] = _f[0];
            this[1, 1] = _f[1];
            this[2, 1] = _f[2];
            _f.crossProduct(_a, _b);
            _f.div(_d);
            this[0, 2] = _f[0];
            this[1, 2] = _f[1];
            this[2, 2] = _f[2];
*/
            fixed (double* _ptr1 = &this.rawData[0, 0], _ptr2 = &m.rawData[0, 0], _v1 = &_a.rawData[0], _v2 = &_b.rawData[0],_v3=&_c.rawData[0],_V=&_f.rawData[0])
            {
                *_v1 = *_ptr2;
                *(_v1 + 1) = *(_ptr2 + 3);
                *(_v1 + 2) = *(_ptr2 + 6);
                *_v2 = *(_ptr2+1);
                *(_v2 + 1) = *(_ptr2 + 4);
                *(_v2 + 2) = *(_ptr2 + 7);
                *_v3 = *(_ptr2+2);
                *(_v3 + 1) = *(_ptr2 + 5);
                *(_v3 + 2) = *(_ptr2 + 8);
                *_V = (*(_v2 + 1) * *(_v3 + 2) - *(_v2 + 2) * *(_v3 + 1))/_d;
                *(_V + 1) = (*(_v2 + 2) * *(_v3 + 0) - *(_v2 + 0) * *(_v3 + 2))/_d;
                *(_V + 2) = (*(_v2 + 0) * *(_v3 + 1) - *(_v2 + 1) * *(_v3 + 0))/_d;
                *_ptr1 = *_V;
                *(_ptr1+3) = *(_V+1);
                *(_ptr1+6) = *(_V+2);
                *_V = (*(_v3 + 1) * *(_v1 + 2) - *(_v3 + 2) * *(_v1 + 1)) / _d;
                *(_V + 1) = (*(_v3 + 2) * *(_v1 + 0) - *(_v3 + 0) * *(_v1 + 2)) / _d;
                *(_V + 2) = (*(_v3 + 0) * *(_v1 + 1) - *(_v3 + 1) * *(_v1 + 0)) / _d;
                *(_ptr1+1) = *_V;
                *(_ptr1 + 4) = *(_V + 1);
                *(_ptr1 + 7) = *(_V + 2);
                *_V = (*(_v1 + 1) * *(_v2 + 2) - *(_v1 + 2) * *(_v2 + 1)) / _d;
                *(_V + 1) = (*(_v1 + 2) * *(_v2 + 0) - *(_v1 + 0) * *(_v2 + 2)) / _d;
                *(_V + 2) = (*(_v1 + 0) * *(_v2 + 1) - *(_v1 + 1) * *(_v2 + 0)) / _d;
                *(_ptr1 + 2) = *_V;
                *(_ptr1 + 5) = *(_V + 1);
                *(_ptr1 + 8) = *(_V + 2);
            }
        }
        public double det
        {
            get
            {
                if (this.nCol != this.nRow)
                {
                    return 0;
                }
                int N = this.nCol;
                switch (N)
                {
                    case 0:
                        return 0;
                    case 1:
                        return this.det1();
                    case 2:
                        return this.det2();
                    case 3:
                        return this.det3();
                    default:
                        return det_general();
                }
            }
        }
        private double det_general()
        {
            double det=1.0,buf=0.0;
            int i,j,k;
            double[,] a = this.rawData;
            double[,] b = new double[this.nCol, this.nRow];
            int n=this.nCol;
            for (i = 0; i < n; i++)
            {
                for(j=0;j<n;j++)
                {
                    b[i, j] = a[i, j];
                }
            }
            //三角行列を作成
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    if (i < j)
                    {
                        buf = b[j, i] / b[i, i];
                        for (k = 0; k < n; k++)
                        {
                            b[j, k] -= b[i, k] * buf;
                        }
                    }
                }
            }
            //対角部分の積
            for (i = 0; i < n; i++)
            {
                det *= b[i, i];
            }

            return det;
        }
        private double det1()
        {
            return this[0, 0];
        }
        unsafe private double det2()
        {
            double v = 0;
            fixed (double* _ptr1 = &this.rawData[0, 0])
            {
                v += *(_ptr1 + 0) * *(_ptr1 + 3);
                v -= *(_ptr1 + 1) * *(_ptr1 + 2);
            }
            return v;
        }
        unsafe private double det3()
        {
            double res = 0.0;
//            res = this[0, 0] * this[1, 1] * this[2, 2];
//            res += this[1, 0] * this[2, 1] * this[0, 2];
//            res += this[2, 0] * this[0, 1] * this[1, 2];
//            res -= this[2, 0] * this[1, 1] * this[0, 2];
//            res -= this[1, 0] * this[0, 1] * this[2, 2];
//            res -= this[0, 0] * this[2, 1] * this[1, 2];
            fixed (double* _ptr = &this.rawData[0, 0])
            {
                res = *(_ptr) * *(_ptr + 4) * *(_ptr + 8);
                res += *(_ptr+3) * *(_ptr + 7) * *(_ptr + 2);
                res += *(_ptr+6) * *(_ptr + 1) * *(_ptr + 5);
                res -= *(_ptr+6) * *(_ptr + 4) * *(_ptr + 2);
                res -= *(_ptr+3) * *(_ptr + 1) * *(_ptr + 8);
                res -= *(_ptr) * *(_ptr + 7) * *(_ptr + 5);
            }
            return res;
        }
        ///Operators
        public static matrix operator +(matrix m1, matrix m2)
        {
            matrix res = new matrix(m1.nRow, m1.nCol);
            Add(m1, m2, res);
            return res;
        }
        public static matrix operator +(matrix m, int p)
        {
            matrix res = new matrix(m.nRow, m.nCol);
            B_equals_aI_plus_A(p, m, res);
            return res;
        }
        public static matrix operator +(int p, matrix m)
        {
            matrix res = new matrix(m.nRow, m.nCol);
            B_equals_aI_plus_A(p, m, res);
            return res;
        }
        public static matrix operator -(matrix m1, matrix m2)
        {
            matrix res = new matrix(m1.nRow, m1.nCol);
            Subtract(m1, m2, res);
            return res;
        }
        public static matrix operator -(matrix m, int p)
        {
            matrix res = new matrix(m.nRow, m.nCol);
            Subtract(m, p, res);
            return res;
        }
        public static matrix operator -(int p, matrix m)
        {
            matrix res = new matrix(m.nRow, m.nCol);
            Subtract(m, p, res);
            return res;
        }
        public static matrix operator *(matrix m, int p)
        {
            matrix res = new matrix(m.nRow, m.nCol);
            Mult(m, p, res);
            return res;
        }
        public static matrix operator *(int p, matrix m)
        {
            matrix res = new matrix(m.nRow, m.nCol);
            Mult(m, p, res);
            return res;
        }
        public static matrix operator *(matrix m1, matrix m2)
        {
            return Mult(m1, m2);
        }
        public static matrix operator /(matrix m, int p)
        {
            matrix res = new matrix(m.nRow, m.nCol);
            Div(m, p, res);
            return res;
        }
        public static matrix operator /(int p, matrix m)
        {
            matrix res = new matrix(m.nRow, m.nCol);
            Div(m, p, res);
            return res;
        }
        /// <summary>
        /// Frobenious norm
        /// returns sum of every pair of component
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        unsafe public static double DoubleDot(matrix m1, matrix m2)
        {
            double res = 0;
//if (!isCompatible(m1, m2))
//            {
//                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException("v8idkd");
//            }
//            for (int i = 0; i < m2.nCol; i++)
//            {
//                for (int j = 0; j < m2.nRow; j++)
//                {
//                    res+=m2[i, j] * m1[i, j];
//                }
            //}
            fixed (double* _ptr1 = &m1.rawData[0, 0], _ptr2 = &m2.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                int S=m2.nCol*m2.nRow;
                for (int i = 0; i < S; i++)
                {
                    res += *ptr1 * *ptr2;
                    ptr1++;
                    ptr2++;
                }
            }
            return res;
        }
        public double bilinear(vector v1,vector v2)
        {
            return (matrix.bilinear(this,v1,v2));
        }

        unsafe public static double bilinear(matrix m, vector v1, vector v2)
        {
            if (m.nRow != v1.nElem)
            {
                throw new mikity.Exceptions.SizeOfVectorsNotCompatibleException("vectors must be compatible with the matrix");
            }
            if (m.nCol != v1.nElem)
            {
                throw new mikity.Exceptions.SizeOfVectorsNotCompatibleException("vectors must be compatible with the matrix");

            }
            double res = 0;
            fixed (double* _ptr1 = &m.rawData[0, 0], _ptr2 = &v1.rawData[0], _ptr3 = &v2.rawData[0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                for (int i = 0; i < m.nRow; i++)
                {
                    double* ptr3 = _ptr3;
                    for (int j = 0; j < m.nCol; j++)
                    {
                        res += *ptr2 * *ptr1 * *ptr3;
                        ptr1++;
                        ptr3++;
                    }
                    ptr2++;
                }
            }
            return res;
        }
        /// <summary>
        /// nxm matrix to nxm vector
        /// </summary>
        /// <param name="v"></param>
        unsafe public void DecomposeTo(vector v)
        {
            fixed (double* _ptr1 = &this.rawData[0, 0], _ptr2 = &v.rawData[0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                for (int i = 0; i < nRow * nCol; i++)
                {
                    *ptr2 = *ptr1;
                    ptr1++;
                    ptr2++;
                }
            }
        }
        /// <summary>
        /// Mask i-th column if v[i]==1
        /// Sastain i-th column if v[i]==0
        /// </summary>
        /// <param name="v"></param>
        unsafe public void mask(vector v)
        {
            fixed (double* _ptr1 = &this.rawData[0, 0], _ptr2 = &v.rawData[0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                for (int i = 0; i < this.nRow; i++)
                {
                    ptr2 = _ptr2;
                    for (int j = 0; j < this.nCol; j++)
                    {
                        *ptr1 = *ptr1 - (*ptr1 * *ptr2);
                        ptr1++;
                        ptr2++;
                    }
                }
            }
        }

    }
   

}
