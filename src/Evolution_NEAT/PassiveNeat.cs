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

        private IDistanceMetric mDistanceMetric;
        private ISpeciationStrategy<NeatGenome> mSpeciationStrategy;
        private IComplexityRegulationStrategy mComplexityRegulationStrategy;


        private PassiveNeatAlgorithm mNeatAlgorithm;

        public PassiveNeatAlgorithm _NeatAlgorithm
        {
            get { return mNeatAlgorithm; }
        }



        private FitnessEvaluationEnumerator mReuseEnumerator;

        //temporary variables stored during shift to the next generation
        private bool mInGenerationShift = false;
        public bool _InGenerationShift
        {
            get { return mInGenerationShift; }
        }
        private bool mTmpEmptySpeciesFlag;
        private List<NeatGenome> mTmpOffspringList;

        public PassiveNeat()
        {
            mReuseEnumerator = new FitnessEvaluationEnumerator(this);
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
        }
        public void Init(int inputCount, int outputCount, NeatEvolutionAlgorithmParameters param,
                            NetworkActivationScheme activationScheme, NeatGenomeParameters genomeParams, XmlReader xmlGenomeList)
        {
            mParams = param;
            mActivationScheme = activationScheme;
            mGenomeParams = genomeParams;
            mInputCount = inputCount;
            mOutputCount = outputCount;
            Init();
            InitGenome(xmlGenomeList);
        }
        private void Init()
        {
            // Create distance metric. Mismatched genes have a fixed distance of 10; for matched genes the distance is their weigth difference.
            mDistanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);

            //parallel won't work in dotnet 3.5
            //ISpeciationStrategy<NeatGenome> speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(distanceMetric, _parallelOptions);
            mSpeciationStrategy = new KMeansClusteringStrategy<NeatGenome>(mDistanceMetric);

            // Create complexity regulation strategy.
            mComplexityRegulationStrategy = new NullComplexityRegulationStrategy();


            mGenomeDecoder = new NeatGenomeDecoder(mActivationScheme);


            // Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
            mGenomeFactory = new NeatGenomeFactory(mInputCount, mOutputCount, mGenomeParams);


            mNeatAlgorithm = new PassiveNeatAlgorithm(mParams, mSpeciationStrategy, mComplexityRegulationStrategy);

        }

        private void InitGenome(int populationCount)
        {
            // Create an initial population of randomly generated genomes.
            mGenomeList = mGenomeFactory.CreateGenomeList(populationCount, 0);
        }


        private void InitGenome(XmlReader xmlGenomeList)
        {
            mGenomeList = NeatGenomeXmlIO.ReadCompleteGenomeList(xmlGenomeList, false, (NeatGenomeFactory)mGenomeFactory);
            xmlGenomeList.Close();
        }



        public void StartGeneration()
        {
            if (mNeatAlgorithm.SpecieList == null)
            {
                //not yet fully initialized. wait until the first time FinishGeneration is called and the first generation is ready
            }
            else
            {
                mInGenerationShift = true;
                this._NeatAlgorithm.IncreaseGenerationCounter();
                this._NeatAlgorithm.PerformOneGeneration_BeforeEvaluation(out mTmpEmptySpeciesFlag, out mTmpOffspringList);
                this._NeatAlgorithm.PerformOneGeneration_Evaluation();
            }
        }

        public void FinishGeneration()
        {

            if (mNeatAlgorithm.SpecieList == null)
            {
                //user evauluated the first generation. initialize the whole neat algoirthm now
                mNeatAlgorithm.Initialize(new PassiveListEvaluator(), mGenomeFactory, mGenomeList);
            }
            else
            {
                this._NeatAlgorithm.PerformOneGeneration_AfterEvaluation(mTmpEmptySpeciesFlag, mTmpOffspringList);
                mInGenerationShift = false;
            }
        }
        //public bool NextGeneration()
        //{
        //    //first step is the initialization and the first rating + creation of species
        //    if (mNeatAlgorithm.SpecieList == null)
        //    {
        //        mNeatAlgorithm.Initialize(new PassiveListEvaluator(), mGenomeFactory, mGenomeList);
        //    }
        //    else
        //    {
        //        ((PassiveNeatAlgorithm)mNeatAlgorithm).CalcNextGeneration();
        //    }
        //    return true;
        //}

        /// <summary>
        /// This will return an emumerator for all Genomes + allow easy cached access to the neuronal networks for fitness evaluation
        /// 
        /// There is only one instance! This will reset all other enumerators return by this property as well!
        /// </summary>
        public FitnessEvaluationEnumerator GetReusableFitnessEnumerator()
        {
            mReuseEnumerator.Reset();
            return mReuseEnumerator;
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

        public class FitnessEvaluationEnumerator : IEnumerator<FitnessEvaluation>
        {
            private PassiveNeat mNeat;
            private int mIndex = -1;

            public int _Index
            {
                get { return mIndex; }
            }

            private FitnessEvaluation mCurrentEvaluation; //reference will be reused

            public FitnessEvaluation Current
            {
                get
                {
                    if (IsInBoundry() == false)
                        return null;
                    return mCurrentEvaluation;
                }
            }

            public FitnessEvaluationEnumerator(PassiveNeat neat)
            {
                mNeat = neat;
                mCurrentEvaluation = new FitnessEvaluation(mNeat);
            }

            private bool IsInBoundry()
            {
                if (mIndex < 0 || mIndex >= mNeat._GenomeList.Count)
                    return false;
                return true;
            }

            public void Dispose()
            {
                //we don't use any resources
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                mIndex++;
                if (IsInBoundry() == false)
                    return false;
                mCurrentEvaluation._Genome = mNeat._GenomeList[mIndex];
                return true;
            }

            public void Reset()
            {
                mIndex = -1;
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
