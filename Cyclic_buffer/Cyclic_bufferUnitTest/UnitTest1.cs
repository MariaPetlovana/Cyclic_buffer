using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cyclic_buffer;

namespace Cyclic_bufferUnitTest
{
    [TestClass]
    public class UnitTest1
    {

        private const int RandomDataLength = 100;
        
        private int[] FillRandomData(int Length)
        {
            int[] arr = new int[Length];
            int seed = 127;
            Random r = new Random(seed);

            for (int i = 0; i < arr.Length; ++i)
            {
                arr[i] = r.Next();
            }

            return arr;
        }

        [TestMethod]
        public void ConstructorsAllowOverWriteRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength);
            Ring_buffer<int> buf1 = new Ring_buffer<int>(MyArray.Length);
            Ring_buffer<int> MyBuf1 = new Ring_buffer<int>(MyArray.Length, true);

            Assert.AreEqual(buf1.AllowOverWrite, MyBuf1.AllowOverWrite);

            Ring_buffer<int> buf2 = new Ring_buffer<int>(MyArray.Length, MyArray[0]);
            Ring_buffer<int> MyBuf2 = new Ring_buffer<int>(MyArray.Length, MyArray[0], true);
            
            Assert.AreEqual(buf2.AllowOverWrite, MyBuf2.AllowOverWrite);

            Ring_buffer<int> buf3 = new Ring_buffer<int>(RandomDataLength, RandomDataLength - 20, MyArray[0]);
            Ring_buffer<int> MyBuf3 = new Ring_buffer<int>(RandomDataLength, RandomDataLength - 20, MyArray[0], true);
            
            Assert.AreEqual(buf3.AllowOverWrite, MyBuf3.AllowOverWrite);
        }

        [TestMethod]
        public void ConstructorsRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength);
            Ring_buffer<int> buf1 = new Ring_buffer<int>(MyArray.Length);
            Ring_buffer<int> MyBuf1 = new Ring_buffer<int>(MyArray.Length, false);

            CollectionAssert.AreEqual(buf1, MyBuf1);
            
            Ring_buffer<int> buf2 = new Ring_buffer<int>(MyArray.Length, MyArray[0]);
            Ring_buffer<int> MyBuf2 = new Ring_buffer<int>(MyArray.Length, MyArray[0], false);

            CollectionAssert.AreEqual(buf2, MyBuf2);
            
            Ring_buffer<int> buf3 = new Ring_buffer<int>(RandomDataLength, RandomDataLength - 20, MyArray[0]);
            Ring_buffer<int> MyBuf3 = new Ring_buffer<int>(RandomDataLength, RandomDataLength - 20, MyArray[0], false);

            CollectionAssert.AreEqual(buf3, MyBuf3);
       }

        [TestMethod]
        public void CopyRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength);
            Ring_buffer<int> buf = new Ring_buffer<int>(MyArray.Length);

            buf.Insert(MyArray);

            Ring_buffer<int> CopyBuf = new Ring_buffer<int>(buf);

            CollectionAssert.AreEqual(buf, CopyBuf);
            Assert.IsFalse(buf.IsEmpty());
            Assert.IsFalse(CopyBuf.IsEmpty());
        }

        [TestMethod]
        public void EmptyRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength);                      
            Ring_buffer<int> buf = new Ring_buffer<int>(MyArray.Length);

            buf.Insert(MyArray);
            
            int[] NewArray = new int[MyArray.Length];
            buf.Get(NewArray);

            CollectionAssert.AreEqual(MyArray, NewArray);
            Assert.IsFalse(!buf.IsEmpty());
        }

        [TestMethod]
        public void FullRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength);
            Ring_buffer<int> buf = new Ring_buffer<int>(MyArray.Length);

            buf.Insert(MyArray);

            CollectionAssert.AreEqual(MyArray, buf.Linearize());
        }

        [TestMethod]
        public void RandomOverWriteRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength);
            Ring_buffer<int> buf = new Ring_buffer<int>(MyArray.Length);

            buf.Insert(FillRandomData(RandomDataLength / 2));
            buf.Insert(MyArray);

            CollectionAssert.AreEqual(MyArray, buf.Linearize());
        }

        [TestMethod]
        public void PropertiesRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength - 20);
            Ring_buffer<int> buf = new Ring_buffer<int>(RandomDataLength - 20);

            buf.Capacity = 100;
            buf.Insert(MyArray);

            Assert.AreEqual(100, buf.Capacity);
            Assert.AreEqual(80, buf.Size);
            Assert.AreEqual(20, buf.Reserve);
            Assert.AreEqual(0, buf.Head);
            Assert.AreEqual(80, buf.Tail);
            Assert.AreEqual(MyArray[0], buf.First());
            Assert.AreEqual(MyArray[RandomDataLength - 21], buf.Last());
            Assert.AreEqual(true, buf.AllowOverWrite);
            Assert.IsFalse(buf.IsEmpty());
            Assert.IsFalse(buf.IsFull());
            Assert.IsFalse(!buf.IsLinearized());
        }

        [TestMethod]
        public void ResizeRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength - 20);
            Ring_buffer<int> buf = new Ring_buffer<int>(MyArray.Length);

            buf.Insert(MyArray);
            buf.Capacity = 100;
            buf.ReSize(100, 5);
            
            Ring_buffer<int> MyBuf = new Ring_buffer<int>(MyArray.Length);
            
            MyBuf.Insert(MyArray);
            MyBuf.Capacity = 100;

            for (int i = 0; i < 20; ++i)
            {
                MyBuf.Insert(5);
            }
            
            CollectionAssert.AreEqual(buf, MyBuf);
            Assert.IsFalse(MyBuf.IsEmpty());
        }

        [TestMethod]
        public void RResizeRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength - 20);
            Ring_buffer<int> buf = new Ring_buffer<int>(RandomDataLength - 20);

            buf.Skip(20);
            buf.Capacity = 100;
            buf.RReSize(100, 5);
            buf.Insert(MyArray);            

            Ring_buffer<int> MyBuf = new Ring_buffer<int>(RandomDataLength);

            for (int i = 0; i < 20; ++i)
            {
                MyBuf.Insert(5);
            }

            MyBuf.Insert(MyArray);
            
            CollectionAssert.AreEqual(buf, MyBuf);
            Assert.IsFalse(MyBuf.IsEmpty());
        }

        [TestMethod]
        public void ReverseRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength);
            Ring_buffer<int> buf = new Ring_buffer<int>(MyArray.Length);

            buf.Insert(MyArray);
            buf.Reverse();

            int[] NewArray = new int[RandomDataLength];
            for (int i = 0; i < RandomDataLength; ++i)
            {
                NewArray[i] = MyArray[RandomDataLength - i - 1];
            }

            int[] CopyArray = buf.Linearize();

            CollectionAssert.AreEqual(NewArray, CopyArray);
            Assert.IsFalse(buf.IsEmpty());
        }

        [TestMethod]
        public void RotateRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength);
            Ring_buffer<int> buf = new Ring_buffer<int>(MyArray.Length);

            buf.Insert(MyArray);
            Ring_buffer<int> MyBuf = new Ring_buffer<int>(buf);            

            buf.Rotate(buf.Head, 30, buf.Tail);
            buf.Rotate(buf.Head, 70, buf.Tail);

            int[] NewArray1 = buf.Linearize();
            int[] NewArray2 = MyBuf.Linearize();
            
            CollectionAssert.AreEqual(NewArray1, NewArray2);
            Assert.IsFalse(buf.IsEmpty());
        }

        [TestMethod]
        public void ArrayOneTwoRing_buffer()
        {
            Ring_buffer<int> buf = new Ring_buffer<int>(RandomDataLength, true);
            buf.Insert(FillRandomData(RandomDataLength));

            for (int i = 0; i < RandomDataLength / 5; ++i)
            {
                buf.Pop_Front();
            }

            Random r = new Random();

            for (int i = 0; i < RandomDataLength / 10; ++i)
            {
                buf.Insert(r.Next());
            }

            int[] ComparearrayPart1 = buf.Array_One();

            Assert.AreEqual(80, ComparearrayPart1.Length);

            int[] ComparearrayPart2 = buf.Array_Two();
            
            int[] Comparearray = new int[ComparearrayPart1.Length + ComparearrayPart2.Length];
            int u = 0;
            for ( ; u < ComparearrayPart1.Length; ++u)
            {
                Comparearray[u] = ComparearrayPart1[u];
            }
            for (int j = 0; j < ComparearrayPart2.Length; ++j, ++u)
            {
                Comparearray[u] = ComparearrayPart2[j];
            }
            
            CollectionAssert.AreEqual(Comparearray, buf.Linearize());
        }

        [TestMethod]
        public void PopRing_buffer()
        {
            Ring_buffer<int> MyBuf = new Ring_buffer<int>(3);

            MyBuf.Insert(1);
            MyBuf.Insert(2);
            MyBuf.Insert(3);

            int a = MyBuf.ElementAt(0);  // a == 1
            int b = MyBuf.ElementAt(1);  // b == 2
            int c = MyBuf.ElementAt(2);  // c == 3

            MyBuf.Insert(4);
            MyBuf.Insert(5);

            a = MyBuf.ElementAt(0);  // a == 4
            b = MyBuf.ElementAt(1);  // b == 5
            c = MyBuf.ElementAt(2);  // c == 3

            int[] arr = MyBuf.Linearize();

            MyBuf.Pop_Back();
            MyBuf.Pop_Front();

            int d = MyBuf.First();
            int e = MyBuf.ElementAt(0);

            Assert.AreEqual(d, e);
        }

        [TestMethod]
        public void ContainsGetRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength);
            Ring_buffer<int> buf = new Ring_buffer<int>(MyArray.Length);

            buf.Insert(MyArray);

            Assert.IsTrue(buf.Contains(MyArray[0]));

            int a = buf.Get();

            Assert.AreEqual(MyArray[0], a);
            Assert.IsFalse(buf.Contains(MyArray[0]));
            Assert.IsFalse(buf.IsEmpty());
        }

        [TestMethod]
        public void RemoveElementRing_buffer()
        {
            int[] MyArray = FillRandomData(RandomDataLength);
            Ring_buffer<int> buf = new Ring_buffer<int>(MyArray.Length);

            buf.Insert(MyArray);
            buf.Remove(MyArray[5]);

            int[] NewArray = new int[RandomDataLength - 1];

            for (int i = 0; i < 5; ++i)
            {
                NewArray[i] = MyArray[i];
            }

            for (int i = 6; i < RandomDataLength; ++i)
            {
                NewArray[i-1] = MyArray[i];
            }

            int[] LineArray = buf.Linearize();

            CollectionAssert.AreEqual(NewArray, LineArray);
            Assert.IsFalse(buf.IsEmpty());
        }
    }
}
