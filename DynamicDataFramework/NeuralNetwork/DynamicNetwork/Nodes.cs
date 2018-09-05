using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicData
{
    [Serializable]
    public class OutputNode
    {
        /// <summary>
        /// Text answer returned when OutputNode is selected.
        /// </summary>
        public string AnswerText { get; set; }

        /// <summary>
        /// Total number of Inputs for the Network.
        /// </summary>
        public int InputNodes { get; set; }

        /// <summary>
        /// Total number of Nodes(excluding OutputNodes) for the network.
        /// </summary>
        public int TotalNodes { get; set; }

        public double[] Weights { get; set; }

        /// <summary>
        /// Output controller for the network
        /// </summary>
        /// <param name="answerText">Text answer returned when OutputNode is selected.</param>
        /// <param name="inputNodes">Total number of Inputs for the Network.</param>
        /// <param name="totalNodes">Total number of Nodes(excluding OutputNodes) for the network.</param>
        public OutputNode(string answerText, int inputNodes, int totalNodes)
        {
            AnswerText = answerText;
            InputNodes = inputNodes;
            TotalNodes = totalNodes;

            if (totalNodes > inputNodes)
            {
                if (totalNodes > 0 && inputNodes > 0)
                {
                    Weights = new double[totalNodes];
                }
            }

            ReRollWeights(new Random(DateTime.Now.Millisecond));
        }

        /// <summary>
        /// Empty constructor for OutputNode
        /// </summary>
        public OutputNode()
        {

        }

        /// <summary>
        /// Used to train the network. Reroll takes place after percentage accuracy fails to increase
        /// </summary>
        /// <param name="random">Use existing random to prevent duplicate values</param>
        public void ReRollWeights(Random random = null)
        {
            if(random == null)
            {
                random = new Random(DateTime.Now.Millisecond); // assign if null
            }

            int r = random.Next(0, Weights.Length);

            for(int i = 0; i < Weights.Length; i++)
            {
                Weights[i] = random.NextDouble(); // give weights a random value between 0 and 1
            }
        }

        /// <summary>
        /// Copy the node in a way that is memory independant which prevents overiding when values change.
        /// </summary>
        /// <returns></returns>
        public OutputNode Copy()
        {
            OutputNode n = new OutputNode();

            n.AnswerText = AnswerText;
            n.InputNodes = InputNodes;
            n.TotalNodes = TotalNodes;

            if (Weights != null)
            {
                n.Weights = new double[Weights.Length];
                Weights.CopyTo(n.Weights, 0);
            }

            return n;
        }

        /// <summary>
        /// Determines the value of the nodes weighted connections
        /// </summary>
        /// <param name="data">Array containing total value of the node network. </param>
        /// <returns>A weighted total value as DoubleScore and a text label indicating the networks selection.</returns>
        public OutputPair ValueOutput(double[] data)
        {
            if (Weights != null)
            {
                if (Weights.Length > 0)
                {
                    double num = 0;
                    double weightTotal = 1;

                    for (int i = InputNodes; i < data.Length; i++)
                    {
                        num += data[i] * Weights[i];
                        weightTotal += Weights[i]; // used to calculate the weighted average as opposed to a full average
                    }

                    if (weightTotal > 0)
                    {
                        double bt = (num /= (weightTotal)); //weighted average calculation instead of full average (ex. double bt = (num /= data.length))

                        return new OutputPair
                        {
                            Answer = AnswerText,
                            DoubleScore = bt
                        };
                    }
                }
            }

            return new OutputPair { Answer = string.Empty, DoubleScore = 0 };
        }
    }

    /// <summary>
    /// Class for network nodes. ("The mitocontria are the powerhouse of the cell..." :)
    /// </summary>
    [Serializable]
    public class Node
    {
        /// <summary>
        /// The stored value resulting from the calcuation of the connected nodes.
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// A number between 0.00 and 1.00 that acts as an off switch for the node if the value falls below that number.
        /// </summary>
        public double Threshold { get; set; }
        /// <summary>
        /// Set to true if the node is an input.
        /// </summary>
        public bool isInput { get; set; }
        /// <summary>
        /// List of indices of the connnected nodes.
        /// </summary>
        public int[] Connections { get; set; }

        /// <summary>
        /// Empty constructor for the Node class.
        /// </summary>
        public Node()
        {

        }

        /// <summary>
        /// Copy the node in a way that is memory independant which prevents overiding when values change.
        /// </summary>
        /// <returns></returns>
        public Node Copy()
        {
            Node n = new Node();

            if (Connections != null)
            {
                n.Connections = new int[Connections.Length];
                Connections.CopyTo(n.Connections, 0);
            }

            n.Value = Value;
            n.isInput = isInput;
            n.Threshold = Threshold;

            return n;
        }

        /// <summary>
        /// Calculates the Value of the node
        /// </summary>
        /// <param name="values">Array containing the values of the other nodes in the network.</param>
        /// <returns>The nodes value if it is greater than the threshold. Otherwise returns 0. Input nodes just return the raw value.</returns>
        public double GetValue(double[] values)
        {
            double total = 0;

            if (!isInput)
            {
                if (Connections != null)
                {
                    if (Connections.Length > 0)
                    {
                        for (int i = 0; i < Connections.Length; i++)
                        {
                            total += values[Connections[i]];
                        }

                        total /= Connections.Length;

                        Value = total;

                        if (Value < Threshold) return 0;
                    }
                }
            }

            return Value;
        }
    }
}
