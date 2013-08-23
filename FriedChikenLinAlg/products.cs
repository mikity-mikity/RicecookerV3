using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace mikity.LinearAlgebra
{
    public partial class matrix/* : __matrix<double>, IBasicOperations<matrix>*/
    {
        unsafe public matrix Products(matrix m1,matrix m2)
        {
            double v = 0;
            
            int S=m1.nCol*m2.nRow;
            int K = m1.nCol;
            int n=m1.nRow;
            int m=m2.nCol;
            int r=m2.nCol;

            fixed (double* _ptr1=&m1.rawData[0,0],_ptr2=&m2.rawData[0,0], _ptr3 = &this.rawData[0, 0])
            {
                double* ptr3 = _ptr3;//this
                double* ptr1=_ptr1;//m1
                double* ptr2=_ptr2;//m2
                double* ptr4=_ptr1;//m1
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        v = 0;
                        ptr1 = ptr4;
                        ptr2 = _ptr2 + j;
                        for (int k = 0; k < K; k++)
                        {
                            v += *ptr1 * *ptr2;
                            ptr1++;
                            ptr2 += r;
                        }
                        *ptr3 = v;
                        ptr3++;
                    }
                    ptr4 += K;
                }
            }
  
            return this;
        }

        unsafe public matrix Products(vector v1, vector v2)
        {

/*            for (int i = 0; i < this.nCol; i++)
            {
                for (int j = 0; j < this.nRow; j++)
                {
                    this[i, j] = v1[i] * v2[j];
                }
            }
 * */
            fixed (double* _ptr1 = &this.rawData[0, 0], _ptr2 = &v1.rawData[0], _ptr3 = &v2.rawData[0])
            {
                double* ptr1 = _ptr1;
                double* ptr2 = _ptr2;
                for(int i=0;i<this.nCol;i++)
                {
                    double* ptr3 = _ptr3;
                    for(int j=0;j<this.nRow;j++)
                    {
                        *ptr1 = *ptr2 * *ptr3;
                        ptr1++;
                        ptr3++;
                    }
                    ptr2++;
                }
            }
            return this;
        }


    }
   
}
