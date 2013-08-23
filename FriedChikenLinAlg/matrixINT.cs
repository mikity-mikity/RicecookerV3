using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace mikity.LinearAlgebra
{
    /// <summary>
    /// Base class of matrices.
    /// </summary>
    /// <typeparam name="T">Type of elements of the matrix</typeparam>
    public class matrixINT:__matrix<int>,IBasicOperations<matrixINT>
    {
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
                    F = F.PadRight(max, ' ');
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
        public matrixINT()
        {
        }
        public matrixINT(int n, int m)
            : base(n, m, 0)
        {
        }
        public matrixINT(int n, int m, int def)
            : base(n, m, def)
        {
        }
        public matrixINT(int[,] m)
            : base(m)
        {
        }
        unsafe public matrixINT ones()
        {
            int S = this.nRow * this.nCol;
            fixed (int* _ptr1 = &this.rawData[0, 0])
            {
                int* ptr1 = _ptr1;
                for (int i = 0; i < S; i++)
                {
                    *ptr1 = 1;
                    ptr1++;

                }
            }
            return this;
        }
        unsafe public matrixINT zeros()
        {
            int S=this.nRow*this.nCol;
            fixed (int* _ptr1 = &this.rawData[0,0])
            {
                int* ptr1 = _ptr1;
                for (int i = 0; i < S; i++)
                {
                    *ptr1 = 0;
                    ptr1++;
                    
                }
            }
            return this;
        }
        public static void zeros(matrix m)
        {
            for (int i = 0; i < m.nRow; i++)
            {
                for (int j = 0; j < m.nCol; j++)
                {
                    m[i, j] = 0;
                }
            }
        }
        public static matrixINT zeros(int nRow, int nCol)
        {
            matrixINT res = new matrixINT(nRow, nCol, 0);
            return res;
        }
        public matrixINT eye()
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
        public static void eye(matrixINT m)
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
        public static matrixINT eye(int nRow, int nCol)
        {
            return (new matrixINT(nRow, nCol, 0).eye());
        }
        public static matrixINT eye(int N)
        {
            return (new matrixINT(N,N).eye());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static implicit operator matrixINT(int[,] m)
        {
            return new matrixINT(m);
        }
        ///Operators
        public static matrixINT operator +(matrixINT m1, matrixINT m2)
        {
            matrixINT res = new matrixINT(m1.nRow, m1.nCol);
            Add(m1, m2, res);
            return res;
        }
        public static matrixINT operator +(matrixINT m, int p)
        {
            matrixINT res = new matrixINT(m.nRow, m.nCol);
            Add(m, p, res);
            return res;
        }
        public static matrixINT operator +(int p, matrixINT m)
        {
            matrixINT res = new matrixINT(m.nRow, m.nCol);
            Add(m, p, res);
            return res;
        }
        public static matrixINT operator *(matrixINT m, int p)
        {
            matrixINT res = new matrixINT(m.nRow, m.nCol);
            Mult(m, p, res);
            return res;
        }
        public static matrixINT operator *(int p,matrixINT m)
        {
            matrixINT res = new matrixINT(m.nRow, m.nCol);
            Mult(m, p, res);
            return res;
        }
        ///Alternatives of Operators
        public static void Add(matrixINT m1, matrixINT m2, matrixINT m3)
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
        public static void Add(matrixINT m1, int p, matrixINT m2)
        {
            if (!isCompatible(m1, m2))
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException("9gk");
            }
            for (int i = 0; i < m2.nRow; i++)
            {
                for (int j = 0; j < m2.nCol; j++)
                {
                    m2[i, j] = m1[i, j] + p;
                }
            }
        }
        public static void Mult(matrixINT m1, int p, matrixINT m2)
        {
            if (!isCompatible(m1, m2))
            {
                throw new mikity.Exceptions.SizeOfMatricesNotCompatibleException("v8idkd");
            }
            for (int i = 0; i < m2.nRow; i++)
            {
                for (int j = 0; j < m2.nCol; j++)
                {
                    m2[i, j] = m1[i, j]*p;
                }
            }
        }

        public vectorINT getRow(int j)
        {
            vectorINT res = new vectorINT(this.nCol);
            for (int i = 0; i < this.nCol; i++)
            {
                res[i] = this[j, i];
            }
            return res;
        }
        unsafe public void Subtract(int m)
        {
            fixed (int* _ptr1 = &this.rawData[0, 0])
            {
                int* ptr1 = _ptr1;
                for (int i = 0; i < this.nRow * this.nCol; i++)
                {
                    *ptr1 -= m;
                    ptr1++;
                }
            }
        }

        unsafe public void Subtract(matrixINT m)
        {
            fixed (int* _ptr1 = &this.rawData[0,0], _ptr2 = &m.rawData[0,0])
            {
                int* ptr1 = _ptr1;
                int* ptr2 = _ptr2;
                for (int i = 0; i < this.nRow*this.nCol; i++)
                {
                    *ptr1 -= *ptr2;
                    ptr1++;
                    ptr2++;
                }
            }
        }



        public matrixINT MinusThis()
        {
            return MinusTo(this);
        }

        unsafe public matrixINT MinusTo(matrixINT res)
        {
            int S = this.nRow * this.nCol;
            fixed (int* _ptr1 = &this.rawData[0, 0],_ptr2=&res.rawData[0,0])
            {
                int* ptr1 = _ptr1;
                int* ptr2 = _ptr2;
                for (int i = 0; i < S; i++)
                {
                    *ptr2 = -*ptr1;
                    ptr1++;
                    ptr2++;

                }
            }
            return this;
        }
    }
    
}
