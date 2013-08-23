using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace mikity.LinearAlgebra
{
    public class matrixVector : __vector<matrix>
    {
        public matrixVector(int nRow, int nCol, int num):base(num)
        {
            for (int i = 0; i < num; i++)
            {
                this[i] = new matrix(nRow, nCol).eye();
            }
        }
    }
}
