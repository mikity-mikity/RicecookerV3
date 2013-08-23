using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace mikity.LinearAlgebra
{
    public class vectorINT:__vector<int>,IBasicOperations<vectorINT>
    {
        private static vectorINT V3 = new vectorINT(3, 0);
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

        public vectorINT()
            : base()
        {
        }
        public vectorINT(int N)
            : base(N,0)
        {
        }
        public vectorINT(int N, int def)
            : base(N, def)
        {
        }
        public vectorINT(int[] p)
            : base(p)
        {
        }
        public void copyFrom(params int[] p)
        {
            if (p.Length != this.nElem)
            {
                throw new mikity.Exceptions.SizeOfVectorsNotCompatibleException();
            }
            else
            {
                for (int i = 0; i < this.nElem; i++)
                {
                    this.rawData[i] = p[i];
                }
            }
        }
        public static vectorINT zeros(int N)
        {
            return new vectorINT(N).zeros();
        }
        public static vectorINT ones(int N)
        {
            return new vectorINT(N).ones();
        }
        public double norm
        {
            get
            {
                return Math.Sqrt(vectorINT.dot(this, this));
            }
        }
        public vectorINT Add(vectorINT v)
        {
            for(int i=0;i<this.nElem;i++)
            {
                this[i]+=v[i];
            }
            return this;
        }
        unsafe public vectorINT Add(vectorINT v,int p)
        {
//            for(int i=0;i<this.nElem;i++)
//            {
//                this[i]+=v[i]*p;
//            }
            int S = nElem;
            fixed (int* _ptr1 = &this.rawData[0], _ptr2 = &v.rawData[0])
            {
                int* ptr1 = _ptr1;
                int* ptr2 = _ptr2;
                for (int i = 0; i < S; i++)
                {
                    *ptr1 += *ptr2 * p;
                    ptr1++;
                    ptr2++;
                }
            }
            return this;
        }
        unsafe public static int dot(vectorINT v1, vectorINT v2)
        {
            int res = 0;
            //if (!isCompatible(v1, v2))
            //{
            //    throw new mikity.Exceptions.SizeOfVectorsNotCompatibleException();
            //}
            //for (int i = 0; i < v1.nElem; i++)
            //{
            //    res += v1[i] * v2[i];
            //}
            int S = v1.nElem;
            fixed (int* _ptr1 = &v1.rawData[0], _ptr2 = &v2.rawData[0])
            {
                int* ptr1 = _ptr1;
                int* ptr2 = _ptr2;
                for (int i = 0; i < S; i++)
                {
                    res += *ptr1 * *ptr2;
                    ptr1++;
                    ptr2++;
                }
            }
            return res;
        }
        public void mult(int v)
        {
            for (int i = 0; i < nElem; i++)
            {
                this[i] *= v;
            }
        }
        public void add(int v)
        {
            for (int i = 0; i < nElem; i++)
            {
                this[i] += v;
            }
        }
        unsafe public void Subtract(int v)
        {
            fixed (int* _ptr1 = &this.rawData[0])
            {
                int* ptr1 = _ptr1;
                for (int i = 0; i < this.nElem; i++)
                {
                    *ptr1 -= v;
                    ptr1++;
                }
            }
        }
        unsafe public void Subtract(vectorINT v)
        {
            fixed (int* _ptr1 = &this.rawData[0], _ptr2 = &v.rawData[0])
            {
                int* ptr1 = _ptr1;
                int* ptr2 = _ptr2;
                for (int i = 0; i < this.nElem; i++)
                {
                    *ptr1 -= *ptr2;
                    ptr1++;
                    ptr2++;
                }
            }
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
        public vectorINT crossProduct(vectorINT v1,vectorINT v2)
        {
            if (!isCompatible(v1, v2, this,vectorINT.V3))
            {
                throw new mikity.Exceptions.SizeOfVectorsNotCompatibleException();
            }
            this[0] = v1[1] * v2[2] - v1[2] * v2[1];
            this[1] = v1[2] * v2[0] - v1[0] * v2[2];
            this[2] = v1[0] * v2[1] - v1[1] * v2[0];
            return this;
        }
        public static vectorINT crossProduct(vectorINT v1, vectorINT v2,vectorINT v3)
        {
            if (!isCompatible(v1, v2, v3,vectorINT.V3))
            {
                throw new mikity.Exceptions.SizeOfVectorsNotCompatibleException();
            }
            v3[0] = v1[1] * v2[2] - v1[2] * v2[1];
            v3[1] = v1[2] * v2[0] - v1[0] * v2[2];
            v3[2] = v1[0] * v2[1] - v1[1] * v2[0];
            return v3;
        }

        public static vectorINT range(int start, int end, int step)
        {
            vectorINT res = new vectorINT((int)(end - start) / step + 1);
            for (int i = start, j = 0; i <= end; j++, i += step)
            {
                res[j] = i;
            }
            return res;
        }
        public static vectorINT range(int start, int end)
        {
            vectorINT res = new vectorINT((end - start)  + 1);
            for (int i = start, j = 0; i <= end; j++, i ++)
            {
                res[j] = i;
            }
            return res;
        }
        public static implicit operator vectorINT(int[] v)
        {
            return new vectorINT(v);
        }


        public static bool compare(vectorINT first, vectorINT second)
        {
            List<int> a = new List<int>(first.rawData);
            List<int> b = new List<int>(second.rawData);
            a.Sort();
            b.Sort();
            for (int i = 0; i < first.nElem; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        unsafe public vectorINT zeros()
        {
            //double[] p = this.rowData;
            //for (int i = 0; i < this.nElem; i++)
            //{
            //    p[i] = 0;
            //}
            int S = this.nElem;
            fixed (int* _ptr = &this.rawData[0])
            {
                int* ptr = _ptr;
                for (int i = 0; i < S; i++)
                {
                    *ptr = 0;
                    ptr++;
                }
            }
            return this;
        }
        unsafe public vectorINT ones()
        {
            //double[] p = this.rowData;
            //for (int i = 0; i < this.nElem; i++)
            //{
            //    p[i] = 0;
            //}
            int S = this.nElem;
            fixed (int* _ptr = &this.rawData[0])
            {
                int* ptr = _ptr;
                for (int i = 0; i < S; i++)
                {
                    *ptr = 1;
                    ptr++;
                }
            }
            return this;
        }

        public vectorINT MinusThis()
        {
            return MinusTo(this);
        }

        unsafe public vectorINT MinusTo(vectorINT res)
        {
            fixed (int* _ptr1 = &this.rawData[0], _ptr2 = &res.rawData[0])
            {
                int* ptr1 = _ptr1;
                int * ptr2 = _ptr2;
                for (int i = 0; i < nElem; i++)
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
