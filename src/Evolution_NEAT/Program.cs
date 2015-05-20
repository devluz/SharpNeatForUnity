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

namespace Evolution_NEAT
{
    public class Program
    {
        static void Main(string[] args)
        {
            TestPassiveNeat();

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
            neat.InitAndRun(2, 1, param, activationScheme, genomeParams, 150);

            //neat.Init(2, 1, 150, evaluator);

            PassiveNeat.EvaluationTask task;
            while (neat.WaitForEvaluation(out task))
            {
                task.resultingFitness = TestNetworkForXOR(task.mNetwork);
                if (task.resultingFitness > 10)
                {
                    neat.LeaveAfterGeneration();
                    Console.WriteLine("Found solution. Fitness " + task.resultingFitness);
                }
                neat.Continue(task);

            }
            Console.WriteLine("Finished after generation " + neat.NeatAlgorithm.CurrentGeneration);
            Console.ReadLine();
        }

        public static void TestEnumNeat()
        {
            NeatEvolutionAlgorithmParameters param = new NeatEvolutionAlgorithmParameters();
            NetworkActivationScheme activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(2);
            NeatGenomeParameters genomeParams = new NeatGenomeParameters();

            //SimpleXorEvaluator evaluator = new SimpleXorEvaluator();
            // Create IBlackBox evaluator.
            //XorBlackBoxEvaluator evaluator = new XorBlackBoxEvaluator();
            EnumerableNeat neat = new EnumerableNeat();
            neat.InitAndRun(2, 1, param, activationScheme, genomeParams, 150);

            //neat.Init(2, 1, 150, evaluator);

            EnumerableNeat.EvaluationTask task;
            while (neat.WaitForEvaluation(out task))
            {
                task.resultingFitness = TestNetworkForXOR(task.mNetwork);
                if (task.resultingFitness > 10)
                {
                    neat.LeaveAfterGeneration();
                    Console.WriteLine("Found solution. Fitness " + task.resultingFitness);
                }
                neat.Continue(task);
                
            }
            Console.WriteLine("Finished after generation " + neat.NeatAlgorithm.CurrentGeneration);
            Console.ReadLine();
        }
        public static void TestSimpleNeat()
        {

            SimpleXorEvaluator evaluator = new SimpleXorEvaluator();
            // Create IBlackBox evaluator.
            SimpleNeat neat = new SimpleNeat();
            neat.Init(evaluator);

            neat.Start();

            Console.WriteLine("Wait");
            Console.ReadLine();
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



    public class SimpleXorEvaluator : SimpleEvaluator
    {
        public const int InputCount = 2;
        public const int OutputCount = 1;
        public const int Population = 150;

        public SimpleXorEvaluator()
            : base(InputCount, OutputCount, Population)
        {
            
        }
        public override double TestNetwork(IBlackBox box)
        {
            double fitness = Program.TestNetworkForXOR(box);
            if (fitness > 10)
                Finish();
            return fitness;
        }
    }

    public class XorBlackBoxEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        const double StopFitness = 10.0;
        ulong _evalCount;
        bool _stopConditionSatisfied;

        #region IPhenomeEvaluator<IBlackBox> Members

        /// <summary>
        /// Gets the total number of evaluations that have been performed.
        /// </summary>
        public ulong EvaluationCount
        {
            get { return _evalCount; }
        }

        /// <summary>
        /// Gets a value indicating whether some goal fitness has been achieved and that
        /// the the evolutionary algorithm/search should stop. This property's value can remain false
        /// to allow the algorithm to run indefinitely.
        /// </summary>
        public bool StopConditionSatisfied
        {
            get { return _stopConditionSatisfied; }
        }

        /// <summary>
        /// Evaluate the provided IBlackBox against the XOR problem domain and return its fitness score.
        /// </summary>
        public FitnessInfo Evaluate(IBlackBox box)
        {
            double fitness = 0;
            double output;
            double pass = 1.0;
            ISignalArray inputArr = box.InputSignalArray;
            ISignalArray outputArr = box.OutputSignalArray;

            _evalCount++;

            //----- Test 0,0
            box.ResetState();

            // Set the input values
            inputArr[0] = 0.0;
            inputArr[1] = 0.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = outputArr[0];
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            fitness += 1.0 - output; // Use this line to punish absolute error instead of squared error.
            //fitness += 1.0 - (output * output);
            if (output > 0.5)
            {
                pass = 0.0;
            }

            //----- Test 1,1
            // Reset any black box state from the previous test case.
            box.ResetState();

            // Set the input values
            inputArr[0] = 1.0;
            inputArr[1] = 1.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = outputArr[0];
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            fitness += 1.0 - output; // Use this line to punish absolute error instead of squared error.
            //fitness += 1.0 - (output * output);
            if (output > 0.5)
            {
                pass = 0.0;
            }

            //----- Test 0,1
            // Reset any black box state from the previous test case.
            box.ResetState();

            // Set the input values
            inputArr[0] = 0.0;
            inputArr[1] = 1.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = outputArr[0];
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            fitness += output; // Use this line to punish absolute error instead of squared error.
            //fitness += 1.0 - ((1.0 - output) * (1.0 - output));
            if (output <= 0.5)
            {
                pass = 0.0;
            }

            //----- Test 1,0
            // Reset any black box state from the previous test case.
            box.ResetState();

            // Set the input values
            inputArr[0] = 1.0;
            inputArr[1] = 0.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = outputArr[0];
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            fitness += output; // Use this line to punish absolute error instead of squared error.
            //fitness += 1.0 - ((1.0 - output) * (1.0 - output));
            if (output <= 0.5)
            {
                pass = 0.0;
            }

            // If all four outputs were correct, that is, all four were on the correct side of the
            // threshold level - then we add 10 to the fitness.
            fitness += pass * 10.0;

            if (fitness >= StopFitness)
            {
                _stopConditionSatisfied = true;
            }
            //Console.WriteLine(fitness + " count " + i);
            i++;
            return new FitnessInfo(fitness, fitness);
        }
        int i = 0;
        /// <summary>
        /// Reset the internal state of the evaluation scheme if any exists.
        /// Note. The XOR problem domain has no internal state. This method does nothing.
        /// </summary>
        public void Reset()
        {
        }

        #endregion
    }
}
