using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using SharpNeat.SpeciationStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace Evolution_NEAT
{
    public class PassiveNeat
    {


        //configuration //move later to constructor
        #region configuration

        private int mInputCount = 2;
        private int mOutputCount = 1;

        /// <summary>
        /// Activation scheme used in the generated genomes
        /// </summary>
        private NetworkActivationScheme mActivationScheme = null;


        private NeatEvolutionAlgorithmParameters mParams = null;
        public NeatEvolutionAlgorithmParameters _Params
        {
            get { return mParams; }
            set { mParams = value; }
        }

        private NeatGenomeParameters mGenomeParams = null;
        public NeatGenomeParameters _GenomeParams
        {
            get { return mGenomeParams; }
            set { mGenomeParams = value; }
        }

        #endregion
        /// <summary>
        /// Tool to generate the genome list with the correct amout of in and output neurons
        /// </summary>
        private IGenomeFactory<NeatGenome> mGenomeFactory;

        /// <summary>
        /// The genome list contains the "programming" of the neuronal networks
        /// </summary>
        private List<NeatGenome> mGenomeList;


        private NeatEvolutionAlgorithm<NeatGenome> mNeatAlgorithm;

        public NeatEvolutionAlgorithm<NeatGenome> NeatAlgorithm
        {
            get { return mNeatAlgorithm; }
        }

        private EnumEvaluator mEvaluator = new EnumEvaluator();

        public PassiveNeat()
        {

        }

        public void InitAndRun(int inputCount,
                                int outputCount,
                                NeatEvolutionAlgorithmParameters param,
                                NetworkActivationScheme activationScheme,
                                NeatGenomeParameters genomeParams,
                                int popultionCount)
        {
            mParams = param;
            mActivationScheme = activationScheme;
            mGenomeParams = genomeParams;
            Thread t = new Thread(() =>
            {
                mEvaluator._State = State.Calculating;
                Init(inputCount, outputCount, popultionCount);
                Start();
            });
            t.Start();
        }

        public void InitAndRun(int inputCount,
                                int outputCount,
                                NeatEvolutionAlgorithmParameters param,
                                NetworkActivationScheme activationScheme,
                                NeatGenomeParameters genomeParams,
                                XmlReader reader)
        {
            mParams = param;
            mGenomeParams = genomeParams;
            Thread t = new Thread(() =>
            {
                mEvaluator._State = State.Calculating;
                Init(inputCount, outputCount, reader);
                Start();
            });
            t.Start();
        }


        private void Init(int inputCount, int outputCount, int popultionCount)
        {
            mInputCount = inputCount;
            mOutputCount = outputCount;



            //First step is we configurate the network and create the genomes that encode them
            GenerateGenomeList(popultionCount);
            InitAlgorithm();
        }
        private void Init(int inputCount, int outputCount, XmlReader xmlGenomeList)
        {
            mInputCount = inputCount;
            mOutputCount = outputCount;


            ReadGenomeList(xmlGenomeList);
            InitAlgorithm();
        }
        private void InitAlgorithm()
        {

            // Create evolution algorithm and attach update event.
            mNeatAlgorithm = CreateEvolutionAlgorithm(mGenomeFactory, mGenomeList, mEvaluator);
            mNeatAlgorithm.UpdateEvent += UpdateEvent;

            mNeatAlgorithm.UpdateScheme = new UpdateScheme(1);
        }


        private void UpdateEvent(object sender, EventArgs args)
        {
            mEvaluator.OnGenerationFinished();
        }

        private void Start()
        {

            // Start algorithm (it will run on a background thread).
            mNeatAlgorithm.StartContinue();

        }

        private void GenerateGenomeList(int populationCount)
        {
            // Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
            mGenomeFactory = new NeatGenomeFactory(mInputCount, mOutputCount, mGenomeParams);

            // Create an initial population of randomly generated genomes.
            mGenomeList = mGenomeFactory.CreateGenomeList(populationCount, 0);
        }
        private void ReadGenomeList(XmlReader xmlGenomeList)
        {
            mGenomeFactory = (NeatGenomeFactory)new NeatGenomeFactory(mInputCount, mOutputCount, mGenomeParams);
            mGenomeList = NeatGenomeXmlIO.ReadCompleteGenomeList(xmlGenomeList, false, (NeatGenomeFactory)mGenomeFactory);

            xmlGenomeList.Close();

        }

        public void NextStep()
        {
            throw new NotImplementedException();
            //mNeatAlgorithm.CalcNextGeneration();
        }


        /// <summary>
        /// Create and return a NeatEvolutionAlgorithm object ready for running the NEAT algorithm/search. Various sub-parts
        /// of the algorithm are also constructed and connected up.
        /// This overload accepts a pre-built genome population and their associated/parent genome factory.
        /// </summary>
        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(IGenomeFactory<NeatGenome> genomeFactory, List<NeatGenome> genomeList, IPhenomeEvaluator<IBlackBox> evaluator)
        {
            // Create distance metric. Mismatched genes have a fixed distance of 10; for matched genes the distance is their weigth difference.
            IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);

            //parallel won't work in dotnet 3.5
            //ISpeciationStrategy<NeatGenome> speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(distanceMetric, _parallelOptions);
            ISpeciationStrategy<NeatGenome> speciationStrategy = new KMeansClusteringStrategy<NeatGenome>(distanceMetric);

            // Create complexity regulation strategy.
            IComplexityRegulationStrategy complexityRegulationStrategy = new NullComplexityRegulationStrategy();

            // Create the evolution algorithm.
            NeatEvolutionAlgorithm<NeatGenome> ea = new NeatEvolutionAlgorithm<NeatGenome>(mParams, speciationStrategy, complexityRegulationStrategy);


            // Create genome decoder.
            IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = new NeatGenomeDecoder(mActivationScheme);

            // Create a genome list evaluator. This packages up the genome decoder with the genome evaluator.
            //IGenomeListEvaluator<NeatGenome> innerEvaluator = new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator, _parallelOptions);
            IGenomeListEvaluator<NeatGenome> innerEvaluator = new SerialGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator);




            //game isn't 100% deterministic yet so we have to turn this off to not prefer networks
            //that were lucky once!

            // Wrap the list evaluator in a 'selective' evaulator that will only evaluate new genomes. That is, we skip re-evaluating any genomes
            // that were in the population in previous generations (elite genomes). This is determined by examining each genome's evaluation info object.
            //IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(
            //                                                                        innerEvaluator,
            //                                                                        SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());
            // Initialize the evolution algorithm.
            ea.Initialize(innerEvaluator, genomeFactory, genomeList);

            // Finished. Return the evolution algorithm
            return ea;
        }


        public bool WaitForEvaluation(out EvaluationTask task)
        {
            return mEvaluator.WaitForEvaluation(out task);
        }
        public void Continue(EvaluationTask task)
        {
            mEvaluator.Continue(task);
        }
        public void LeaveAfterGeneration()
        {
            mEvaluator.Finish();
        }


        public struct EvaluationTask
        {
            public IBlackBox mNetwork;
            public double resultingFitness;
        }

        public enum State
        {
            Uninitialized,
            Calculating,
            WaitForEvaluationBegin,
            WaitForEvaluationFinished,
            ShutDown,
            Error
        }

        public State _State
        {
            get
            {

                return mEvaluator._State;
            }
        }
        private class EnumEvaluator : IPhenomeEvaluator<IBlackBox>
        {

            private State mState = State.Uninitialized;

            public State _State
            {
                get { return mState; }
                set { mState = value; }
            }

            private EvaluationTask mCurrentTask;

            //used to wait for outside thread for the evaluator. will be released if the evaluation can begin
            private AutoResetEvent mWaitForEvaluationBegin = new AutoResetEvent(false);

            //Allows this thread to wait until the outside finished the evaluation
            private AutoResetEvent mWaitForEvaluationEnded = new AutoResetEvent(false);





            ulong mEvalCount = 0;
            public ulong EvaluationCount
            {
                get { return mEvalCount; }
            }

            private bool isStopConditionSatisfied;
            public bool StopConditionSatisfied
            {
                get { return isStopConditionSatisfied; }
            }


            public void Finish()
            {
                isStopConditionSatisfied = true;
            }



            public bool WaitForEvaluation(out EvaluationTask task)
            {
                do
                {
                    // didn't get a task yet. time to give up?
                    if (mState == State.ShutDown)
                    {
                        task = new EvaluationTask();
                        return false;
                    }
                }
                while (mWaitForEvaluationBegin.WaitOne(50) == false);

                if (mState != State.WaitForEvaluationBegin)
                {
                    mState = State.Error;
                    throw new InvalidOperationException("Threads out of sync. State: " + mState + " instead of " + State.WaitForEvaluationBegin);
                }
                //Console.WriteLine("WaitForEvaluationFinished");
                mState = State.WaitForEvaluationFinished;
                task = mCurrentTask;
                return true;
            }


            public void Continue(EvaluationTask task)
            {

                if (mState != State.WaitForEvaluationFinished)
                {
                    mState = State.Error;
                    throw new InvalidOperationException("Threads out of sync. State: " + mState + " instead of " + State.WaitForEvaluationFinished);
                }

                mCurrentTask = task;
                //Console.WriteLine("Calculating");
                mState = State.Calculating;
                mWaitForEvaluationEnded.Set();
            }

            public void OnGenerationFinished()
            {
                if (isStopConditionSatisfied)
                {
                    if (mState != State.Calculating)
                    {
                        mState = State.Error;
                        throw new InvalidOperationException("Threads out of sync. State: " + mState + " instead of " + State.WaitForEvaluationFinished);
                    }
                    mState = State.ShutDown;
                }
            }
            public FitnessInfo Evaluate(IBlackBox box)
            {
                mEvalCount++;

                mCurrentTask = new EvaluationTask();
                mCurrentTask.mNetwork = box;


                if (mState != State.Calculating)
                {
                    mState = State.Error;
                    throw new InvalidOperationException("Threads out of sync. State: " + mState + " instead of " + State.Calculating);
                }
                //Console.WriteLine("WaitForEvaluationBegin");
                mState = State.WaitForEvaluationBegin;
                mWaitForEvaluationBegin.Set();
                mWaitForEvaluationEnded.WaitOne();


                //double fitness = TestNetwork(box);
                return new FitnessInfo(mCurrentTask.resultingFitness, mCurrentTask.resultingFitness);
            }
            public virtual void Reset()
            {
            }
        }


    }
}
