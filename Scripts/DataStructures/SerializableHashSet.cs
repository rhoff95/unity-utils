#if NET_4_6 || NET_STANDARD_2_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Plugins.unity_utils.Scripts.DataStructures
{
    public abstract class SerializableHashSetBase
    {
        public abstract class Storage
        {
        }

        protected class HashSet<TValue> : System.Collections.Generic.HashSet<TValue>
        {
            public HashSet()
            {
            }

            public HashSet(IEnumerable<TValue> set) : base(set)
            {
            }

            public HashSet(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }

    [Serializable]
    public abstract class SerializableHashSet<T> : SerializableHashSetBase, ISet<T>,
        ISerializationCallbackReceiver, IDeserializationCallback, ISerializable
    {
        private HashSet<T> _mHashSet;
        [SerializeField] private T[] mKeys;

        protected SerializableHashSet()
        {
            _mHashSet = new HashSet<T>();
        }

        protected SerializableHashSet(ISet<T> set)
        {
            _mHashSet = new HashSet<T>(set);
        }

        public void CopyFrom(ISet<T> set)
        {
            _mHashSet.Clear();
            foreach (var value in set)
            {
                _mHashSet.Add(value);
            }
        }

        public void OnAfterDeserialize()
        {
            if (mKeys != null)
            {
                _mHashSet.Clear();
                var n = mKeys.Length;
                for (var i = 0; i < n; ++i)
                {
                    _mHashSet.Add(mKeys[i]);
                }

                mKeys = null;
            }
        }

        public void OnBeforeSerialize()
        {
            var n = _mHashSet.Count;
            mKeys = new T[n];

            var i = 0;
            foreach (var value in _mHashSet)
            {
                mKeys[i] = value;
                ++i;
            }
        }

        #region ISet<TValue>

        public int Count => ((ISet<T>)_mHashSet).Count;
        public bool IsReadOnly => ((ISet<T>)_mHashSet).IsReadOnly;

        public bool Add(T item)
        {
            return ((ISet<T>)_mHashSet).Add(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            ((ISet<T>)_mHashSet).ExceptWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            ((ISet<T>)_mHashSet).IntersectWith(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return ((ISet<T>)_mHashSet).IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return ((ISet<T>)_mHashSet).IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return ((ISet<T>)_mHashSet).IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return ((ISet<T>)_mHashSet).IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return ((ISet<T>)_mHashSet).Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return ((ISet<T>)_mHashSet).SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            ((ISet<T>)_mHashSet).SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            ((ISet<T>)_mHashSet).UnionWith(other);
        }

        void ICollection<T>.Add(T item)
        {
            ((ISet<T>)_mHashSet).Add(item);
        }

        public void Clear()
        {
            ((ISet<T>)_mHashSet).Clear();
        }

        public bool Contains(T item)
        {
            return ((ISet<T>)_mHashSet).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ISet<T>)_mHashSet).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return ((ISet<T>)_mHashSet).Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((ISet<T>)_mHashSet).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ISet<T>)_mHashSet).GetEnumerator();
        }

        #endregion

        #region IDeserializationCallback

        public void OnDeserialization(object sender)
        {
            ((IDeserializationCallback)_mHashSet).OnDeserialization(sender);
        }

        #endregion

        #region ISerializable

        protected SerializableHashSet(SerializationInfo info, StreamingContext context)
        {
            _mHashSet = new HashSet<T>(info, context);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ((ISerializable)_mHashSet).GetObjectData(info, context);
        }

        #endregion
    }
#endif
}
