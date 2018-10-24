using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace LandisPro
{
    class system1
    {
        private readonly static char DELIMIT = '#';
        private const int RAND_MAX = 0x7fff;
        private static int charCommentFlag = 0;

        [DllImport("msvcrt.dll")]
        public static extern int srand(int seed);

        [DllImport("msvcrt.dll")]
        public static extern int rand();

        public static void fseed(int seed)
        {
            srand(seed);
            //fixRand = new Random(seed); //srand(seed);
            //Console.WriteLine("Seed {0}", seed);
        }

        public static double drand()
        {
            //Console.WriteLine("drand()");
            //return fixRand.NextDouble();
            //return 0.5f;
            return (double)rand() / (double)RAND_MAX;
        }

        public static float frand()
        {
            //Console.WriteLine("frand()");
            //return (float)fixRand.NextDouble();
            //return 0.5f;
            return (float)rand() / (float)RAND_MAX;
        }

        public static float frand1()
        {
            //Console.WriteLine("frand1()");
            //return (float)fixRand.NextDouble();
            //return 0.5f;
            return (float)((double)rand() / (double)(RAND_MAX + 1));
        }

        public static int frandrand()
        {
            //Console.WriteLine("frandrand()");
            return rand();
            //return fixRand.Next();
            //return 1;
        }

        private static char read_firstchar_nonspace(StreamReader sr)
        {
            while (true) //remove the possible blanks in the beginning 
            {
                char c = (char)sr.Read();

                if (char.IsWhiteSpace(c))
                    continue;
                else
                {
                    // Console.WriteLine("nonspace {0}  {1}", (int)c, c);
                    return c;
                }
            }
        }

        public static int irand(int a, int b)
        {
            //Console.WriteLine("irand()");
            // return (int)(fixRand.Next() % (b - a + 1) + a);
            //return fixRand.Next(a, b);
            return (int)(rand() % (b - a + 1) + a);
        }
        

        public static float read_float(StreamReader sr)
        {
            char[] content = new char[32];

            content[0] = read_firstchar_nonspace(sr);

            int count = 1;
            int point = 0;

            while (true)
            {
                //char c = (char)sr.Peek();
                int tmp_ret_val = sr.Peek();

                if (-1 == tmp_ret_val)//end of file
                    break;

                char c = (char)tmp_ret_val;


                if ('.' == c)
                {
                    sr.Read();
                    content[count] = c;
                    count++; point++;
                    //Console.WriteLine("{0}  {1}", (int)c, c);
                    continue;
                }

                if (char.IsDigit(c))
                {
                    sr.Read();
                    content[count] = c;
                    count++;
                    //Console.WriteLine("{0}  {1}", (int)c, c);
                }
                else
                {
                    //Console.WriteLine("float {0}  {1}", (int)c, c);
                    break;
                }


            }

            string str_val = new string(content);
            float ret_val;
            try
            {
                ret_val = Convert.ToSingle(str_val);
            }
            catch
            {
                Console.WriteLine("convert to float failure: the content is {0}", content);
                throw new Exception();
            }

            return ret_val;
        }

        private static bool isCharComment(char ch)
        {
            if (DELIMIT == ch)
                charCommentFlag = (charCommentFlag + 1) % 2;

            if (charCommentFlag == 1 || ch == DELIMIT)
                return true;
            else
                return false;
        }

        public static bool LDeof(StreamReader sr)
        {
            while (!sr.EndOfStream)
            {
                char ch = (char)sr.Peek();

                if (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\v' || ch == '\f' || ch == '\r' || (isCharComment(ch)))
                {
                    ch = (char)sr.Read();
                    continue;
                }
                else
                    break;
            }

            if (!sr.EndOfStream)
            {
                return false; // Console.WriteLine("not end of file");
            }
            else
            {
                return true; // Console.WriteLine("end of file");
            }

        }

        public static int read_int(StreamReader sr)
        {
            char[] content = new char[32];

            content[0] = read_firstchar_nonspace(sr);

            int count = 1;

            while (true)
            {
                //char c = (char)sr.Peek();
                int tmp_ret_val = sr.Peek();

                if (-1 == tmp_ret_val)//end of file
                    break;

                char c = (char)tmp_ret_val;

                if (char.IsDigit(c))
                {
                    sr.Read();
                    content[count] = c;
                    count++;

                }
                else
                {
                    break;
                }
            }

            string str_val = new string(content);
            int ret_val;
            try
            {
                ret_val = Convert.ToInt32(str_val);
            }
            catch
            {
                Console.WriteLine("convert to int failure: the content is {0}", content);
                throw new Exception();
            }

            return ret_val;
        }

        public static string read_string(StreamReader sr)
        {
            char[] content = new char[256];

            content[0] = read_firstchar_nonspace(sr);

            int count = 1;

            while (true)
            {
                int ret_val = sr.Peek();

                if (-1 == ret_val)//end of file
                    break;

                char c = (char)ret_val;

                //do not know how to process "#" , so remove "#" in the file please, otherwise will be problematic
                if (char.IsWhiteSpace(c))
                    break;

                sr.Read();
                content[count] = c;
                count++;
                //                Console.WriteLine("string {0}  {1}   {2}", (int)c, c, count);
            }

            char[] new_content = new char[count];
            Array.Copy(content, 0, new_content, 0, count);


            string str_val;
            try
            {
                str_val = new string(new_content);
            }
            catch
            {
                Console.WriteLine("convert to string failure: the content is {0}", content);
                throw new Exception();
            }

            return str_val;
        }

        public static void skipblanks(StreamReader sr)
        {
            while (!sr.EndOfStream)
            {
                char ch = (char)sr.Peek();

                if (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\v' || ch == '\f' || ch == '\r' || (isCharComment(ch)))
                {
                    ch = (char)sr.Read();
                    continue;
                }
                else
                    break;
            }
        }

        //might need to convert the value to char
        public static int LDfgetc(StreamReader sr)
        {
            int FirstChar = sr.Read();

            //in the original c++ version, the end of a line is only 10, but in c#, it is 13&10
            //therefore, here 13 will be removed
            while (13 == FirstChar)
            {
                FirstChar = sr.Read();
            }

            return FirstChar;
        }
    }
}
