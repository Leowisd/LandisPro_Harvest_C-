using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class IntArray
    {
        private int size;
        private int[] array;

        public void checkIndex(int index)
        {
            if (index<1 && index >size)
                throw new Exception("Error: Invalid Index");
        }

        public IntArray(int n)
        {
            if (n <= 0)
                throw new Exception("Error: invalid n");
            size = n;
            array = new int[size];
            reset();
        }

        ~IntArray()
        {
            array = null;
        }

        public void reset()
        {
            for (int i = 0; i < size; i++)
                array[i] = 0;
        }

        public int this[int index]
        {
            get
            {
                checkIndex(index);
                return array[index - 1];
            }
            set
            {
                array[index-1]= value;
            }
        }

        public int number()
        {
            return size;
        }

        public int sum()
        {
            int s = 0;
            for (int i = 0; i < size; i++)
            {
                s += array[i];
            }

            return s;
        }

        public void dump()
        {
            for (int i=0;i<size;i++)
                Console.Write(array[i] + " ");
            Console.WriteLine();
        }
    }
}
