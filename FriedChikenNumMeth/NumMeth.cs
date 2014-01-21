using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using mikity.LinearAlgebra;
namespace mikity.NumericalMethodHelper
{
    public static class FriedChiken
    {
        public static void clear()
        {
            numCond = 0;
            particleSystems= new List<particleSystem>();
            __initialParticles = new List<particle>();
            __particles = null;
            __mask=null;
            index=null;
            r=null;
            q = null;
            x = null;
            _r = null;
            _q = null;
            __r = null;
            __q = null;
            __index = new List<int[]>();
            _isInit = false;
            __dof = 0;
            _initializing = false;
            _isInit = false;
        }
        public enum mask
        {
            X, Y, Z
        }
        public static double G=9.8;
        public static List<particleSystem> particleSystems = new List<particleSystem>();
        /// <summary>
        /// Before initialize, particles are added to this.
        /// After initialize particles will be copied to x.
        /// </summary>
        private static List<particle> __initialParticles = new List<particle>();
        private static matrix __particles=null;
        private static List<int[]> __index = new List<int[]>();
        public static vector __mask=null;
        public static matrixINT index=null;
        public static vector r = null, q = null, x = null;
        public static vector _r = null, _q = null;
        public static vector __r = null, __q = null;
        //public static matrix 
        public static vector gradient;
        public static vector load;
        public static vector omega;
        public static vector reaction;
        private static vector residual=null;
        private static matrix jacobian=null;
        public static int numCond=0;
        //埋め込まれたユークリッド空間の次元。通常は３。
        private static int __dim = 3;
        private static bool _isInit = false;
        private static int __dof = 0;
        private static bool _initializing = false;
        public static double energy;
        public static double getEnergy()
        {
            return energy;
        }
        public static matrix getParticles()
        {
            return __particles;
        }
        public static matrix getJacobian()
        {
            return jacobian;
        }
        public static vector getResidual()
        {
            return residual;
        }
        public static vector getGradient()
        {
            return gradient;
        }
        public static vector getLoad()
        {
            return load;
        }
        public static vector getOmega()
        {
            return omega;
        }
        public static vector getReaction()
        {
            return reaction;
        }
        public static int nParticles
        {
            private set;
            get;
        }
        public static int DOF
        {
            private set
            {
                __dof = value;
            }
            get
            {
                return __dof;
            }
        }
        public static bool isInitialized
        {
            get
            {
                return _isInit;
            }
        }
        public static bool isInitilizing
        {
            get
            {
                return _initializing;
            }
        }
        public static int dimension
        {
            set
            {
                __dim = value;
            }
            get
            {
                if (__dim == 0)
                {
                    throw new mikity.Exceptions.DimensionOfEucledianSpaceNotDetermined();
                }
                else
                {
                    return __dim;
                }
            }
        }
        public static double tolerance
        {
            set;
            get;
        }
        private static int m_subDiv = 2;
        public static int subDivision
        {
            set
            {
                if (value < 2)
                {
                    m_subDiv = 2;
                }
                else
                {
                    m_subDiv = value;
                }
            }
            get
            {
                return m_subDiv;
            }
        }
        public static void setConstants(int dim=3,double tor=0.000001,int subDiv=2)
        {
            dimension = dim;
            tolerance = tor;
            subDivision = subDiv;
            vector.tolerance = tor;
        }
        public static void begin()
        {
            _initializing = true;
            //色々初期化
            nParticles = __initialParticles.Count;
            DOF = dimension * nParticles;
            __particles = new matrix(nParticles,FriedChiken.dimension).zeros();
            index=new matrixINT(nParticles,dimension);
            for (int i = 0; i < nParticles; i++)
            {
                for (int k = 0; k < dimension; k++)
                {
                    __particles[i, k] = __initialParticles[i][k]/* +(r.NextDouble() - 0.5) * 0.0001*/;
                    index[i, k] = __index[i][k];
                }
            }
            //initializing masking vector
            __mask = new vector(DOF).zeros();
            numCond = 0;
            foreach (particleSystem p in particleSystems)
            {
                p.copy(FriedChiken.__particles);
                p.begin();
            }
            if (fixX)
            {
                for (int i = 0; i < nParticles; i++)
                {
                    __mask[index[i, 0]] = 1;
                }
            }
            if (fixY)
            {
                for (int i = 0; i < nParticles; i++)
                {
                    __mask[index[i, 1]] = 1;
                }
            }
            if (fixZ)
            {
                for (int i = 0; i < nParticles; i++)
                {
                    __mask[index[i, 2]] = 1;
                }
            }
            if (numCond > 0)
            {
                FriedChiken.residual = new vector(numCond).zeros();
                FriedChiken.jacobian = new matrix(numCond, DOF).zeros();

            }
            else
            {
                FriedChiken.residual = null;
                FriedChiken.jacobian = null;
            }
            FriedChiken.gradient = new vector(DOF).zeros();
            FriedChiken.load = new vector(DOF).zeros();
            FriedChiken.omega = new vector(DOF).zeros();
            FriedChiken.reaction = new vector(DOF).zeros();
            FriedChiken.x = new vector(DOF).zeros();
            __particles.DecomposeTo(FriedChiken.x);
            FriedChiken.q = new vector(DOF).zeros();
            FriedChiken.r = new vector(DOF).zeros();
            FriedChiken._q = new vector(DOF).zeros();
            FriedChiken._r = new vector(DOF).zeros();
            FriedChiken.__q = new vector(DOF).zeros();
            FriedChiken.__r = new vector(DOF).zeros();
            _initializing = false;
            _isInit = true;

            Random r = new Random(0);
            for (int i = 0; i < nParticles; i++)
            {
                for (int k = 0; k < dimension; k++)
                {
                    __particles[i, k] = __particles[i, k] +(r.NextDouble() - 0.5) * 0.001;
                }
            }
            __particles.DecomposeTo(FriedChiken.x);

        }
        public static void Tick(int count)
        {
            FriedChiken.gradient.zeros();
            FriedChiken.energy = 0;
            FriedChiken.load.zeros();
            FriedChiken.omega.zeros();
            FriedChiken.reaction.zeros();
            if (numCond > 0)
            {
                FriedChiken.jacobian.zeros();
                FriedChiken.residual.zeros();
            }
            FriedChiken.x.Decompose(__particles);
            foreach (particleSystem p in particleSystems)
            {
                p.copy(FriedChiken.__particles);
                p.Update();
            }
        }
        public static void Tack(int count)
        {
//            FriedChiken.gradient.mask(FriedChiken.getMask());
            omega.Subtract(gradient ,load);
            reaction.dotMult(omega, __mask);
            omega.Subtract(reaction);
            if (numCond > 0)
            {

                jacobian.mask(__mask);
            }
        }
        /// <summary>
        /// Register particleSystems to FriedChiken.
        /// If the number of particle systems is greater than 1, the particles will be merged.
        /// Particle systems added different times will not be merged.
        /// </summary>
        /// <param name="pSL"></param>
        public static void addParticleSystems(params particleSystem[] pSL)
        {
            int c = pSL.Length;
            if (c == 1)
            {
                FriedChiken.particleSystems.Add(pSL[0]);
                for (int i = 0; i < pSL[0].__N; i++)
                {
                    int S = __initialParticles.Count;
                    __initialParticles.Add(new particle(pSL[0][i]));
                    int[] a = new int[dimension];
                    for (int j = 0; j < dimension; j++)
                    {
                        a[j] = S * 3 + j;
                    }
                    __index.Add(a);
                    pSL[0].globalIndex[i] = __initialParticles.Count - 1;
                }
            }
            else
            {
                FriedChiken.particleSystems.AddRange(pSL);
                for (int k = 0; k < pSL.Length; k++)
                {
                    for (int i = 0; i < pSL[k].__N; i++)
                    {
                        //Merge
                        //If a particle exists that have the same position to the current particle, 
                        //just index is transfered. Otherwise, new particle will be created.
                        int T = -1;
                        for (int s = 0; s < k; s++)
                        {
                            for (int u = 0; u < pSL[s].__N; u++)
                            {
                                if(vector.isSame(pSL[k][i],pSL[s][u]))
                                {
                                    T = pSL[s].globalIndex[u];
                                }
                                if (T != -1) break;
                            }
                            if (T != -1) break;
                        }
                        if (T == -1)
                        {
                            int S = __initialParticles.Count;
                            __initialParticles.Add(new particle(pSL[k][i]));
                            int[] a = new int[dimension];
                            for (int j = 0; j < dimension; j++)
                            {
                                a[j] = S * 3 + j;
                            }
                            __index.Add(a);
                            pSL[k].globalIndex[i] = __initialParticles.Count - 1;
                        }
                        else
                        {
                            pSL[k].globalIndex[i] = T;
                        }
                    }
                }
            }
        }


        public static void maskComponent(params mask[] _mask)
        {
            foreach (mask e in _mask)
            {
                if (e == mask.X)
                {
                    fixX = true;
                }
                if (e == mask.Y)
                {
                    fixY = true;
                }
                if (e == mask.Z)
                {
                    fixZ = true;
                }
            }
        }
        public static bool fixX
        {
            private set;
            get;
        }
        public static bool fixY
        {
            private set;
            get;
        }
        public static bool fixZ
        {
            private set;
            get;
        }

        public static vector getMask()
        {
            return __mask;
        }

        internal static int requestConditionNumber()
        {
            numCond++;
            return numCond-1;

        }
    }



    public class point : vector
    {
        public int __dim;
        public point()
            : base(FriedChiken.dimension)
        {
            __dim = FriedChiken.dimension;
        }
    }
    public class particle : point
    {
        public particle(params double[] p)
            : base()
        {
            if (p.Length == __dim)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    this.rawData[i] = p[i];
                }
            }
            else
            {
                throw new mikity.Exceptions.SizeOfVectorsNotCompatibleException();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class drawPoint : point
    {
        public drawPoint()
            : base()
        {
        }
    }


}
