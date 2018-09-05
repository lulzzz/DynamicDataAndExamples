using System;
using System.Collections.Generic;

namespace DynamicData
{
    /// <summary>
    /// Work in progress. Will eventually be a language for all things data and networks.
    /// </summary>
    public class CompileDAT
    {
        public string HeadName { get; private set; }
        public List<DATkernal> DatList { get; set; }

        public CompileDAT(byte[] file)
        {
            DatList = new List<DATkernal>();
        }

        public byte[] Compile(string file)
        {
            // process code
            return null;
        }

        public void Run()
        {
            bool running = true;

            int currentIndex = 0;

            while (running)
            {
                if (currentIndex < DatList.Count)
                {
                    currentIndex = DatList[currentIndex].Run(currentIndex);
                }
                else
                {
                    running = false;
                }
            }
        }

        private void Heading(string head)
        {
            int lstIn = 0;

            for (int i = 0; i < head.Length; i++)
            {
                if (head[i] == 'N')
                {
                    string h = head.Substring(i, 5);
                    if (h == "Name:")
                    {
                        int index = head.IndexOf(';', i);
                        HeadName = head.Substring(i + 5, index - (i + 5));
                    }
                }
                else if (i + 1 < head.Length)
                {
                    if ((head[i] + head[i + 1]).ToString() == "..")
                    {
                        DATkernal krn = new DATkernal();
                        krn.Index = lstIn++;
                        DatList.Add(krn);
                    }
                }
            }
        }

        public class DATkernal
        {
            public enum Stage { Begin, Next, End };
            public enum Condition { Equal, Greater, Less, GreaterEqual, LessEqual }

            public int Index { get; set; }

            public Stage CurrentStage { get; set; }

            public virtual int Run(int idx)
            {
                return -1;
            }
        }

        public class DATloop : DATkernal
        {
            double value = 0;
            double increment = 0.1;
            double maximum = 1;

            public void Create(double val, double inc, double max)
            {
                value = val;
                increment = inc;
                maximum = max;

                CurrentStage = Stage.Begin;
            }

            public override int Run(int idx)
            {
                if (CurrentStage == Stage.Begin)
                {
                    CurrentStage = Stage.Next;
                }
                else if (CurrentStage == Stage.Next)
                {
                    if (value < maximum)
                    {
                        value += increment;
                    }
                    else
                    {
                        CurrentStage = Stage.End;
                        return idx + 1;
                    }

                    return Index;
                }

                return idx + 1;
            }
        }

        public class DATif : DATkernal
        {
            public Condition ifCondition { get; set; }

            public double Right { get; set; }
            public double Left { get; set; }
            public int EndIndex { get; set; }

            public override int Run(int idx)
            {
                if (ifCondition == Condition.Equal)
                {
                    if (Right == Left)
                    {
                        return idx + 1;
                    }
                    else
                    {
                        return EndIndex;
                    }
                }
                else if (ifCondition == Condition.Greater)
                {
                    if (Left > Right)
                    {
                        return idx + 1;
                    }
                    else
                    {
                        return EndIndex;
                    }
                }
                else if (ifCondition == Condition.Less)
                {
                    if (Left < Right)
                    {
                        return idx + 1;
                    }
                    else
                    {
                        return EndIndex;
                    }
                }
                else if (ifCondition == Condition.GreaterEqual)
                {
                    if (Left >= Right)
                    {
                        return idx + 1;
                    }
                    else
                    {
                        return EndIndex;
                    }
                }
                else if (ifCondition == Condition.LessEqual)
                {
                    if (Left <= Right)
                    {
                        return idx + 1;
                    }
                    else
                    {
                        return EndIndex;
                    }
                }

                return idx;
            }
        }
    }
}



/*//example code (work in progress)
 * Name:Testing Script;
 * ..One.DAT;
 * One:
 * loop(i+=8)
 * int(i) x;
 * int(i + 4) y;
 * out x + y;
 * out #255;
 * if(x < y)
 * out "xless";
 * else(x == y)
 * out "xequal";
 * else(x > y)
 * out "xgreater";
 * else;
 * loop;
 * One;
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */
