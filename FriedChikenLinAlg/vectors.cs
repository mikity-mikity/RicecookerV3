using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace mikity.LinearAlgebra
{
    public class vectors : __vector<vector>
    {
        public vectors(int n)
            : base(n)
        {
        }
    }
    public class vectorsINT : __vector<vectorINT>
    {
        public vectorsINT(int n)
            : base(n)
        {
        }
    }
}