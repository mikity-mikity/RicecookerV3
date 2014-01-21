using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using mikity.LinearAlgebra;
namespace mikity.NumericalMethodHelper.materials
{
    unsafe public delegate matrix material(elements.element e, integralPoint _i);
    unsafe public delegate vector gravity(elements.element e, integralPoint _i);
    public interface iMaterial
    {
        material getMaterial();
    }
    public interface iGravity
    {
        gravity getGravity();
    }
    public class zeroGravity : iGravity
    {
        private gravity m_gvt;
        unsafe public zeroGravity()
        {
            m_gvt = new gravity((_e, _i) =>
            {
                _i.gravity.zeros();
                return _i.gravity;
            });

        }
        public gravity getGravity()
        {
            return m_gvt;
        }

    }

    public class normalGravity : iGravity
    {
        private gravity m_gvt;
        private double m_Density;
        public double Density
        {
            set
            {
                m_Density = value;
            }
            get
            {
                return m_Density;
            }
        }
        unsafe public normalGravity()
        {
            m_gvt=new gravity((_e, _i) =>
            {
                _i.gravity.zeros();
                double J = Math.Sqrt(_i.metric.det / _i.refMetric.det);                
                _i.gravity[_e.__dim - 1] = -this.Density * 1/J * mikity.NumericalMethodHelper.FriedChiken.G;
                return _i.gravity;
            });

        }
        public gravity getGravity()
        {
            return m_gvt;
        }

    }
    public class formfindGravity : iGravity
    {
        private gravity m_gvt;
        private double m_Density;
        public double Density
        {
            set
            {
                m_Density = value;
            }
            get
            {
                return m_Density;
            }
        }
        unsafe public formfindGravity()
        {
            m_gvt=new gravity((_e, _i) =>
            {
                _i.gravity.zeros();
                _i.gravity[_e.__dim - 1] = -this.Density * mikity.NumericalMethodHelper.FriedChiken.G;
                return _i.gravity;
            });

        }
        public gravity getGravity()
        {
            return m_gvt;
        }

    }
    public class stVenantMaterial : iMaterial
    {
        private material m_mat;
        private double m_Young = 2000;
        private double m_Poisson = 0.2;
        public double Young
        {
            set
            {
                m_Young = value;
            }
            get
            {
                return m_Young;
            }
        }
        public double Poisson
        {
            set
            {
                m_Poisson = value;
            }
            get
            {
                return m_Poisson;
            }
        }
        public double mu
        {
            get
            {
                return Young / (2 * (1 + Poisson));
            }
        }
        public double lambda
        {
            get
            {
                return Young * Poisson / ((1 + Poisson) * (1 - 2 * Poisson));
            }
        }

        unsafe public stVenantMaterial()
        {
            m_mat=new material((_e, _i) =>
            {
                double J = Math.Sqrt(_i.metric.det / _i.refMetric.det);                
                double _m = 1 / J * 2  * this.mu;
                double _l = 1 / J  * this.lambda;
                int N = _i.__N;
                //_i.stress2.zeros();
                /*for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j <N; j++)
                    {
                        _i.stress2[i, j]=0;
                        for (int k = 0; k < N; k++)
                        {
                            for (int l = 0; l < N; l++)
                            {
                                _i.stress2[i, j] = _i.stress2[i, j]+_l * _i.invMetric[i, j] * _i.invMetric[k, l] * _i.strain[k, l] + _m * _i.invMetric[i, k] * _i.strain[k,l] * _i.invMetric[l,j];
                            }
                        }
                    }
                }*/
                fixed (double* _ptr1 = &_i.stress2.rawData[0, 0], _ptr2 = &_i.strain.rawData[0, 0], _ptr3 = &_i.refInvMetric.rawData[0, 0], _ptr4 = &_i.refInvMetric.rawData[0, 0])
                {
                    double* ptr1 = _ptr1;
                    double* ptr3 = _ptr3;
                    double* ptr5 = _ptr3;
                    double* ptr7 = _ptr4;
                    double v = 0;
                    for (int i = 0; i < N; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            v = 0;
                            double* ptr2 = _ptr2;
                            double* ptr4 = _ptr4;
                            double* __ptr7 = ptr7;
                            for (int k = 0; k < N; k++)
                            {
                                for (int l = 0; l < N; l++)
                                {
                                    v += _l * *(ptr3/* + i * N + j*/) * *(ptr4/* + k * N + l*/) * *(ptr2/* + k * N + l*/) + _m * *(ptr7/* + i * N + k*/) * *(ptr2/* + k * N + l*/) * *(ptr5 + l * N + j);
                                    ptr2++;
                                    ptr4++;
                                }
                                ptr7++;
                            }
                            ptr7 = __ptr7;
                            *ptr1 = v;
                            ptr1++;
                            ptr3++;
                        }
                        ptr7 += N;
                    }
                }
                return _i.stress2;
            });

        }
        public material getMaterial()
        {
            return m_mat;
        }
    }
    public class neoHookeanMaterial : iMaterial
    {
        private material m_mat;
        private double m_mu1=1.0,m_K=1.0;
        public double mu1
        {
            get
            {
                return m_mu1;
            }
            set
            {
                m_mu1 = value;
            }
        }
        public double K
        {
            get
            {
                return m_K;
            }
            set
            {
                m_K = value;
            }
        }
        unsafe public neoHookeanMaterial()
        {
            m_mat = new material((_e, _i) =>
            {
                double J = Math.Sqrt(_i.metric.det / _i.refMetric.det);
                int N = _i.__N;
                double K = m_K;
                double u1 = m_mu1;
                double KK = K / 2d * (J - 1d)/**J*/;//Cauchy stress なのでJで割る
                //_i.stress2.zeros();
                double I1 = 0;
                /*for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        I1 += _i.metric[i, j] * _i.refInvMetric[i, j];
                    }
                }

                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        _i.stress2[i, j] = (_i.refInvMetric[i, j] - I1/3d  * _i.invMetric[i, j]) * u1
                            + K  /2d * (J - 1d) * _i.invMetric[i, j];
                    }
                }*/
                fixed (double* _ptr1 = &_i.metric.rawData[0, 0], _ptr2 = &_i.refInvMetric.rawData[0, 0],_ptr3=&_i.invMetric.rawData[0,0],_ptr4=&_i.stress2.rawData[0,0])
                {
                    double* ptr1=_ptr1;
                    double* ptr2=_ptr2;
                    for (int i = 0; i < N * N; i++)
                    {
                        I1 += *ptr1 * *ptr2;
                        ptr1++;
                        ptr2++;
                    }
                    ptr1 = _ptr1;
                    ptr2 = _ptr2;
                    double* ptr3 = _ptr3;
                    double* ptr4 = _ptr4;
                    for (int i = 0; i < N * N; i++)
                    {
                        *ptr4 = (*ptr2 - I1 / 3d * *ptr3) * u1 + KK**ptr3; //全然違うやん！
                        ptr2++;
                        ptr3++;
                        ptr4++;
                    }
                }

                return _i.stress2;
            });

        }
        public material getMaterial()
        {
            return m_mat;
        }
    }
    public class mooneyRivlinMaterial : iMaterial
    {
        private material m_mat;
        private double m_mu1, m_mu2, m_K;
        public double u1
        {
            set
            {
                m_mu1 = value;
            }
            get
            {
                return m_mu1;
            }
        }
        public double u2
        {
            set
            {
                m_mu2 = value;
            }
            get
            {
                return m_mu2;
            }
        }
        public double K
        {
            set
            {
                m_K = value;
            }
            get
            {
                return m_K;
            }
        }
        unsafe public mooneyRivlinMaterial()
        {
            m_mat = new material((_e, _i) =>
            {
                double J = Math.Sqrt(_i.metric.det / _i.refMetric.det);
                int N = _i.__N;
                double K = m_K;
                double u1 = m_mu1;
                double u2 = m_mu2;
                _i.stress2.zeros();
                double I1 = 0;

                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        I1 += _i.metric[i, j] * _i.refInvMetric[i, j];
                    }
                }
                double f = 0;
                for (int k = 0; k < N; k++)
                {
                    for (int l = 0; l < N; l++)
                    {
                        for (int u = 0; u < N; u++)
                        {
                            for (int s = 0; s < N; s++)
                            {
                                f += _i.refInvMetric[k, l] * _i.refInvMetric[u, s] * _i.metric[l, u] * _i.metric[k, s];
                            }
                        }
                    }
                }
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        double t = 0;
                        for (int k = 0; k < N; k++)
                        {
                            for (int l = 0; l < N; l++)
                            {
                                t += _i.refInvMetric[k, i] * _i.refInvMetric[j, l] * _i.metric[k, l];
                            }
                        }
                        _i.stress2[i, j] = (_i.refInvMetric[i, j] - I1 / 3d * _i.invMetric[i, j]) * u1
                            + (I1 * _i.refInvMetric[i, j] - I1 * I1 / 3d * _i.invMetric[i, j] - t + f / 3d * _i.invMetric[i, j]) * u2
                             + K / 2d * (J - 1d) * _i.invMetric[i, j]*J;
                    }
                }
                return _i.stress2;
            });

        }
        public material getMaterial()
        {
            return m_mat;
        }
    }
    public class formFindingMaterial : iMaterial
    {
        private material m_mat;
        private double m_Weight = 1.0;
        private double m_Power = 0.2;
        public double Weight
        {
            set
            {
                m_Weight = value;
            }
            get
            {
                return m_Weight;
            }
        }
        public double Power
        {
            set
            {
                m_Power = value;
            }
            get
            {
                return m_Power;
            }
        }
        unsafe public formFindingMaterial()
        {
            m_mat = new material((_e, _i) =>
            {
                double n = this.Power * this.Weight * Math.Pow(_e.Volume, this.Power - 1);
                _i.energyDensity = this.Weight*Math.Pow(_e.Volume,this.Power-1);
                return _i.stress2.zeros().plus_xA(n, _i.invMetric);
            });

        }
        public material getMaterial()
        {
            return m_mat;
        }
    }

}

