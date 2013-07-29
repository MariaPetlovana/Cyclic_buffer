using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Cyclic_buffer
{
    public class Ring_buffer<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        protected struct MyExceptions
        {
            public const string CapacityLessThanZero = "Capacity is less than zero";
            public const string SizeLessThanZero = "Size is less than zero";
            public const string CapacityLessThanSize = "Capacity is less than size";
            public const string SizeIsNull = "Buffer is empty";
            public const string SizeEqualCapacity = "Size is equal to capacity, can't overwrite buffer";
            public const string CopyToArrayIsNull = "CopyTo array is null";
            public const string CopyToArrayIndexIsLessThanNull = "CopyTo array index is less than null";
            public const string CopyToArrayIndexIsMoreThanLength = "CopyTo array index is more than length of the CopyTo array";
            public const string InsertArrayIndexIsLessThanNull = "Insert array index is less than null";
            public const string ElementAtIndexIsLessThanNull = "ElementAt index is less than null";
            public const string ElementAtIndexIsMoreThanSize = "ElementAt index is more than size";
            public const string CountCopyToMoreThanSize = "Count of elements copy to is more than size";
            public const string NotEnoughPlaceCopyTo = "Not enough place copy to";
            public const string ValueLessThanCapacity = "New value is less than capacity";
        }

        protected T[] m_buffer;
        protected int m_capacity;
        protected int m_size;
        protected int m_head;
        protected int m_tail;
        protected bool m_allowOverWrite;
        protected object m_SyncRoot;

/****** Ring_buffer<T> constructors *****************************************************************************************/
                
        public Ring_buffer(int _capacity, bool CanOverWrite)
        {
            if (_capacity < 0)
            {
                throw new ArgumentException(MyExceptions.CapacityLessThanZero, "_capacity");
            }

            m_capacity = _capacity;
            m_size = 0;
            m_head = 0;
            m_tail = 0;
            AllowOverWrite = CanOverWrite;
            m_buffer = new T[m_capacity];
        }

        public Ring_buffer(int _capacity)
            :this(_capacity, true)
        {            
        }

        public Ring_buffer(int _size, T item, bool CanOverWrite)
        {
            if (_size < 0)
            {
                throw new ArgumentException(MyExceptions.CapacityLessThanZero, "_size");
            }

            m_capacity = _size;
            m_size = _size;
            m_head = 0;
            m_tail = _size;
            AllowOverWrite = CanOverWrite;
            m_buffer = new T[m_capacity];
            for (int i = 0; i < m_capacity; ++i)
            {
                m_buffer[i] = item;
            }
        }

        public Ring_buffer(int _size, T item)
            :this(_size, item, true)
        {            
        }

        public Ring_buffer(int _capacity, int _size, T item, bool CanOverWrite)
        {
            if (_capacity < 0)
            {
                throw new ArgumentException(MyExceptions.CapacityLessThanZero, "_capacity");
            }

            m_capacity = _capacity;

            if (_size < 0)
            {
                throw new ArgumentException(MyExceptions.SizeLessThanZero, "_size");
            }

            if(_size > m_capacity)
            {
                throw new ArgumentOutOfRangeException("m_capacity", "m_size", MyExceptions.CapacityLessThanSize);
            }

            m_head = 0;
            m_tail = _size;
            AllowOverWrite = CanOverWrite;
            m_buffer = new T[m_capacity];
            for(int i = 0; i < m_size; ++i)
            {
                m_buffer[i] = item;
            }
        }

        public Ring_buffer(int _capacity, int _size, T item)
            :this(_capacity, _size, item, true)
        {
        }

/****** Ring_buffer<T> Copy Constructors ************************************************************************************/
                
        public Ring_buffer(Ring_buffer<T> b, bool CanOverWrite)
        {
            m_capacity = b.Capacity;
            m_size = b.Size;
            m_head = b.Head;
            m_tail = b.Tail;
            AllowOverWrite = CanOverWrite;

            m_buffer = new T[m_capacity];
            for (int i = 0, j = m_head; i < m_size; ++i, ++j)
            {
                if (m_capacity == j)
                {
                    j = 0;
                }

                m_buffer[j] = b.m_buffer[j];
            }
        }

        public Ring_buffer(Ring_buffer<T> b)
            :this(b, true)
        {
        }

/****** Ring_buffer<T> Destructor *******************************************************************************************/
                
        ~Ring_buffer()
        {
            Clear();
            m_buffer = null;
        }

/****** Ring_buffer<T> Properties *******************************************************************************************/
                
        public int Capacity
        {
            get
            {
                return m_capacity;
            }

            set
            {
                if (value == m_capacity)
                {
                    return;
                }

                if (value < m_capacity)
                {
                    throw new ArgumentOutOfRangeException("value", MyExceptions.ValueLessThanCapacity);
                }

                T[] TemporaryArray = new T[value];
                CopyTo(TemporaryArray);
                m_capacity = value;
                m_buffer = TemporaryArray;
            }
        }

        public int Size
        {
            get
            {
                return m_size;
            }
        }

        public int Reserve
        {
            get
            {
                return m_capacity - m_size;
            }
        }

        public int Head
        {
            get
            {
                return m_head;
            }
        }

        public int Tail
        {
            get
            {
                return m_tail;
            }
        }

        public bool AllowOverWrite
        {
            get
            {
                return m_allowOverWrite;
            }

            set
            {
                m_allowOverWrite = value;
            }
        }

/****** Ring_buffer<T> Methods **********************************************************************************************/

        /*
         *  Checks if the buffer is empty.
         */
        public bool IsEmpty()
        {
            return (0 == m_size) ? true : false;
        }

        /*
         *  Checks is the buffer is full.
         */
        public bool IsFull()
        {
            return (m_size == m_capacity) ? true : false;
        }

        /*
         *  Checks if the buffer consists of 1 part.
         */
        public bool IsLinearized()
        {
            return (m_head < m_tail) ? true : false;
        }

        /*
         *  Returns an element at the index position.
         */
        public T ElementAt(int index)
        {
            if (0 == m_size)
            {
                throw new ArgumentNullException("index", MyExceptions.SizeIsNull);
            }

            if (0 > index)
            {
                throw new ArgumentOutOfRangeException("index", MyExceptions.ElementAtIndexIsLessThanNull);
            }

            if (index >= m_size)
            {
                throw new ArgumentOutOfRangeException("index", MyExceptions.ElementAtIndexIsMoreThanSize);
            }

            return m_buffer[index];
        }
                
        /*
         *  If the new size is greater than the current size, copies of item will be inserted at the back 
         * of the buffer in order to achieve the desired size.
         *  If the current number of elements stored in the buffer is greater than the desired new size then 
         * number of m_size - NewSize last elements will be removed. (The capacity will remain unchanged.)
         */
        public void ReSize(int NewSize, T item)
        {
            if(NewSize > m_capacity)
            {
                throw new ArgumentOutOfRangeException("capacity", "NewSize", MyExceptions.CapacityLessThanSize);
            }

            if(0 > NewSize)
            {
                throw new ArgumentOutOfRangeException("NewSize", MyExceptions.SizeLessThanZero);
            }

            if(NewSize == m_size)
            {
                return;
            }

            if(NewSize < m_size)
            {
                for(int j = 0; j < m_size - NewSize; ++j, --m_tail)
                {
                    if(0 == m_tail)
                    {
                        m_tail = m_capacity;
                    }
                }

                m_size = NewSize;

                return;
            }

            for(int j = 0; j < NewSize - m_size; ++j)
            {
                ++m_tail;

                if((m_capacity  + 1) == m_tail)
                {
                    m_tail = 1;
                }
                
                m_buffer[m_tail - 1] = item;                
            }

            m_size = NewSize;

            return;
        }

        /*
         *  If the new size is greater than the current size, copies of item will be inserted at the front
         * of the of the buffer in order to achieve the desired size.
         *  If the current number of elements stored in the buffer is greater than the desired new size then
         * number of m_size - NewSize first elements will be removed. (The capacity will remain unchanged.)
         */
        public void RReSize(int NewSize, T item)
        {
            if (NewSize > m_capacity)
            {
                throw new ArgumentOutOfRangeException("capacity", "NewSize", MyExceptions.CapacityLessThanSize);
            }

            if (0 > NewSize)
            {
                throw new ArgumentOutOfRangeException("NewSize", MyExceptions.SizeLessThanZero);
            }

            if (NewSize == m_size)
            {
                return;
            }

            if (NewSize < m_size)
            {
                for (int j = 0; j < m_size - NewSize; ++j, ++m_head)
                {
                    if (m_capacity == m_head)
                    {
                        m_head = 0;
                    }
                }

                m_size = NewSize;

                return;
            }

            for (int j = 0; j < NewSize - m_size; ++j)
            {
                --m_head;

                if (-1 == m_head)
                {
                    m_head = m_capacity - 1;
                }

                m_buffer[m_head] = item;
                //Console.WriteLine("{0} {1}", m_head, item);
            }

            m_size = NewSize;

            return;
        }

        /*
         *  Reverses the buffer: a new tail is the old m_head, a new head is the old m_tail and so on. 
         */
        public void Reverse()
        {
            for (int i = m_head, j = m_tail, k = 0; k < m_size / 2; ++k, ++i, --j)
            {
                if (m_capacity == i)
                {
                    i = 0;
                }

                if (0 == j)
                {
                    j = m_capacity;
                }

                T tmp = m_buffer[i];
                m_buffer[i] = m_buffer[j - 1];
                m_buffer[j - 1] = tmp;
            }

            return;
        }

        /*
         *  Bypasses a specified number of elements in a sequence. Only for full buffer.
         */
        public void Skip(int count)
        {
            if (!IsFull())
            {
                return;
            }

            m_head += count;

            if (m_head >= m_capacity)
            {
                m_head %= m_capacity;
            }

            return;
        }
        
        /*
         *  Returns the first element of a sequence. 
         */
        public T First()
        {
            if (IsEmpty())
            {
                throw new ArgumentOutOfRangeException(MyExceptions.SizeIsNull);
            }

            return m_buffer[m_head];
        }

        /*
         *  Returns the last element of a sequence. 
         */
        public T Last()
        {
            if (IsEmpty())
            {
                throw new ArgumentOutOfRangeException(MyExceptions.SizeIsNull);
            }

            return m_buffer[m_tail - 1];
        }

        /*
         *  Get the first continuous array of the buffer. 
         */
        public T[] Array_One()
        {
            if (IsEmpty())
            {
                throw new ArgumentOutOfRangeException(MyExceptions.SizeIsNull);
            }

            if (IsLinearized())
            {
                return Linearize();
            }

            T[] ReturnArray = new T[m_capacity - m_head];
            for (int i = m_head, j = 0; i < m_capacity; ++i, ++j)
            {
                ReturnArray[j] = m_buffer[i];
            }

            return ReturnArray;
        }

        /*
         *  Get the second continuous array of the buffer. 
         */
        public T[] Array_Two()
        {
            if (IsEmpty())
            {
                throw new ArgumentOutOfRangeException(MyExceptions.SizeIsNull);
            }

            if (IsLinearized())
            {
                return null;
            }

            T[] ReturnArray = new T[m_tail];
            for (int i = 0; i < m_tail; ++i)
            {
                ReturnArray[i] = m_buffer[i];
            }

            return ReturnArray;
        }

        /*
         *  Linearize the buffer into a continuous array. 
         */
        public T[] Linearize()
        {
            T[] Result = new T[m_size];
            CopyTo(Result);

            return Result;
        }

        /*
         *  Returns the buffer. 
         */
        public T[] GetBuffer()
        {
            return m_buffer;
        }

        /*
         *  Inserts the MyElement to the end of the buffer. 
         */
        public void Insert(T MyElement)
        {
            if ((false == AllowOverWrite) && (m_size == m_capacity))
            {
                throw new OverflowException(MyExceptions.SizeEqualCapacity);
            }

            //m_buffer[tail]

            
            if(m_size < m_capacity)
            {
                ++m_size;

                if(m_tail == m_capacity)
                {
                    m_tail = 1;
                }
                else
                {
                    ++m_tail;
                }
            }
            else
            {
                if (m_capacity == m_tail)
                {
                    if (0 == m_head)
                    {
                        ++m_head;
                    }
                    m_tail = 1;
                }
                else
                {
                    if ((m_capacity - 1) == m_head)
                    {
                        m_head = 0;
                        ++m_tail;
                    }
                    else
                    {
                        ++m_head;
                        ++m_tail;
                    }                    
                }
            }
            
            m_buffer[m_tail - 1] = MyElement;            
            
            return;
        }

        /*
         *  Inserts count elements to the MyArray beginning with the startIndex position. 
         */
        public int Insert(T[] MyArray, int startIndex, int count)
        {
            if (0 > startIndex)
            {
                throw new ArgumentOutOfRangeException("startIndex", MyExceptions.InsertArrayIndexIsLessThanNull);
            }

            count = Math.Min(count, MyArray.Length);
            int TrueCount = (AllowOverWrite) ? count : Math.Min(count, m_capacity - m_size);

            for (int i = startIndex; i < TrueCount; ++i)
            {

                if (m_size < m_capacity)
                {
                    ++m_size;

                    if (m_tail == m_capacity)
                    {
                        m_tail = 1;
                    }
                    else
                    {
                        ++m_tail;
                    }
                }
                else
                {
                    if (m_capacity == m_tail)
                    {
                        if (0 == m_head)
                        {
                            ++m_head;
                        }
                        m_tail = 1;
                    }
                    else
                    {
                        if ((m_capacity - 1) == m_head)
                        {
                            m_head = 0;
                            ++m_tail;
                        }
                        else
                        {
                            ++m_head;
                            ++m_tail;
                        }
                    }
                }

                m_buffer[m_tail - 1] = MyArray[i];
            }

            return TrueCount;
        }

        /*
         *  Inserts the bffer to the MyArray. 
         */
        public int Insert(T[] MyArray)
        {
            return Insert(MyArray, 0, MyArray.Length);            
        }

        /*
         *  Removes all items from the ICollection<T> (in this release it just makes m_size equal to 0). 
         */
        public void Clear()
        {
            m_size = 0;
            m_head = 0;
            m_tail = 0;
            
            return;
        }

        /*
         *  Determines whether the ICollection<T> contains a specific value. 
         */
        public bool Contains(T MyElement)
        {
            if (0 == m_size)
            {
                return false;
            }

            for (int j = 0, i = m_head; j < m_size; ++j, ++i)
            {
                if (i == m_capacity)
                {
                    i = 0;
                }

                if (null == MyElement && null == m_buffer[i])
                {
                    return true;
                }

                if (MyElement.Equals(m_buffer[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         *  Copies count elements of the ICollection<T> to the array, starting at the particular arrayIndex. 
         */
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (null == array)
            {
                throw new ArgumentNullException("array", MyExceptions.CopyToArrayIsNull);
            }

            if (0 > arrayIndex)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", MyExceptions.CopyToArrayIndexIsLessThanNull);
            }

            if (array.Length <= arrayIndex)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", MyExceptions.CopyToArrayIndexIsMoreThanLength);
            }

            int TrueCount = Math.Min(Math.Min(count, m_size), (array.Length - arrayIndex));

            for (int i = m_head, j = arrayIndex; j < TrueCount; ++i, ++j)
            {
                if (m_capacity == i)
                {
                    i = 0;
                }

                array[j] = m_buffer[i];
            }

            return;
        }

        /*
         *  Copies the elements of the ICollection<T> to the array, starting at the particular arrayIndex. 
         */
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex, m_size);

            return;
        }

        /*
         *  Copies the elements of the ICollection<T> to the array. 
         */
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);

            return;
        }

        /*
         *  Gets the first element of the buffer and after that deletes it (decrements m_size). 
         */
        public T Get()
        {
            if (0 == m_size)
            {
                throw new ArgumentOutOfRangeException(MyExceptions.SizeIsNull);
            }

            --m_size;
            T ReturnItem = m_buffer[m_head];
            ++m_head;

            if (m_capacity == m_head)
            {
                m_head = 0;
            }

            return ReturnItem;
        }

        /*
         *  Gets count elements from the buffer, puts them to the array beginning with arrayIndex position,
         * after that deletes gotten elements (decrements m_size).   
         */
        public int Get(T[] array, int arrayIndex, int count)
        {
            if (null == array)
            {
                throw new ArgumentNullException("array", MyExceptions.CopyToArrayIsNull);
            }

            if (0 > arrayIndex)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", MyExceptions.CopyToArrayIndexIsLessThanNull);
            }

            if (array.Length <= arrayIndex)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", MyExceptions.CopyToArrayIndexIsMoreThanLength);
            }

            int TrueCount = Math.Min(Math.Min(count, m_size), (array.Length - arrayIndex));

            for (int j = arrayIndex; j < TrueCount; ++j)
            {
                if (m_capacity == m_head)
                {
                    m_head = 0;
                }

                array[j] = m_buffer[m_head];
                ++m_head;
            }

            m_size -= TrueCount;

            return TrueCount;
        }

        /*
         *  Gets the array.Length elements from the buffer, puts them to the array beginning with arrayIndex position,
         * after that deletes gotten elements (decrements m_size).   
         */
        public int Get(T[] array, int arrayIndex)
        {
            return Get(array, arrayIndex, array.Length);
        }

        /*
         *  Gets the array.Length elements from the buffer, puts them to the array, after that deletes gotten elements
         * (decrements m_size).   
         */
        public int Get(T[] array)
        {
            return Get(array, 0);
        }

        /*
         *  Removes the first occurrence of a specific object from the ICollection<T>. 
         */
        public bool Remove(T MyElement)
        {
            if (false == Contains(MyElement))
            {
                return false;
            }

            for (int i = m_head, j = 0; j < m_size; ++i, ++j)
            {
                if (m_capacity == i)
                {
                    i = 0;
                }

                if (MyElement.Equals(m_buffer[i]))
                {
                    if (i == m_head)
                    {
                        if (m_capacity - 1 == m_head)
                        {
                            m_head = 0;
                        }
                        else
                        {
                            ++m_head;
                        }
                    }

                    for (int k = i, l = 0; k + 1 != m_tail; ++k, ++l)
                    {
                        if (m_capacity == k + 1)
                        {
                            m_buffer[k] = m_buffer[0];
                            k = 0;
                        }
                        else
                        {
                            m_buffer[k] = m_buffer[k + 1];
                        }
                    }

                    break;
                }
            }

            --m_size;

            return true;
        }

        /*
         *  Removes the first element from the buffer. 
         */
        public bool Pop_Front()
        {
            if (0 == m_size)
            {
                throw new ArgumentOutOfRangeException(MyExceptions.SizeIsNull);
            }

            ++m_head;
            if (m_capacity == m_head)
            {
                m_head = 0;
            }

            --m_size;

            return true;
        }

        /*
         *  Removes the last element from the buffer. 
         */
        public bool Pop_Back()
        {
            if (0 == m_size)
            {
                throw new ArgumentOutOfRangeException(MyExceptions.SizeIsNull);
            }

            --m_tail;
            if (0 == m_tail)
            {
                m_tail = m_capacity;
            }

            --m_size;

            return true;
        }

        /*
         *  Rotates the order of the elements in the buffer, in such a way that the element pointed by middle becomes
         * the new first element. 
         */
        public void Rotate(int first, int middle, int last)
        {
            int next = middle;
            while (first != next)
            {
                T tmp = m_buffer[first];
                m_buffer[first] = m_buffer[next];
                m_buffer[next] = tmp;

                ++first;
                ++next;

                if (next == last)
                {
                    next = middle;
                }
                else
                {
                    if (first == middle)
                    {
                        middle = next;
                    }
                }
            }

            return;
        }

        public IEnumerator<T> GetEnumerator()
        {
            int i = m_head;
            for (int j = 0; j < m_size; ++j, ++i)
            {
                if (i == m_capacity)
                {
                    i = 0;
                }

                yield return m_buffer[i];
            }
        }

/****** IEnumerable<T> Methods **********************************************************************************************/

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

/****** IEnumerable Methods *************************************************************************************************/

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

/****** ICollection<T> Properties *******************************************************************************************/

        int ICollection<T>.Count
        {
            get
            {
                return Size;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

/****** ICollection<T> Methods **********************************************************************************************/

        void ICollection<T>.Add(T item)
        {
            Insert(item);
            return;
        }

        void ICollection<T>.Clear()
        {
            Clear();
            return;
        }

        bool ICollection<T>.Contains(T item)
        {
            return Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
            return;
        }

        bool ICollection<T>.Remove(T item)
        {
            return Remove(item);
        }

/****** ICollection Properties **********************************************************************************************/

        int ICollection.Count
        {
            get
            {
                return Size;
            }
        }
        
        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (null == m_SyncRoot)
                {
                    Interlocked.CompareExchange(ref m_SyncRoot, new object(), null);
                }

                return m_SyncRoot;
            }
        }

/****** ICollection Methods *************************************************************************************************/

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            CopyTo((T[])(array), arrayIndex);
            return;
        }
        
    }

/****** class Program *******************************************************************************************************/

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
