using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro
{
    public class TimeStep
    {
        static int times = 10;
        public static int Time_step;
        public  int specNum;
        public static int[] countofagevector;

        public  void gettimestep(int temp)
        {
            Time_step = temp;
            times = temp;
        }
        public  void getSpecNum(int num)
        {
            specNum = num;
            countofagevector = new int[num];
        }
        public  void Setlongevity(int i, int num)
        {
            if (i > specNum)
                Console.WriteLine("Illegal spec index in set longevity");
            countofagevector[i - 1] = num / times;
        }

    }

    class agelist
    {
        static readonly uint[] mask = new uint[32]
        {
            0x00000001, 0x00000002, 0x00000004, 0x00000008,
            0x00000010, 0x00000020, 0x00000040, 0x00000080,
            0x00000100, 0x00000200, 0x00000400, 0x00000800,
            0x00001000, 0x00002000, 0x00004000, 0x00008000,
            0x00010000, 0x00020000, 0x00040000, 0x00080000,
            0x00100000, 0x00200000, 0x00400000, 0x00800000,
            0x01000000, 0x02000000, 0x04000000, 0x08000000,
            0x10000000, 0x20000000, 0x40000000, 0x80000000
        };

        protected int[] agevector;

        public agelist() { }

        public agelist(agelist in_agelist)
        {
            this.copy(in_agelist);
        }

        public void copy(agelist in_agelist)
        {
            if (in_agelist == null)
                return;

            this.copy2(in_agelist.agevector);
        }


        protected void copy2(int[] in_agevector)
        {
            if (in_agevector == null)
                return;

            int count = in_agevector[0];

            agevector = new int[count + 1];

            for (int i = 0; i <= count; i++)
                agevector[i] = in_agevector[i];
        }

        public void clear()
        {
            if (agevector == null)
            {
                agevector = new int[320 + 1];
                agevector[0] = 320;
                // for (i = 1; i <= agevector[0]; i++)
                //  agevector[i] = 0;
            }
            else
            {
                for (int i = 1; i <= agevector[0]; i++)
                    agevector[i] = 0;
            }
        }

        public void reset(int age)
        {
            if (age % TimeStep.Time_step == 0)
                age = age / TimeStep.Time_step;
            else
                age = age / TimeStep.Time_step + 1;
            if (age < 1 || age > 320 / TimeStep.Time_step)
                throw new Exception("Illegal age");
            if (age < 1 || age > agevector[0])
                throw new Exception("Agelist Reset Problem");
            agevector[age] = 0;
        }

        public void set(int age)
        {
            if (age % TimeStep.Time_step == 0)
                age = age / TimeStep.Time_step;
            else
                age = age / TimeStep.Time_step + 1;


            if (age < 1 || age > 320 / TimeStep.Time_step)
                throw new Exception("Illegal age");

            int temp1 = (age - 1) / 32;
            int temp2 = (age - 1) % 32;

            if (age == 64)
                Console.WriteLine("something happened");
            agevector[temp1] |= (int)mask[temp2];

        }

        public bool query()
        {
            for (int i = 1; i <= agevector[0]; i++)
            {
                if (0 == agevector[i])
                    continue;
                else
                    return true;
                //result = result || agevector[i];
            }
            return false;
        }

        public bool query(int age)
        {
            if (age % TimeStep.Time_step == 0)
                age = age / TimeStep.Time_step;
            else
                age = age / TimeStep.Time_step + 1;


            if (age < 1 || age > 320 / TimeStep.Time_step)
                throw new Exception("Illegal age");

            if (age < 1 || age > agevector[0])
                throw new Exception("Agelist Reset Problem");

            //return agevector[age];
            if (agevector[age] == 0)
                return false;
            else
                return true;
        }

        public void dump()
        {
            for (int j = 0; j < 320 / TimeStep.Time_step; j++)
            {
                int temp1 = j / 32;
                int temp2 = j % 32;

                uint ret = (uint)agevector[temp1] & mask[temp2];

                if (ret != 0)
                    Console.WriteLine("{1}: a", temp2);
                else
                    Console.WriteLine("{1}: b", temp2);
            }

            Console.WriteLine();
        }

        public void setTreeNum(int n, int specIndex, int num)
        {
            if (n < 1 || n > TimeStep.countofagevector[specIndex - 1])
                throw new Exception("set age error in agelist 70 Pro");

            if (num < 0)
                agevector[n] = 0;
            else
                agevector[n] = num;
        }

        public void GrowTree()
        {
            for (uint i = (uint)agevector[0]; i > 1; i--)
                agevector[i] = agevector[i - 1];

            agevector[1] = 0;
        }

        public void AGELISTAllocateVector(int index)
        {
            agevector = null;
            agevector = new int[TimeStep.countofagevector[index] + 1];
            int i;
            agevector[0] = TimeStep.countofagevector[index];
            for (i = 1; i <= TimeStep.countofagevector[index]; i++)
            {
                agevector[i] = 0;
            }
        }

        public int getTreeNum(int n, int specIndex)
        {
            if (n == 0)
            {
                return 0; //Add By Qia on March 23 2010 for 'n' index 0 bug
            }
            if (n < 0 || n > TimeStep.countofagevector[specIndex - 1])
            {
                Console.Write("{0:D},{1:D},{2:D}", n, TimeStep.countofagevector[specIndex - 1], agevector[n]);
                throw new Exception("Index age error in agelist 70 Pro");
            }
            return agevector[n];

        }

        public void read(StreamReader infile)
        {
            int numSet = 0, buffer1 = 0, buffer2 = 0, barflag = 0;

            clear();

            int int_times = (int)TimeStep.Time_step;

            system1.skipblanks(infile);

            for (int j = 0; numSet <= 320 && !infile.EndOfStream; j++)
            {
                char ch = (char)system1.LDfgetc(infile);
                if (Char.IsDigit(ch))
                {
                    if (barflag == 0)
                        buffer1 = buffer1 * 10 + ch - 48;
                    else
                        buffer2 = buffer2 * 10 + ch - 48;
                }
                else
                {
                    if (ch == '-')
                    {
                        barflag = 1;
                    }
                    else if (ch == ' ')
                    {
                        if (barflag == 1)
                        {
                            barflag = 0;
                            for (int temp = buffer1; temp <= buffer2; temp = temp + int_times)
                            {
                                if (temp >= int_times)
                                {
                                    agevector[temp / int_times] = 1;

                                    numSet++;
                                }
                            }
                            buffer1 = 0;
                            buffer2 = 0;
                        }
                        else
                        {
                            int temp = buffer1;
                            if (temp >= int_times)
                            {
                                agevector[temp / int_times] = 1;
                                numSet++;
                                buffer1 = 0;
                                buffer2 = 0;
                            }
                        }
                    }
                    else
                    {
                        if (barflag == 1)
                        {
                            barflag = 0;
                            for (int temp = buffer1; temp <= buffer2; temp = temp + int_times)
                            {
                                if (temp >= int_times)
                                {
                                    agevector[temp / int_times] = 1;
                                    numSet++;
                                }
                            }
                            buffer1 = 0;
                            buffer2 = 0;

                        }
                        else
                        {
                            int temp = buffer1;
                            if (temp >= int_times)
                            {
                                agevector[temp / int_times] = 1;
                                numSet++;
                                buffer1 = 0;
                                buffer2 = 0;
                            }
                        }
                        break;
                    }
                }//end else
            }//end for


        }

        public void readTreeNum(string[] sarray, int specIndex)
        {
            int j;
            int numSet = 0;
            int buffer1 = 0;

            for (j = 2; numSet < TimeStep.countofagevector[specIndex] && j < sarray.Length; j++)
            {
                buffer1 = int.Parse(sarray[j]);
                agevector[numSet + 1] = buffer1;
                numSet++;
                //Console.Write(agevector[numSet] + " ");
                buffer1 = 0;
            }

            //Console.WriteLine();
            //Console.Read();
        }

        public int oldest()
        {
            int j;
            for (j = agevector[0]; j >= 1; j--)
            {
                if (agevector[j] > 0)
                {
                    return j * TimeStep.Time_step;
                }
            }
            return 0;

        }
        //Returns the youngest age present.
        public int youngest()
        {
            for (int j = 1; j <= agevector[0]; j++)
            {
                if (agevector[j] > 0)
                    return j * TimeStep.Time_step;
            }

            return 0;
        }

        public int getAgeVectorNum()
        {
            return agevector[0];
        }

        public int getAgeVector(int i)
        {
            if (i < 0)
                throw new Exception("getAgeVector error");

            return agevector[i];
        }

        protected void copy(int[] in_agevector)
        {
            if (in_agevector == null)
                return;

            int count = in_agevector[0];

            agevector = new int[count + 1];

            for (int i = 0; i <= count; i++)
                agevector[i] = in_agevector[i];
        }
    }
}
