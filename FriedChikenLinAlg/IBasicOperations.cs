using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace mikity.LinearAlgebra
{
    public interface IBasicOperations<T>
    {
        T zeros();
        T ones();
/*
        T CopyFrom(T t);
        T CopyTo(T t);

        T Add(T t, T res);
        T Add(double v,T res);
        T Add(int v, T res);
        T Add(T t);
        T Add(double v);
        T Add(int v);

        T Sub(T t, T res);
        T Sub(double v, T res);
        T Sub(int v, T res);
        T Sub(T t);
        T Sub(double v);
        T Sub(int v);

        T Mult(T t, T res);
        T Mult(double v, T res);
        T Mult(int v, T res);
        T Mult(T t);
        T Mult(double v);
        T Mult(int v);

        T Div(T t, T res);
        T DIv(double v, T res);
        T Div(int v, T res);
        T Div(T t);
        T DIv(double v);
        T Div(int v);
*/
        T MinusThis();
        T MinusTo(T res);
        
    }   

}
