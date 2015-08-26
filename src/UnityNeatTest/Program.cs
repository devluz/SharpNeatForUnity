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

namespace Evolution_NEAT
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Enum:");
            //for (int i = 0; i < 10; i++)
            //{
            //    TestEnumNeat();
            //}



            //Console.WriteLine("TestPassiveNeat:");
            //for (int i = 0; i < 10; i++ )
            //{
            //    TestPassiveNeat();
            //}
            //TestPassiveNeat();
            TestAdd();
            Console.ReadLine();

        }
        public static void TestPassiveNeat()
        {
            NeatEvolutionAlgorithmParameters param = new NeatEvolutionAlgorithmParameters();
            NetworkActivationScheme activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(2);
            NeatGenomeParameters genomeParams = new NeatGenomeParameters();

            //SimpleXorEvaluator evaluator = new SimpleXorEvaluator();
            // Create IBlackBox evaluator.
            //XorBlackBoxEvaluator evaluator = new XorBlackBoxEvaluator();
            PassiveNeat neat = new PassiveNeat();
            neat.Init(2, 1, param, activationScheme, genomeParams, 1000);

            bool finished = false;

            //iterator over generations
            do
            {
                neat.StartGeneration();

                //now iterating over every gene to test it
                PassiveNeat.FitnessEvaluationEnumerator en = neat.GetReusableFitnessEnumerator();

                while(en.MoveNext())
                {
                    double fitness = TestNetworkForXOR(en.Current.Network);
                    if (fitness > 10)
                        finished = true;
                    en.Current._Genome.EvaluationInfo.SetFitness(fitness);
                }

                //calculate next generation
                neat.FinishGeneration();
            } while (finished == false);
            //neat.Init(2, 1, 150, evaluator);

            //iterate now



            Console.WriteLine("Finished after generation " + neat._NeatAlgorithm.CurrentGeneration + " fitness " + neat._NeatAlgorithm.CurrentChampGenome.EvaluationInfo.MostRecentFitness);
        }
        public static void TestAdd()
        {
            NeatEvolutionAlgorithmParameters param = new NeatEvolutionAlgorithmParameters();
            NetworkActivationScheme activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(2);
            NeatGenomeParameters genomeParams = new NeatGenomeParameters();

            //SimpleXorEvaluator evaluator = new SimpleXorEvaluator();
            // Create IBlackBox evaluator.
            //XorBlackBoxEvaluator evaluator = new XorBlackBoxEvaluator();
            PassiveNeat neat = new PassiveNeat();
            neat.Init(2, 1, param, activationScheme, genomeParams, 100);

            bool finished = false;

            //iterator over generations
            do
            {
                neat.StartGeneration();

                //now iterating over every gene to test it
                PassiveNeat.FitnessEvaluationEnumerator en = neat.GetReusableFitnessEnumerator();
                Random rnd = new Random(0);

                IBlackBox best = null;
                double bestFitness = 0;
                while (en.MoveNext())
                {
                    IBlackBox box = en.Current.Network;
                    double fitness = 0;
                    ISignalArray inputArr = box.InputSignalArray;
                    ISignalArray outputArr = box.OutputSignalArray;
                    double error;
                    int correctResults = 0;

                    int loop = 50;
                    //if (neat._NeatAlgorithm != null)
                    //    loop = loop + (int)neat._NeatAlgorithm.Statistics._generation;
                    for (int i = 0; i < loop; i++)
                    {
                        const double inMin = 0;
                        const double inMax = 100;
                        const double outMin = 0;
                        const double outMax = 200;

                        double a = rnd.NextDouble() * inMax;
                        double b = rnd.NextDouble() * inMax;
                        // Set the input values
                        inputArr[0] = ToNeuron(a, inMin, inMax);
                        inputArr[1] = ToNeuron(b, inMin, inMax);

                        // Activate the black box.
                        box.Activate();
                        if (!box.IsStateValid)
                        {
                            fitness = 0;
                        }
                        else
                        {
                            double expected = (a + b);
                            double result = FromNeuron(outputArr[0], outMin, outMax);
                            error = Math.Abs(expected - result);

                            if (error < 0.05 * outMax)
                                error = 0;
                            fitness += (1.0 / (1.0 + error)) / loop;
                        }
                    }

                    en.Current._Genome.EvaluationInfo.SetFitness(fitness);

                    if(best == null || bestFitness < fitness)
                    {
                        bestFitness = fitness;
                        best = box;
                    }
                }

                //{
                //    IBlackBox box = best;
                //    double fitness = 0;
                //    ISignalArray inputArr = box.InputSignalArray;
                //    ISignalArray outputArr = box.OutputSignalArray;
                //    double error;
                //    int correctResults = 0;

                //    int loop = 50;
                //    //if (neat._NeatAlgorithm != null)
                //    //    loop = loop + (int)neat._NeatAlgorithm.Statistics._generation;
                //    for (int i = 0; i < loop; i++)
                //    {
                //        double a = rnd.NextDouble();
                //        double b = rnd.NextDouble();
                //        // Set the input values
                //        inputArr[0] = a;
                //        inputArr[1] = b;

                //        // Activate the black box.
                //        box.Activate();
                //        if (!box.IsStateValid)
                //        {
                //            fitness = 0;
                //        }
                //        else
                //        {
                //            double expected = (a + b);
                //            double result = outputArr[0];
                //            error = Math.Abs(expected - result);
                //            if (error < 0.05)
                //                error = 0;
                //            fitness += (1.0 / (1.0 + error)) / loop;
                //        }
                //    }
                //}

                //calculate next generation
                neat.FinishGeneration();
                Console.WriteLine("gen"+ neat._NeatAlgorithm.CurrentGeneration+" mean" + neat._NeatAlgorithm.Statistics._meanFitness + "best:" + neat._NeatAlgorithm.Statistics._maxFitness);



            } while (finished == false);
            //neat.Init(2, 1, 150, evaluator);

            //iterate now



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

        public static void TestMemory()
        {
            NeatEvolutionAlgorithmParameters param = new NeatEvolutionAlgorithmParameters();
            NetworkActivationScheme activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(1);
            NeatGenomeParameters genomeParams = new NeatGenomeParameters();

            //SimpleXorEvaluator evaluator = new SimpleXorEvaluator();
            // Create IBlackBox evaluator.
            //XorBlackBoxEvaluator evaluator = new XorBlackBoxEvaluator();
            PassiveNeat neat = new PassiveNeat();
            neat.Init(1, 1, param, activationScheme, genomeParams, 150);

            
            bool finished = false;

            //iterator over generations
            do
            {
                neat.StartGeneration();

                //now iterating over every gene to test it
                PassiveNeat.FitnessEvaluationEnumerator en = neat.GetReusableFitnessEnumerator();
                Random r = new Random(0);
                while (en.MoveNext())
                {
                    IBlackBox box = en.Current.Network;
                    ISignalArray inputArr = box.InputSignalArray;
                    ISignalArray outputArr = box.OutputSignalArray;
                    double fitness = 0;
                    double firstVal = r.NextDouble();
                    double secVal = r.NextDouble();

                    inputArr[0] = firstVal;
                    box.Activate();
                    for(int i = 0; i < 100; i++)
                    {
                        inputArr[0] = secVal;
                        box.Activate();
                        if (!box.IsStateValid)
                        {
                            fitness = 0;
                            break;
                        }
                        double error = Math.Abs(firstVal - outputArr[0]);
                        if (error < 0.05f)
                            error = 0;
                        fitness += (1 - error);
                        firstVal = secVal;
                        secVal = r.NextDouble();
                    }
                    en.Current._Genome.EvaluationInfo.SetFitness(fitness);
                }

                //calculate next generation
                neat.FinishGeneration();
                Console.WriteLine(neat._NeatAlgorithm.Statistics._maxFitness);
            } while (finished == false);
            //neat.Init(2, 1, 150, evaluator);

            //iterate now



            Console.WriteLine("Finished after generation " + neat._NeatAlgorithm.CurrentGeneration + " fitness " + neat._NeatAlgorithm.CurrentChampGenome.EvaluationInfo.MostRecentFitness);
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
