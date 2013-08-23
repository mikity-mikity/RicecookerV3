using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace mikity.LinearAlgebra
{
    public class __vector<T>
    {
        private T[] _data;
        public __vector()
        {
            _data = new T[0];
        }
        public __vector(int n)
        {
            _data = new T[n];
        }
        public __vector(int n,T def)
        {
            _data = new T[n];
            for (int i = 0; i < n; i++)
            {
                 _data[i] = def;
            }
        }
        public __vector(T[] v)
        {
            this._data = v;
        }
        public int nElem
        {
            get
            {
                return this._data.GetLength(0);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        public void Update(__vector<T> t)
        {
            if (isCompatible(this, t))
            {
                for (int i = 0; i < this.nElem; i++)
                {
                    this._data[i] = t[i];
                }
            }
        }

        /// <summary>
        /// Return containing array.
        /// Just the handle is returned.
        /// </summary>
        /// <param name="m">A matrix containing data array.</param>
        /// <returns>Containing data array.</returns>
        public T[] rawData
        {
            get
            {
                return this._data;
            }
        }
        public T this[int i]
        {
            get
            {
                return _data[i];
            }
            set
            {
                _data[i] = value;
            }
        }
        /// <summary>
        /// Return containing array.
        /// Just the handle is returned.
        /// </summary>
        /// <param name="m">A matrix containing data array.</param>
        /// <returns>Containing data array.</returns>
        public static implicit operator T[](__vector<T> v)
        {
            return v._data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static implicit operator __vector<T>(T[] v)
        {
            return new __vector<T>(v);
        }
        public static bool isCompatible(params __vector<T>[] vs)
        {
            if (vs.Length <= 1) return true;
            for (int i = 1; i < vs.Length; i++)
            {
                if (vs[i].nElem != vs[0].nElem) return false;
            }
            return true;
        }

    }
   
}
