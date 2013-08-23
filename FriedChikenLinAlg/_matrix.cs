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
    public class __matrix<T>
    {
        protected T[,] _data;
        public __matrix()
        {
            _data = new T[0, 0];
        }
        public __matrix(int n, int m)
        {
            if (n < 1) n = 1;
            if (m < 1) m = 1;
            _data = new T[n, m];
        }
        public __matrix(int n, int m,T def)
        {
            if (n < 1) n = 1;
            if (m < 1) m = 1;
            _data = new T[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    _data[i, j] = def;
                }
            }
        }
        public __matrix(T[,] m)
        {
            this._data = m;
        }
        virtual public __matrix<T> copyFrom(__matrix<T> t)
        {
            if (isCompatible(this, t))
            {
                for (int i = 0; i < this.nRow; i++)
                {
                    for (int j = 0; j < this.nCol; j++)
                    {
                        this._data[i, j] = t[i, j];
                    }
                }
            }
            return this;
        }
        virtual public __matrix<T> copyTo(__matrix<T> t)
        {
            if (isCompatible(this, t))
            {
                for (int i = 0; i < this.nRow; i++)
                {
                    for (int j = 0; j < this.nCol; j++)
                    {
                        this._data[i, j] = t[i, j];
                    }
                }
            }
            return t;
        }
        public int nRow
        {
            get
            {
                return this._data.GetLength(0);
            }
        }
        public int nCol
        {
            get
            {
                return this._data.GetLength(1);
            }
        }
        /// <summary>
        /// Return containing array.
        /// Just the handle is returned.
        /// </summary>
        /// <param name="m">A matrix containing data array.</param>
        /// <returns>Containing data array.</returns>
        public T[,] rawData
        {
            get
            {
                return this._data;
            }
        }
        public T this[int i,int j]
        {
            get
            {
                return _data[i, j];
            }
            set
            {
                _data[i, j] = value;
            }
        }
        /// <summary>
        /// Return containing array.
        /// Just the handle is returned.
        /// </summary>
        /// <param name="m">A matrix containing data array.</param>
        /// <returns>Containing data array.</returns>
        public static implicit operator T[,](__matrix<T> m)
        {
            return m._data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static implicit operator __matrix<T>(T[,] m)
        {
            return new __matrix<T>(m);
        }
        public static bool isCompatible(params __matrix<T>[] ms)
        {
            if (ms.Length <= 1) return true;
            for (int i = 1; i < ms.Length; i++)
            {
                if (ms[i].nCol != ms[0].nCol) return false;
                if (ms[i].nRow != ms[0].nRow) return false;
            }
            return true;
        }
    }


}
