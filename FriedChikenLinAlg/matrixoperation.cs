using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace mikity.LinearAlgebra
{
    public partial class matrix : __matrix<double>, IBasicOperations<matrix>
    {

        //basic algebras
        public matrix div(double v)
        {
            for (int i = 0; i < nRow; i++)
            {
                for (int j = 0; j < nCol; j++)
                {
                    this[i, j] /= v;
                }
            }
            return this;
        }
        unsafe public matrix mult(double v)
        {
            /*for (int i = 0; i < nRow; i++)
            {
                for (int j = 0; j < nCol; j++)
                {
                    this[i,j] *= v;
                }
            }*/
            fixed (double* _ptr = &this.rawData[0, 0])
            {
                double* ptr = _ptr;
                int S = nCol * nRow;
                for (int i = 0; i < S; i++)
                {
                    *ptr *= v;
                }
            }
            return this;
        }
        public matrix Add(double v)
        {
            for (int i = 0; i < nRow; i++)
            {
                for (int j = 0; j < nCol; j++)
                {
                    this[i, j] += v;
                }
            }
            return this;
        }
        unsafe public matrix Subtract(double v)
        {
            fixed (double* _ptr1 = &this.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                for (int i = 0; i < this.nCol * this.nRow; i++)
                {
                    *ptr1 -= v;
                    ptr1++;
                }
            }
            return this;
        }
        unsafe public matrix Subtract(matrix m)
        {
            fixed (double* _ptr1 = &this.rawData[0, 0], _ptr2 = &m.rawData[0, 0])
            {
                double* ptr1 = _ptr1, ptr2 = _ptr2;
                for (int i = 0; i < this.nCol * this.nRow; i++)
                {
                    *ptr1 -= *ptr2;
                    ptr1++;
                    ptr2++;
                }
            }
            return this;
        }
        public static void Add(matrix m1, matrix m2, matrix m3)
        {
            if (!isCompatible(m1, m2, m3))
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException("udufu");
            }
            for (int i = 0; i < m3.nRow; i++)
            {
                for (int j = 0; j < m3.nCol; j++)
                {
                    m3[i, j] = m1[i, j] + m2[i, j];
                }
            }
        }
        public static void B_equals_aI_plus_A(int a, matrix A, matrix B)
        {
            if (!isCompatible(A, B))
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException("9gk");
            }
            for (int i = 0; i < B.nRow; i++)
            {
                for (int j = 0; j < B.nCol; j++)
                {
                    B[i, j] = A[i, j] + a;
                }
            }
        }
        unsafe public static void Subtract(matrix m1, matrix m2, matrix m3)
        {
            if (!isCompatible(m1, m2, m3))
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException("udufu");
            }
            /*            for (int i = 0; i < m3.nCol; i++)
                        {
                            for (int j = 0; j < m3.nRow; j++)
                            {
                                m3[i, j] = m1[i, j] - m2[i, j];
                            }
                        }
             * */
            fixed (double* _ptr1 = &m1.rawData[0, 0], _ptr2 = &m2.rawData[0, 0], _ptr3 = &m3.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                double* ptr3 = _ptr3;
                int S = m3.nRow * m3.nCol;
                for (int i = 0; i < S; i++)
                {
                    *ptr3 = *ptr1 - *ptr2;
                    ptr1++;
                    ptr2++;
                    ptr3++;
                }
            }
        }
        public static void Subtract(matrix m1, int p, matrix m2)
        {
            if (!isCompatible(m1, m2))
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException("9gk");
            }
            for (int i = 0; i < m2.nRow; i++)
            {
                for (int j = 0; j < m2.nCol; j++)
                {
                    m2[i, j] = m1[i, j] - p;
                }
            }
        }
        public static void Mult(matrix m1, int p, matrix m2)
        {
            if (!isCompatible(m1, m2))
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException("v8idkd");
            }
            for (int i = 0; i < m2.nRow; i++)
            {
                for (int j = 0; j < m2.nCol; j++)
                {
                    m2[i, j] = m1[i, j] * p;
                }
            }
        }
        public static matrix Mult(matrix m1, matrix m2)
        {
            matrix res = new matrix(m1.nRow, m2.nCol, 0);
            /*if (!isCompatible(m1, m2,res))
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException("v8idkd");
            }*/
            if (m1.nCol != m2.nRow)
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException("v8idkd");
            }
            for (int i = 0; i < res.nRow; i++)
            {
                for (int j = 0; j < res.nCol; j++)
                {
                    for (int k = 0; k < m1.nCol; k++)
                    {
                        res[i, j] = res[i, j] + m1[i, k] * m2[k, j];
                    }
                }
            }
            return res;
        }
        /// <summary>
        /// m2=m1/p
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="p"></param>
        /// <param name="m2"></param>
        public static matrix Div(matrix m1, int p, matrix m2)
        {
            if (!isCompatible(m1, m2))
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException("v8idkd");
            }
            for (int i = 0; i < m2.nRow; i++)
            {
                for (int j = 0; j < m2.nCol; j++)
                {
                    m2[i, j] = m1[i, j] / p;
                }
            }
            return m2;
        }
        unsafe public void CopyFrom(matrix m)
        {
            if (isCompatible(this, m))
            {
                fixed (double* _ptr1 = &this.rawData[0, 0], _ptr2 = &m.rawData[0, 0])
                {
                    double* ptr1 = _ptr1;
                    double* ptr2 = _ptr2;
                    for (int i = 0; i < m.nRow * m.nCol; i++)
                    {
                        *ptr1 = *ptr2;
                        ptr1++;
                        ptr2++;
                    }
                }
            }
            else
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException();
            }
        }
        /// <summary>
        /// C=C+xA
        /// </summary>
        /// <param name="x"></param>
        /// <param name="A"></param>
        /// <returns></returns>
        unsafe public matrix plus_xA(double x, matrix A)
        {
            fixed (double* _ptr1 = &this.rawData[0, 0], _ptr2 = &A.rawData[0, 0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                for (int i = 0; i < this.nCol * nRow; i++)
                {
                    *ptr1 += *ptr2 * x;
                    ptr1++;
                    ptr2++;
                }
            }
            return this;

        }
        unsafe public static void y_equals_xA(vector x, matrix A, vector y)
        {
            fixed (double* _ptr1 = &x.rawData[0], _ptr2 = &A.rawData[0, 0], _ptr3 = &y.rawData[0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                double* ptr3 = _ptr3;
                for (int j = 0; j < A.nCol; j++)
                {
                    ptr2 = _ptr2 + j;
                    ptr1 = _ptr1;
                    double v = 0;
                    for (int i = 0; i < A.nRow; i++)
                    {
                        v += *ptr2 * *ptr1;
                        ptr1++;
                        ptr2 += A.nCol;
                    }
                    *ptr3 = v;
                    ptr3++;
                }
            }
        }

        unsafe public static void y_equals_Ax(matrix A,vector x, vector y)
        {
            fixed (double* _ptr1 = &x.rawData[0], _ptr2 = &A.rawData[0, 0], _ptr3 = &y.rawData[0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                double* ptr3 = _ptr3;
                for (int j = 0; j < A.nRow; j++)
                {
                    ptr1 = _ptr1;
                    double v = 0;
                    for (int i = 0; i < A.nCol; i++)
                    {
                        v += *ptr2 * *ptr1;
                        ptr1++;
                        ptr2++;
                    }
                    *ptr3 = v;
                    ptr3++;
                }
            }
        }



        unsafe public matrix zeros()
        {
            int S = this.nRow * this.nCol;
            fixed (double* _ptr = &this.rawData[0, 0])
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

        unsafe public matrix ones()
        {
            int S = this.nRow * this.nCol;
            fixed (double* _ptr = &this.rawData[0, 0])
            {
                double* ptr = _ptr;
                for (int i = 0; i < S; i++)
                {
                    *ptr = 1;
                    ptr++;
                }
            }
            return this;
        }

        public matrix MinusThis()
        {
            return MinusTo(this);
        }

        unsafe public matrix MinusTo(matrix res)
        {
            int S = this.nRow * this.nCol;
            fixed (double* _ptr1 = &this.rawData[0, 0],_ptr2=&res.rawData[0,0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                for (int i = 0; i < S; i++)
                {
                    *ptr2 = -*ptr1;
                    ptr1++;
                    ptr2++;
                }
            }
            return res;
        }
    }

}
