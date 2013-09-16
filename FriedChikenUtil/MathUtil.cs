using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using mikity.LinearAlgebra;
namespace mikity
{
    public class MathUtil
    {
        

        public static int __pow_INT_INT(int a, int b)
        {
            int res = 1;
            for (int i = 0; i < b; i++)
            {
                res *= a;
            }
            return res;
        }
        private static void duplicate(int[,] M, int n, int[] baseNums)
        {
            if (n >= M.GetLength(1))
                return;
            int t = factorial(baseNums,n);

            int u = t * baseNums[n];
            int s = M.GetLength(1);
            for (int i = 0; i < baseNums[n]; i++)
            {
                for (int j = 0; j < t; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        M[j + i * t, k] = M[j, k];
                    }
                    M[j + i * t, n] = i;
                }
            }
            duplicate(M, n + 1, baseNums);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="N"></param>
        /// <param name="baseNum"></param>
        /// <returns>
        /// When N=3,baseNum=2, then returns
        /// new int[2^3,3]{{0,0,0},{1,0,0},{0,1,0},{1,1,0},{0,0,1},{1,0,1},{0,1,1},{1,1,1}};
        /// </returns>
        public static int[,] generate(int N, int[] baseNums)
        {
            int n = factorial(baseNums);
            int[,] _psi = matrixINT.zeros(n, N);
            duplicate(_psi, 0, baseNums);
            return _psi;
        }
        public static int[,] generate(int N, int baseNum)
        {
            int[] baseNums = new int[N];
            for (int j = 0; j < N; j++)
            {
                baseNums[j] = baseNum;
            }
            return generate(N,baseNums);
        }
        public static int[,] generate(int N, List<int> baseNums)
        {
            return generate(N, baseNums.ToArray());
        }
        public static matrixINT[] psi = new matrixINT[3] { generate(1, 2), generate(2, 2), generate(3, 2) };
        public static matrixINT[] phi = new matrixINT[3] { -2 * psi[0] + 1, -2 * psi[1] + 1, -2 * psi[2] + 1 };
        public static double[,] bicubic(int _dim,int[] nEdgeNodes)
        {
            int _nNodes = __pow_INT_INT(2, _dim);
            //点群生成用数列生成
            matrixINT psi2 = mikity.MathUtil.generate(_dim, nEdgeNodes);
            int nNewNodes = psi2.nRow;
            //要素内座標生成元
            double[][] __u = new double[_dim][];
            for (int i = 0; i < _dim; i++)
            {
                __u[i] = new double[nEdgeNodes[i]];
            }
            //等分
            for (int j = 0; j < _dim; j++)
            {
                for (int i = 0; i < __u[j].Length; i++)
                {
                    __u[j][i] = i * (1.0d / (nEdgeNodes[j] - 1));
                }
            }
            //要素内座標
            double[,] co = new double[nNewNodes, _dim];
            for (int i = 0; i < nNewNodes; i++)
            {
                for (int j = 0; j < _dim; j++)
                {
                    co[i, j] = __u[j][psi2[i, j]];
                }
            }
            //位置ベクトルに付加する重み
            double[,] wt = matrix.zeros(nNewNodes, _nNodes);
            matrixINT phi = MathUtil.phi[_dim-1];
            matrixINT psi = MathUtil.psi[_dim-1];
            for (int i = 0; i < nNewNodes; i++)
            {
                for (int j = 0; j < _nNodes; j++)
                {
                    wt[i, j] = 1.0;
                    for (int k = 0; k < _dim; k++)
                    {
                        wt[i, j] = wt[i, j] * (co[i, k] * phi[j, k] + psi[j, k]);
                    }
                }
            }
            return wt;
        }
        /// <summary>
        /// aからbまでの総乗
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int factorial(int a, int b)
        {
            int res = 1;
            for (int i = a; i <= b; i++)
            {
                res *= i;
            }
            return res;
        }
        /// <summary>
        /// S内の総ての実数の総積
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static double factorial(double[] S)
        {
            double res = 1;
            for (int i = 0; i < S.Length; i++)
            {
                res *= S[i];
            }
            return res;
        }
        public static int factorial(int[] S)
        {
            int res = 1;
            for (int i = 0; i < S.Length; i++)
            {
                res *= S[i];
            }
            return res;
        }
        /// <summary>
        /// 添字0からnまでの総乗
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static double factorial(double[] S,int n)
        {
            double res = 1;
            for (int i = 0; i < n; i++)
            {
                res *= S[i];
            }
            return res;
        }
        public static int factorial(int[] S,int n)
        {
            int res = 1;
            for (int i = 0; i < n; i++)
            {
                res *= S[i];
            }
            return res;
        }
        public static int[][] isoparametricElements(int[] nEdgeNodes)
        {
            int _dim = nEdgeNodes.Length;
            int[] nEdgeElements = new int[_dim];
            for (int i = 0; i < _dim; i++)
            {
                nEdgeElements[i] = nEdgeNodes[i] - 1;
            }
            //要素数
            int nElements = MathUtil.factorial(nEdgeElements);
            int _nNodes=__pow_INT_INT(2,_dim);
            int[][] el = new int[nElements][];
            for (int i = 0; i < nElements; i++)
            {
                el[i] = new int[_nNodes];
            }
            //要素生成
            {
                //要素生成用数列（オフセット値）
                matrixINT psi3 = mikity.MathUtil.generate(_dim, nEdgeElements);
                //もととなる要素
                matrixINT psi4 = mikity.MathUtil.psi[_dim - 1];
                int[] ss = new int[_dim];
                ss[0] = 1;
                for (int k = 1; k < _dim; k++)
                {
                    ss[k] = ss[k - 1] * nEdgeNodes[k - 1];
                }
                for (int i = 0; i < nElements; i++)
                {
                    int s = 0;
                    for (int k = 0; k < _dim; k++)
                    {
                        s += psi3[i, k] * ss[k];
                    }
                    for (int j = 0; j < _nNodes; j++)
                    {
                        int tt = s;
                        for (int k = 0; k < _dim; k++)
                        {
                            tt += psi4[j, k] * ss[k];
                        }
                        el[i][j] = tt;
                    }
                }
            }
            return el;
        }
    }

}
