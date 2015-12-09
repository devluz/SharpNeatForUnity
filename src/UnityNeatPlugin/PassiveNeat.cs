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

namespace UnityNeatPlugin
{

    /// <summary>
    /// Just a wrapper around Passive Neat Algorithm. 
    /// Might be not ready for general use yet. It was designed for a very narrow scenario.
    /// </summary>
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

        private IDistanceMetric mDistanceMetric;
        private ISpeciationStrategy<NeatGenome> mSpeciationStrategy;
        private IComplexityRegulationStrategy mComplexityRegulationStrategy;


        private PassiveNeatAlgorithm mNeatAlgorithm;

        public PassiveNeatAlgorithm _NeatAlgorithm
        {
            get { return mNeatAlgorithm; }
        }





        //
        /// <summary>
        /// Indicates that fitness isn't yet known for all genes if true.
        /// 
        /// If false all genes were evaluated and a new generation can be started.
        /// 
        /// This will be true after creation or after loading a file as the fitness will be unknown and only after
        /// the first evaluation there will be access to statistics
        /// </summary>
        private bool mInEvaluation = true;
        public bool _InEvaluation
        {
            get { return mInEvaluation; }
        }

        /// <summary>
        /// This will be false after restart until the first evaluation is complete
        /// the first FinishGeneration was called
        /// </summary>
        private bool mIsFullyIntialized = false;
        public bool _IsFullyIntialized
        {
            get
            {
                return mIsFullyIntialized;
            }
        }


        private bool mTmpEmptySpeciesFlag;
        private List<NeatGenome> mTmpOffspringList;



        public PassiveNeat()
        {

        }

        public void Init(int inputCount, int outputCount, NeatEvolutionAlgorithmParameters param,
                            NetworkActivationScheme activationScheme, NeatGenomeParameters genomeParams, int popultionCount)
        {
            mParams = param;
            mActivationScheme = activationScheme;
            mGenomeParams = genomeParams;
            mInputCount = inputCount;
            mOutputCount = outputCount;
            Init();
            InitGenome(popultionCount);
            mInEvaluation = true;
        }
        public void Init(int inputCount, int outputCount, NeatEvolutionAlgorithmParameters param,
                            NetworkActivationScheme activationScheme, NeatGenomeParameters genomeParams, XmlReader xmlGenomeList, uint startGeneration)
        {
            mParams = param;
            mActivationScheme = activationScheme;
            mGenomeParams = genomeParams;
            mInputCount = inputCount;
            mOutputCount = outputCount;

            Init(startGeneration);
            InitGenome(xmlGenomeList);
            mInEvaluation = true;

        }
        private void Init(uint startgeneration = 0)
        {
            // Create distance metric. Mismatched genes have a fixed distance of 10; for matched genes the distance is their weigth difference.
            mDistanceMetric = new ManhattanDistanceMetricNet35(1.0, 0.0, 10.0);

            //parallel won't work in dotnet 3.5
            //ISpeciationStrategy<NeatGenome> speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(distanceMetric, _parallelOptions);
            mSpeciationStrategy = new KMeansClusteringStrategy<NeatGenome>(mDistanceMetric);

            // Create complexity regulation strategy.
            mComplexityRegulationStrategy = new NullComplexityRegulationStrategy();


            mGenomeDecoder = new NeatGenomeDecoder(mActivationScheme);


            // Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
            mGenomeFactory = new NeatGenomeFactory(mInputCount, mOutputCount, mGenomeParams);


            mNeatAlgorithm = new PassiveNeatAlgorithm(mParams, mSpeciationStrategy, mComplexityRegulationStrategy, startgeneration);

        }

        private void InitGenome(int populationCount)
        {
            // Create an initial population of randomly generated genomes.
            mGenomeList = mGenomeFactory.CreateGenomeList(populationCount, 0);
        }


        private void InitGenome(XmlReader xmlGenomeList)
        {
            mGenomeList = NeatGenomeXmlIO.ReadCompleteGenomeList(xmlGenomeList, false, (NeatGenomeFactory)mGenomeFactory);
        }


        /// <summary>
        /// This will either create new offspring and prepare everything for evaluation if the algorithm is fully initialized or do nothing
        /// if not initialized (in this case the fitness need to be known first to initialize everythong properly
        /// </summary>
        public bool StartGeneration()
        {
            if (mNeatAlgorithm.SpecieList == null)
            {
                return false;
                //not yet fully initialized. wait until the first time FinishGeneration is called and the first generation is ready
            }
            else
            {
                mInEvaluation = true;
                this._NeatAlgorithm.IncreaseGenerationCounter();
                this._NeatAlgorithm.PerformOneGeneration_CreateOffspring(out mTmpEmptySpeciesFlag, out mTmpOffspringList);

                //this isn't doing anything in the current version
                this._NeatAlgorithm.PerformOneGeneration_StartEvaluation();
                return true;
            }
        }

        /// <summary>
        /// This can be called after all genes were evalulated. it will calculate species and update statistics
        /// (either by finishing the evaluation or by reinitializing the algorithm if a new file was loaded)
        /// </summary>
        public void FinishGeneration()
        {
            //initialize it first if this didn't happen yet
            if (mNeatAlgorithm.SpecieList == null)
            {
                //user evauluated the first generation. initialize the whole neat algoirthm now
                mNeatAlgorithm.Initialize(new PassiveListEvaluator(), mGenomeFactory, mGenomeList);
                mIsFullyIntialized = true;
            }
            else
            {
                this._NeatAlgorithm.PerformOneGeneration_AfterEvaluation(mTmpEmptySpeciesFlag, mTmpOffspringList);
            }
            mInEvaluation = false;
        }



        public class FitnessEvaluation
        {
            private PassiveNeat mNeat;
            private NeatGenome mGenome;

            public NeatGenome _Genome
            {
                get { return mGenome; }
                internal set { mGenome = value; }
            }

            public FitnessEvaluation(PassiveNeat neat)
            {
                mNeat = neat;
            }

            public IBlackBox Network
            {
                get
                {
                    IBlackBox phenome = (IBlackBox)mGenome.CachedPhenome;
                    if (phenome == null)
                    {   // Decode the phenome and store a ref against the genome.
                        phenome = (IBlackBox)mNeat._GenomeDecoder.Decode(mGenome);
                        mGenome.CachedPhenome = phenome;
                    }

                    //failed to create a black box? set fitess to 0
                    if (phenome == null)
                    {   // Non-viable genome.
                        mGenome.EvaluationInfo.SetFitness(0.0);
                        mGenome.EvaluationInfo.AuxFitnessArr = null;
                    }
                    return phenome;
                }
            }
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
