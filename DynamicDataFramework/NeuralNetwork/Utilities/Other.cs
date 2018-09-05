using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using DynamicData;

namespace DynamicData
{
    /// <summary>
    /// Handles response information relating to training of the network.
    /// </summary>
    public class NetworkResponse
    {
        /// <summary>
        /// Error response text.
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// Number of ouputs that were correct for the set of training data.
        /// </summary>
        public double Correct { get; set; }
        /// <summary>
        /// Number of ouputs that were wrong for the set of training data.
        /// </summary>
        public double Wrong { get; set; }

        Stopwatch stp = null;

        /// <summary>
        /// Percentage accuracy against training data.
        /// </summary>
        public double Percent
        {
            get
            {
                if(Correct + Wrong > 0)
                {
                    return Correct / (Correct + Wrong);
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Number of milliseconds since the start of the training instance.
        /// </summary>
        public double ElapsedMiliseconds
        {
            get
            {
                if (stp != null)
                {
                    stp.Stop();

                    return stp.ElapsedMilliseconds;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Empty constructor for NetworkResponse
        /// </summary>
        public NetworkResponse()
        {
            stp = new Stopwatch(); //setup timer and start it.
            stp.Start();
        }
    }

    /// <summary>
    /// Setup options for DynamicNetwork 
    /// </summary>
    public class DynamicBuild
    {
        public int MaxNodeConnections = 7;
        public int MinNodeConnections = 2;

        public int MaxTotalNodes = 256;
        public int MinTotalNodes = 72;

        public bool TakeRandom = true;

        public bool ShuffleData = true;
    }

    /// <summary>
    /// Class to store output results
    /// </summary>
    public class OutputPair
    {
        public string Answer { get; set; }
        public byte Score { get; set; }
        public double DoubleScore { get; set; }
        public string[] AllOptions { get; set; }
        public byte[] Scores { get; set; }
    }

    /// <summary>
    /// Save state class used to store current network state.
    /// </summary>
    [Serializable]
    public class SaveState
    {
        public List<Node> Nodes { get; set; }
        public List<OutputNode> Outputs { get; set; }
    }
}
