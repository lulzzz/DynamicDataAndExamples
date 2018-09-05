using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DynamicData
{
    /// <summary>
    /// Standard training and information storage class for all DynamicData networks.
    /// </summary>
    public class PairBit
    {
        /// <summary>
        /// Answer for training data. Not used in normal use of the network.
        /// </summary>
        public string Answer { get; set; }
        /// <summary>
        /// Raw binary input from the file.
        /// </summary>
        public byte[] Inputs { get; set; }
        /// <summary>
        /// Double values used as inputs in the network. Created from the raw binary input.
        /// </summary>
        public double[] DoubleInputs { get; set; } // add save and load function

        /// <summary>
        /// Loads a binary PairBit file as a list of PairBits 
        /// </summary>
        /// <param name="file">Location of file in system</param>
        /// <returns></returns>
        public static List<PairBit> LoadFromFile(string file)
        {
            List<PairBit> lst = new List<PairBit>();

            if (File.Exists(file))
            {
                byte[] fData = File.ReadAllBytes(file);

                int headerSize = 5;

                int dataLen = BitConverter.ToInt32(fData, 0);
                int labelLen = fData[4];

                int total = dataLen + labelLen;

                byte[] label = new byte[labelLen];
                byte[] put = new byte[dataLen];

                PairBit pb = new PairBit();

                for (int i = (headerSize - 1); i < fData.Length; i++)
                {
                    int pos = ((i - (headerSize)) % total);

                    if (pos >= 0)
                    {
                        if (pos == 0 && i > headerSize)
                        {
                            pb = new PairBit();
                            pb.Answer = fromByteString(label);

                            pb.Inputs = new byte[put.Length];
                            put.CopyTo(pb.Inputs, 0);

                            lst.Add(pb);
                        }

                        if (pos < labelLen)
                        {
                            label[pos] = fData[i];
                        }
                        else
                        {
                            put[pos - labelLen] = fData[i];
                        }
                    }
                }

                pb = new PairBit();
                pb.Answer = fromByteString(label);
                pb.Inputs = new byte[put.Length];
                put.CopyTo(pb.Inputs, 0);

                lst.Add(pb);
            }
            return lst;
        }

        /// <summary>
        /// Saves a list of PairBits to a file in binary format
        /// </summary>
        /// <param name="data">List of PairBits</param>
        /// <param name="file">System location you wish to save to.</param>
        public static void SaveToFile(List<PairBit> data, string file)
        {
            List<byte> bts = new List<byte>();

            if (data != null)
            {
                if (data.Count > 0)
                {
                    List<string> anLen = data.Select(x => x.Answer).Distinct().ToList(); // get all potential answers

                    PairBit topBit = data.OrderBy(x => x.Answer.Length).Last();
                    int len = topBit.Inputs.Length;
                    byte ans = (byte)topBit.Answer.Length;

                    bts.AddRange(BitConverter.GetBytes(len));
                    bts.Add(ans);

                    foreach (PairBit bit in data)
                    {
                        if(bit.Answer != null)
                        {
                            if(bit.Answer != string.Empty)
                            {
                                bts.AddRange(convertString(bit.Answer, ans));
                            }
                        }
                       
                        bts.AddRange(bit.Inputs);
                    }

                    File.WriteAllBytes(file, bts.ToArray());
                }
            }
        }

        private static List<byte> convertString(string s, byte len)
        {
            char[] ch = s.ToCharArray();

            List<byte> bts = new List<byte>();

            for (int i = 0; i < len; i++)
            {
                if (i < ch.Length) bts.Add((byte)ch[i]);
                else bts.Add(32); //keep uniform for parsing
            }

            return bts;
        }

        private static string fromByteString(byte[] bts)
        {
            string f = string.Empty;

            foreach (byte c in bts)
            {
                f += (char)c;
            }

            for(int i = bts.Length - 1; i > 0; i--)
            {
                if(bts[i] == 32)
                {
                    f = f.Remove(i, 1);
                }
            }

            return f;
        }

        /// <summary>
        /// Creates an array of double values to be used in a network
        /// </summary>
        /// <param name="EightByte"> if(EightByte == true) create a double out of eight bytes in a row. Otherwise convert from byte to double. (ex. 0 is 0.00, 128 is 0.5, 255 is 1, etc.)</param>
        public void CreateDoubleInputs(bool EightByte)
        {
            List<double> outputs = new List<double>();

            if (Inputs != null)
            {

                if (EightByte)
                {
                    if (Inputs.Length >= 8)
                    {
                        for (int i = 0; i < Inputs.Length; i += 8)
                        {
                            double bt = BitConverter.ToDouble(Inputs, i);
                            outputs.Add(bt);
                        }
                    }
                }
                else
                {
                    foreach (byte b in Inputs)
                    {
                        double bt = ((double)b / 255);
                        outputs.Add(bt);
                    }
                }

                DoubleInputs = outputs.ToArray();
            }
        }

        /// <summary>
        /// Inverts double inputs that have values between 0.00 and 1.00 (ex.  value = (1 - value);)
        /// </summary>
        public void InvertDoubleInputs()
        {
            List<double> dlist = DoubleInputs.ToList();

            foreach(double d in DoubleInputs)
            {
                dlist.Add(1 - d);
            }

            DoubleInputs = dlist.ToArray();
        }

        /// <summary>
        /// Creates an array of raw byte values to be saved
        /// </summary>
        /// <param name="EightByte"> if(EightByte == true) create eight bytes in a row from a double. Otherwise convert from double to byte. (0.0 is 0, 0.5 is 128, 1.0 is 255, etc.) Max at 255, Min at 0</param>
        public void CreateRawInputsFromDoubleInputs(bool EightByte)
        {
            List<byte> dlist = new List<byte>();

            if (EightByte)
            {
                if (DoubleInputs != null)
                {
                    if (DoubleInputs.Length > 0)
                    {
                        for (int i = 0; i < DoubleInputs.Length; i++)
                        {
                            byte[] bt = BitConverter.GetBytes(DoubleInputs[i]);
                            dlist.AddRange(bt);
                        }
                    }
                }
            }
            else
            {
                foreach (double d in DoubleInputs)
                {
                    double n = d * 255;
                    if (n > 255) n = 255;
                    if (n < 0) n = 0;

                    dlist.Add((byte)n);
                }
            }

            Inputs = dlist.ToArray();
        }
    }
}
