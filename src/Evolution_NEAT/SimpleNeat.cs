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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
namespace Evolution_NEAT
{


    public class SimpleNeat
    {



        //configuration //move later to constructor
        #region configuration 

        private int mInputCount = 2;
        private int mOutputCount = 1;
        private int mPopulationCount = 150;
        /// <summary>
        /// Activation scheme used in the generated genomes
        /// </summary>
        NetworkActivationScheme mActivationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(2);


        NeatEvolutionAlgorithmParameters _eaParams = new NeatEvolutionAlgorithmParameters();

        #endregion
        /// <summary>
        /// Tool to generate the genome list with the correct amout of in and output neurons
        /// </summary>
        IGenomeFactory<NeatGenome> mGenomeFactory;

        /// <summary>
        /// The genome list contains the "programming" of the neuronal networks
        /// </summary>
        List<NeatGenome> mGenomeList;


        NeatEvolutionAlgorithm<NeatGenome> _ea;



        public void Init(SimpleEvaluator evaluator)
        {
            mInputCount = evaluator.InputNeuronCount;
            mOutputCount = evaluator.OutputNeuronCount;
            mPopulationCount = evaluator.PopulationCount;
            //First step is we configurate the network and create the genomes that encode them
            GenerateGenomeList();
            // Create evolution algorithm and attach update event.
            _ea = CreateEvolutionAlgorithm(mGenomeFactory, mGenomeList, evaluator);
            _ea.UpdateEvent += new EventHandler(evaluator.UpdateEvent);
        }

        public void Start()
        {

            // Start algorithm (it will run on a background thread).
            _ea.StartContinue();
            Console.ReadLine();
        }

        private void GenerateGenomeList()
        {

            /// <summary>
            /// Paramters used to create the genome
            /// </summary>
            NeatGenomeParameters neatGenomeParams;




            neatGenomeParams = new NeatGenomeParameters();
            neatGenomeParams.FeedforwardOnly = mActivationScheme.AcyclicNetwork;

            // Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
            mGenomeFactory = new NeatGenomeFactory(mInputCount, mOutputCount, neatGenomeParams);

            // Create an initial population of randomly generated genomes.
            mGenomeList = mGenomeFactory.CreateGenomeList(mPopulationCount, 0);

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
            IComplexityRegulationStrategy complexityRegulationStrategy = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Absolute, 10);

            // Create the evolution algorithm.
            NeatEvolutionAlgorithm<NeatGenome> ea = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, speciationStrategy, complexityRegulationStrategy);


            // Create genome decoder.
            IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = new NeatGenomeDecoder(mActivationScheme);

            // Create a genome list evaluator. This packages up the genome decoder with the genome evaluator.
            //IGenomeListEvaluator<NeatGenome> innerEvaluator = new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator, _parallelOptions);
            IGenomeListEvaluator<NeatGenome> innerEvaluator = new SerialGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, evaluator);

            

            // Wrap the list evaluator in a 'selective' evaulator that will only evaluate new genomes. That is, we skip re-evaluating any genomes
            // that were in the population in previous generations (elite genomes). This is determined by examining each genome's evaluation info object.
            IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>(
                                                                                    innerEvaluator,
                                                                                    SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());
            // Initialize the evolution algorithm.
            ea.Initialize(selectiveEvaluator, genomeFactory, genomeList);

            // Finished. Return the evolution algorithm
            return ea;
        }


    }



    public abstract class SimpleEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        private int mInputCount;

        public int InputNeuronCount
        {
            get { return mInputCount; }
        }
        private int mOutputCount;

        public int OutputNeuronCount
        {
            get { return mOutputCount; }
        }
        private int mPopulationCount;

        public int PopulationCount
        {
            get { return mPopulationCount; }
        }



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
        public SimpleEvaluator(int inputCount, int outputCount, int populationCount)
        {
            mInputCount = inputCount;
            mOutputCount = outputCount;
            mPopulationCount = populationCount;
        }




        public FitnessInfo Evaluate(IBlackBox box)
        {
            mEvalCount++;

            double fitness = TestNetwork(box);


            //double fitness = TestNetwork(box);
            return new FitnessInfo(fitness, fitness);
        }
        public virtual void Reset()
        {
        }

        public virtual void UpdateEvent(object sender, EventArgs e)
        {
            NeatEvolutionAlgorithm<NeatGenome> _ea = (NeatEvolutionAlgorithm<NeatGenome>)sender;
            Console.WriteLine(string.Format("gen={0:N0} bestFitness={1:N6}", _ea.CurrentGeneration, _ea.Statistics._maxFitness));
        }

        abstract public double TestNetwork(IBlackBox box);
    }

}
