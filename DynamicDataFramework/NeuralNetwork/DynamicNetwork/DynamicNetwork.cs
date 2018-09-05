using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DynamicData
{
    public class DynamicNetwork
    {
        /// <summary>
        /// Nodes in your network. Includes inputs. (The Mitocontria)
        /// </summary>
        public List<Node> Nodes { get; set; }
        /// <summary>
        /// Outputs the network can select as the answer.
        /// </summary>
        public List<OutputNode> Outputs { get; set; }

        private List<PairBit> train = null; // training data

        /// <summary>
        /// Build Options. Determine how to setup the network.
        /// </summary>
        public DynamicBuild BuildOptions = null;

        private Random rndm = null;

        private int total = 128;
        private int totalIn = 1;

        /// <summary>
        /// Empty Dynamic Network Constructor. Allows you to implement your own node structure.
        /// </summary>
        public DynamicNetwork()
        {

        }

        /// <summary>
        /// Used to setup a training instance of a network
        /// </summary>
        /// <param name="TrainData"></param>
        /// <param name="build"></param>
        public DynamicNetwork(List<PairBit> TrainData, DynamicBuild build = null)
        {
            BuildOptions = build;
            train = TrainData;

            if (BuildOptions != null)
            {
                totalIn = TrainData.OrderBy(x => x.Inputs.Length).Last().Inputs.Length;

                if (BuildOptions.ShuffleData) ReShuffleTrainingData();
            }

            Outputs = new List<OutputNode>();
            Nodes = new List<Node>();

            SetUpBuild();
        }

        /// <summary>
        /// Loads Nodes from a serialized file.
        /// </summary>
        /// <param name="file">The serialized file location.</param>
        public DynamicNetwork(string file)
        {
            SaveState state = new SaveState();
            FileStream f = File.Open(file, FileMode.Open);
            BinaryFormatter b = new BinaryFormatter();
            state = (SaveState)b.Deserialize(f);
            f.Close();

            Nodes = state.Nodes;
            Outputs = state.Outputs;
        }

        /// <summary>
        /// Saves Node States to a serialized binary file
        /// </summary>
        /// <param name="file">The save to file path.</param>
        /// <param name="state">Set to save a specific state. Otherwise it saves the current state.</param>
        public void SaveNetwork(string file, SaveState state = null)
        {
            if(state == null) state = GetSaveState();

            FileStream f = File.Open(file, FileMode.OpenOrCreate);
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(f, state);
            f.Close();               
        }

        /// <summary>
        /// Gets the current saved state.
        /// </summary>
        /// <returns></returns>
        public SaveState GetSaveState()
        {
            SaveState state = new SaveState
            {
                Outputs = CopyOutputs(),
                Nodes = CopyNodes(),
            };

            return state;      
        }

        /// <summary>
        /// Used to save the state of the network during training.
        /// </summary>
        /// <returns></returns>
        public List<Node> CopyNodes()
        {
            List<Node> nds = new List<Node>();

            foreach (Node n in Nodes)
            {
                nds.Add(n.Copy());
            }

            return nds;
        }

        /// <summary>
        /// Used to save the state of the network during training.
        /// </summary>
        /// <returns></returns>
        public List<OutputNode> CopyOutputs()
        {
            List<OutputNode> nds = new List<OutputNode>();

            foreach (OutputNode n in Outputs)
            {
                nds.Add(n.Copy());
            }

            return nds;
        }

        /// <summary>
        /// Setup method for the DynamicBuildOptions
        /// </summary>
        public void SetUpBuild()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;

            rndm = new Random(secondsSinceEpoch - DateTime.Now.Millisecond);

            if (BuildOptions != null)
            {
                if (BuildOptions.TakeRandom)
                {
                    SetUpRandom();
                }

                total = rndm.Next(BuildOptions.MinTotalNodes, BuildOptions.MaxTotalNodes) + totalIn;
            }

            SetUpNodes();
            SetUpOutputs();
        }

        /// <summary>
        /// Create a node network according to the DynamicBuild Options.
        /// </summary>
        public void SetUpNodes()
        {
            if (BuildOptions != null)
            {
                for (int i = 0; i < total; i++)
                {
                    int conNum = rndm.Next(BuildOptions.MinNodeConnections, BuildOptions.MaxNodeConnections);

                    List<int> indexs = new List<int>();

                    for (int n = 0; n < conNum; n++)
                    {
                        indexs.Add(rndm.Next(0, total));
                    }

                    bool inpt = false;

                    if (i < totalIn)
                    {
                        Nodes.Add(new Node { Connections = null, isInput = true, Value = 0, Threshold = 0 });
                    }
                    else
                    {
                        Nodes.Add(new Node { Connections = indexs.ToArray(), isInput = inpt, Value = 0, Threshold = rndm.NextDouble() });
                    }
                }
            }
        }

        /// <summary>
        /// Create output nodes that correlate with a distinct array of answers found in the training data.
        /// </summary>
        public void SetUpOutputs()
        {
            string[] ans = GetAnswers();

            if (ans != null)
            {
                foreach (string a in ans)
                {
                    if (BuildOptions != null) Outputs.Add(new OutputNode(a, totalIn, total));
                    else { Outputs.Add(new OutputNode(a, Nodes.Count - ans.Length, Nodes.Count)); }
                }
            }
        }

        /// <summary>
        /// Assigns the random class according to DynamicBuild Options.
        /// </summary>
        /// <param name="random">Leave as null for auto-assignment</param>
        public void SetUpRandom(Random random = null)
        {
            if (random == null)
            {
                if (BuildOptions.TakeRandom)
                {
                    int div = ExternalRandom();
                    rndm = new Random(div);
                }
                else { rndm = new Random(this.GetHashCode()); }
            }
            else rndm = random;
        }

        /// <summary>
        /// Rerolls the output weights. Used in training
        /// </summary>
        public void ReRollNetwork()
        {
            foreach (OutputNode n in Outputs)
            {
                n.ReRollWeights(rndm);
            }
        }

        /// <summary>
        /// Gets a distinct array of answers from training data
        /// </summary>
        /// <returns></returns>
        public string[] GetAnswers()
        {
            if (train != null)
            {
                List<string> ans = train.Select(x => x.Answer).Distinct().ToList();
                return ans.OrderBy(x => x.ToString()).ToArray();
            }
            else return null;
        }

        /// <summary>
        /// Reshuffles the data to prevent network bias
        /// </summary>
        public void ReShuffleTrainingData()
        {
            Shuffle.ShuffleList(train);
        }

        /// <summary>
        /// Fills the input nodes with the input data.
        /// </summary>
        /// <param name="rawData"></param>
        public void Seed(double[] rawData)
        {
            for (int i = 0; i < rawData.Length; i++)
            {
                if (i < Nodes.Count)
                {
                    Nodes[i].isInput = true;
                    Nodes[i].Value = rawData[i];
                }
            }
        }

        /// <summary>
        /// Fills the input nodes with the input data.
        /// </summary>
        /// <param name="rawData"></param>
        public void Seed(byte[] rawData)
        {
            for (int i = 0; i < rawData.Length; i++)
            {
                if (i < Nodes.Count)
                {
                    Nodes[i].isInput = true;
                    Nodes[i].Value = rawData[i];
                }
            }
        }

        /// <summary>
        /// if(DynamicBuild.TakeRandom == true){request an int value from https://www.random.org }
        /// </summary>
        /// <returns>Int requested from web</returns>
        public int ExternalRandom()
        {
            System.Net.WebClient wb = new System.Net.WebClient();
            string bt = wb.DownloadString("https://www.random.org/cgi-bin/randbyte?nbytes=4&format=d");
            string[] spl = bt.Split(' ');
            List<string> fn = new List<string>();
            foreach (string sp in spl)
            {
                if (sp != string.Empty) fn.Add(sp);
            }

            byte[] convrt =
            {
                Convert.ToByte(fn[0]),
                Convert.ToByte(fn[1]),
                Convert.ToByte(fn[2]),
                Convert.ToByte(fn[3])
            };
            return BitConverter.ToInt32(convrt, 0);
        }

        /// <summary>
        /// Run the network with a set of inputs. bit.Answer is not required for this and can be set to null.
        /// </summary>
        /// <param name="bit">Only Needs the set of inputs you would like to use. Use bit.CreateDoubleInputs(true); if you want double inputs that are not 0.0 to 1.0</param>
        /// <returns>An output that indicates what the network views as the correct answer.</returns>
        public OutputPair RunNetwork(PairBit bit)
        {
            OutputPair outPair = new OutputPair();

            if(bit.DoubleInputs == null)
            {
                bit.CreateDoubleInputs(false);
            }

            Seed(bit.DoubleInputs);

            double[] valuesOut = new double[Nodes.Count]; //stores node values

            for (int m = Nodes.Count - 1; m > -1; m--)
            {
                valuesOut[m] = Nodes[m].Value; //add initial node values to the array
            }

            for (int m = Nodes.Count - 1; m > bit.Inputs.Length - 1; m--) //cycle through the nodes
            {
                if (!Nodes[m].isInput) // skip inputs
                {
                    int[] dx = Nodes[m].Connections;

                    if (dx != null)
                    {
                        double val = Nodes[m].GetValue(valuesOut); // calculate node value from connection values

                        valuesOut[m] = val;
                    }
                }
            }

            List<OutputPair> op = new List<OutputPair>();

            for (int i = 0; i < Outputs.Count; i++)
            {
                op.Add(Outputs[i].ValueOutput(valuesOut)); // calculate outputs
            }

            OutputPair selected = op.OrderBy(x => x.DoubleScore).Last(); // get output by the highest DoubleScore

            return selected;
        }

        /// <summary>
        /// Assign Training Data
        /// </summary>
        /// <param name="data">Training Dataset</param>
        public void SetTrain(List<PairBit> data)
        {
            train = data;
        }

        /// <summary>
        /// Method for training a network.
        /// </summary>
        /// <param name="Cutoff">Percentage threshold for cutting the network training instance short. Saves time. (ex. 75% is 0.75, 100% is 1.00, etc.)</param>
        /// <param name="sampleSize">Number of samples to run before checking if the Cutoff should activate. Increases after each successful check. (check += check) </param>
        /// <returns>Response detailing errors encountered and percentage accuracy.</returns>
        public NetworkResponse Train(double Cutoff = 0, double sampleSize = 50)
        {
            NetworkResponse stp = new NetworkResponse();

            if (BuildOptions != null)
                if (BuildOptions.ShuffleData)
                    ReShuffleTrainingData();

            double check = sampleSize;

            if(train == null)
            {
                stp.Error = "Failed: No training data.";

                return stp;
            }
            else if (train.Count < 1)
            {
                stp.Error = "Failed: Not enought training data.";

                return stp;
            }
                int start = 0;

            for (int t = start; t < train.Count; t++)
            {
                if ((train[t].Inputs.Length < Nodes.Count))
                {
                    OutputPair outPair = null;

                    outPair = RunNetwork(train[t]);

                    if (train[t].Answer == outPair.Answer)
                    {
                        stp.Correct++;
                    }
                    else
                    {
                        stp.Wrong++;
                    }

                    if (check < t)
                    {
                        if (stp.Percent < Cutoff)
                        {
                            stp.Error = "Failed to Maintain High Percentage.";

                            return stp;
                        }

                        check += check;
                    }
                }
                else
                {
                    stp.Error = "Failed: Not enough nodes for inputs.";

                    return stp;
                }
            }

            return stp;
        }

    }
}
