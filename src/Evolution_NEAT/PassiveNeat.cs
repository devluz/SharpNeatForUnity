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

        private IGenomeDecoder<NeatGenome, IBlackBox> mGenomeDecoder;
        public IGenomeDecoder<NeatGenome, IBlackBox> _GenomeDecoder
        {
            get { return mGenomeDecoder; }
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

        public List<NeatGenome> _GenomeList
        {
            get { return mGenomeList; }
        }


        private NeatEvolutionAlgorithm<NeatGenome> mNeatAlgorithm;

        public NeatEvolutionAlgorithm<NeatGenome> _NeatAlgorithm
        {
            get { return mNeatAlgorithm; }
        }

        

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
            Init(inputCount, outputCount, popultionCount);
            //Start();
        }

        //public void InitAndRun(int inputCount,
        //                        int outputCount,
        //                        NeatEvolutionAlgorithmParameters param,
        //                        NetworkActivationScheme activationScheme,
        //                        NeatGenomeParameters genomeParams,
        //                        XmlReader reader)
        //{
        //    mParams = param;
        //    mGenomeParams = genomeParams;
        //    Thread t = new Thread(() =>
        //    {
        //        mEvaluator._State = State.Calculating;
        //        Init(inputCount, outputCount, reader);
        //        Start();
        //    });
        //    t.Start();
        //}


        private void Init(int inputCount, int outputCount, int populationCount)
        {
            mInputCount = inputCount;
            mOutputCount = outputCount;

            mGenomeDecoder = new NeatGenomeDecoder(mActivationScheme);


            // Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
            mGenomeFactory = new NeatGenomeFactory(mInputCount, mOutputCount, mGenomeParams);

            // Create an initial population of randomly generated genomes.
            mGenomeList = mGenomeFactory.CreateGenomeList(populationCount, 0);

            mNeatAlgorithm = CreateEvolutionAlgorithm();
        }
        private void Init(int inputCount, int outputCount, XmlReader xmlGenomeList)
        {
            mInputCount = inputCount;
            mOutputCount = outputCount;


            ReadGenomeList(xmlGenomeList);
            mNeatAlgorithm = CreateEvolutionAlgorithm();
            mNeatAlgorithm.Initialize(new PassiveListEvaluator(), mGenomeFactory, mGenomeList);
        }





        private void Start()
        {
            mNeatAlgorithm.StartContinue();

        }


        private void ReadGenomeList(XmlReader xmlGenomeList)
        {
            mGenomeFactory = (NeatGenomeFactory)new NeatGenomeFactory(mInputCount, mOutputCount, mGenomeParams);
            mGenomeList = NeatGenomeXmlIO.ReadCompleteGenomeList(xmlGenomeList, false, (NeatGenomeFactory)mGenomeFactory);

            xmlGenomeList.Close();

        }

        public bool NextStep()
        {
            //first step is the initialization and the first rating + creation of species
            if (mNeatAlgorithm.SpecieList == null)
            {
                mNeatAlgorithm.Initialize(new PassiveListEvaluator(), mGenomeFactory, mGenomeList);
            }
            else
            {
                ((PassiveNeatAlgorithm)mNeatAlgorithm).CalcNextGeneration();
            }
            return true;
        }


        private class PassiveNeatAlgorithm : NeatEvolutionAlgorithm<NeatGenome>
        {
            /// <summary>
            /// Constructs with the provided NeatEvolutionAlgorithmParameters and ISpeciationStrategy.
            /// </summary>
            public PassiveNeatAlgorithm(NeatEvolutionAlgorithmParameters eaParams,
                                        ISpeciationStrategy<NeatGenome> speciationStrategy,
                                        IComplexityRegulationStrategy complexityRegulationStrategy)
            : base(eaParams, speciationStrategy, complexityRegulationStrategy)
            {

            }

            public void CalcNextGeneration()
            {
                _currentGeneration++;
                this.PerformOneGeneration();
            }
        }


        /// <summary>
        /// Create and return a NeatEvolutionAlgorithm object ready for running the NEAT algorithm/search. Various sub-parts
        /// of the algorithm are also constructed and connected up.
        /// This overload accepts a pre-built genome population and their associated/parent genome factory.
        /// </summary>
        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm()
        {
            // Create distance metric. Mismatched genes have a fixed distance of 10; for matched genes the distance is their weigth difference.
            IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);

            //parallel won't work in dotnet 3.5
            //ISpeciationStrategy<NeatGenome> speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(distanceMetric, _parallelOptions);
            ISpeciationStrategy<NeatGenome> speciationStrategy = new KMeansClusteringStrategy<NeatGenome>(distanceMetric);

            // Create complexity regulation strategy.
            IComplexityRegulationStrategy complexityRegulationStrategy = new NullComplexityRegulationStrategy();

            // Create the evolution algorithm.
            NeatEvolutionAlgorithm<NeatGenome> ea = new PassiveNeatAlgorithm(mParams, speciationStrategy, complexityRegulationStrategy);


            // Create genome decoder.

            // Create a genome list evaluator. This packages up the genome decoder with the genome evaluator.
            //IGenomeListEvaluator<NeatGenome> innerEvaluator = new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator, _parallelOptions);
            
            //IGenomeListEvaluator<NeatGenome> innerEvaluator = new SerialGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator);
            //IGenomeListEvaluator<NeatGenome> innerEvaluator = new PassiveListEvaluator();



            //game isn't 100% deterministic yet so we have to turn this off to not prefer networks
            //that were lucky once!

            // Wrap the list evaluator in a 'selective' evaulator that will only evaluate new genomes. That is, we skip re-evaluating any genomes
            // that were in the population in previous generations (elite genomes). This is determined by examining each genome's evaluation info object.
            //IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(
            //                                                                        innerEvaluator,
            //                                                                        SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());
            // Initialize the evolution algorithm.
            


            // Finished. Return the evolution algorithm
            return ea;
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



        private class PassiveListEvaluator : IGenomeListEvaluator<NeatGenome>
        {
            private ulong mEvaluationCount = 0;
            public ulong EvaluationCount
            {
                get { return mEvaluationCount; }
            }

            public bool StopConditionSatisfied
            {
                get { return false; }
            }

            public void Evaluate(IList<NeatGenome> genomeList)
            {

                mEvaluationCount += (ulong)genomeList.Count;
            }

            public void Reset()
            {

            }
        }


    }
}
