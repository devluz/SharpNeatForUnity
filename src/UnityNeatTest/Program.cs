using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using UnityNeatPlugin;

namespace UnityNeatTest
{
    /// <summary>
    /// Just for experimentation
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {

            TestPassiveNeat();

            Console.ReadLine();

        }
        public static void TestPassiveNeat()
        {
            NeatEvolutionAlgorithmParameters param = new NeatEvolutionAlgorithmParameters();
            NetworkActivationScheme activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(2);
            NeatGenomeParameters genomeParams = new NeatGenomeParameters();

            

            PassiveNeat neat = new PassiveNeat();
            neat.Init(2, 1, param, activationScheme, genomeParams, 1000);

            bool finished = false;

            

            //iterator over generations
            do
            {
                //either starts a new generation or randomly generates the first one if none exist
                neat.StartGeneration();

                
                foreach(NeatGenome curGenome in neat._GenomeList)
                {
                    if(curGenome.CachedPhenome == null)
                    {
                        //genome not yet decoded
                        curGenome.CachedPhenome = neat._GenomeDecoder.Decode(curGenome);
                    }

                    IBlackBox bb = (IBlackBox)curGenome.CachedPhenome;
                    double fitness = TestNetworkForXOR(bb);
                    if (fitness > 10)
                        finished = true;
                    curGenome.EvaluationInfo.SetFitness(fitness);
                }

                //calculate next generation
                neat.FinishGeneration();
            } while (finished == false);


            Console.WriteLine("Finished after generation " + neat._NeatAlgorithm.CurrentGeneration + " fitness " + neat._NeatAlgorithm.CurrentChampGenome.EvaluationInfo.MostRecentFitness);
        }

        private static double ToNeuron(double val, double min, double max)
        {
            double dif = max - min;
            double res = (val - min) / dif;
            return -1 + res*2;
        }

        private static double FromNeuron(double val, double min, double max)
        {
            double dif = max - min;
            double res = min + dif * val;
            return res;
        }


        public static double TestNetworkForXOR(IBlackBox box)
        {
            double fitness = 0;
            ISignalArray inputArr = box.InputSignalArray;
            ISignalArray outputArr = box.OutputSignalArray;
            double error;
            int correctResults = 0;

            // Set the input values
            inputArr[0] = 0.0;
            inputArr[1] = 0.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
                return 0;
            error = Math.Abs(0 - outputArr[0]);
            if (error < 0.5)
                correctResults++;
            fitness += 1 - error;

            // Set the input values
            inputArr[0] = 0.0;
            inputArr[1] = 1.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
                return 0;
            error = Math.Abs(1 - outputArr[0]);
            if (error < 0.5)
                correctResults++;
            fitness += 1 - error;

            // Set the input values
            inputArr[0] = 1.0;
            inputArr[1] = 0.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
                return 0;
            error = Math.Abs(1 - outputArr[0]);
            if (error < 0.5)
                correctResults++;
            fitness += 1 - error;

            // Set the input values
            inputArr[0] = 1.0;
            inputArr[1] = 1.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
                return 0;
            error = Math.Abs(0 - outputArr[0]);
            if (error < 0.5)
                correctResults++;
            fitness += 1 - error;


            if (correctResults == 4)
            {
                fitness += 10;
            }
            return fitness;
        }
    }
}
