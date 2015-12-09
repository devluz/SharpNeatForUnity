using SharpNeat.Decoders.Neat;
using SharpNeat.Domains;
using SharpNeat.Genomes.Neat;
using SharpNeatGUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;
using SharpNeat.Decoders;
using SharpNeat.Genomes.Neat;



    /// <summary>
    /// Manages the neuronal network with the SimpleNeatBrain2 stategy.
    /// 
    /// 
    /// 
    /// </summary>
    public class SimpleNeatBrain3
    {
        public const int COUNT_ENEMY_SENSOR = 4;
        public const int COUNT_FOOD_SENSOR = 2;
        public const float DISTANCE_MAX = 40.0f;



        public const string NAME = "SimpleNeatBrain3";
        public const int COUNT_INPUT = 2 + COUNT_ENEMY_SENSOR * 3 + COUNT_FOOD_SENSOR * 3;
        public const int COUNT_OUTPUT = 2;


        public static readonly NetworkActivationScheme sActivationScheme;
        public static readonly NeatGenomeParameters sGenomeParams;

        static SimpleNeatBrain3()
        {
            sActivationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(2);
            sGenomeParams = new NeatGenomeParameters();
            sGenomeParams.FeedforwardOnly = sActivationScheme.AcyclicNetwork;
            sGenomeParams.InitialInterconnectionsProportion = 0.05;
            sGenomeParams.ConnectionWeightRange = 5;
            sGenomeParams.FitnessHistoryLength = 10;

        }

        public int InputCount
        {
            get
            {
                return COUNT_INPUT;
            }
        }
        public int OutputCount
        {
            get
            {
                return COUNT_OUTPUT;
            }
        }



        public NetworkActivationScheme ActivationScheme
        {
            get { return sActivationScheme; }
        }

        public NeatGenomeParameters GenomeParams
        {
            get { return sGenomeParams; }
        }

    }

    public class NeatBrain4
    {
        public const int COUNT_SELF_SENSOR = 3;
        public const int COUNT_ENEMY_SENSOR = 2;
        public const int COUNT_FOOD_SENSOR = 2;
        public const float DISTANCE_MAX = 40.0f;



        public const string NAME = "NeatBrain4";
        public const int COUNT_INPUT = COUNT_SELF_SENSOR + COUNT_ENEMY_SENSOR * 3 + COUNT_FOOD_SENSOR * 3;
        public const int COUNT_OUTPUT = 3;


        public static readonly NetworkActivationScheme sActivationScheme;
        public static readonly NeatGenomeParameters sGenomeParams;

        static NeatBrain4()
        {
            sActivationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(2);
            sGenomeParams = new NeatGenomeParameters();
            sGenomeParams.FeedforwardOnly = sActivationScheme.AcyclicNetwork;
            sGenomeParams.InitialInterconnectionsProportion = 0.05;
            sGenomeParams.ConnectionWeightRange = 5;
            sGenomeParams.FitnessHistoryLength = 10;

        }

        public int InputCount
        {
            get
            {
                return COUNT_INPUT;
            }
        }
        public int OutputCount
        {
            get
            {
                return COUNT_OUTPUT;
            }
        }



        public NetworkActivationScheme ActivationScheme
        {
            get { return sActivationScheme; }
        }

        public NeatGenomeParameters GenomeParams
        {
            get { return sGenomeParams; }
        }

    }

    static class Program
    {
        public static NeatGenome CreateNeatBrainFromFile(string path)
        {
            string xmldata = File.ReadAllText(path);
            return CreateNeatBrainFromString(xmldata);
        }
        public static NeatGenome CreateNeatBrainFromString(string xmldata)
        {
            //string xmldata = @"<?xml version=""1.0"" encoding=""utf-8""?><Root><ActivationFunctions><Fn id=""0"" name=""SteepenedSigmoid"" prob=""1"" /></ActivationFunctions><Networks><Network id=""149540"" birthGen=""44"" fitness=""800429.1095676088""><Nodes><Node type=""bias"" id=""0"" fnId=""0"" /><Node type=""in"" id=""1"" fnId=""0"" /><Node type=""in"" id=""2"" fnId=""0"" /><Node type=""in"" id=""3"" fnId=""0"" /><Node type=""in"" id=""4"" fnId=""0"" /><Node type=""in"" id=""5"" fnId=""0"" /><Node type=""in"" id=""6"" fnId=""0"" /><Node type=""in"" id=""7"" fnId=""0"" /><Node type=""in"" id=""8"" fnId=""0"" /><Node type=""in"" id=""9"" fnId=""0"" /><Node type=""in"" id=""10"" fnId=""0"" /><Node type=""in"" id=""11"" fnId=""0"" /><Node type=""in"" id=""12"" fnId=""0"" /><Node type=""in"" id=""13"" fnId=""0"" /><Node type=""in"" id=""14"" fnId=""0"" /><Node type=""out"" id=""15"" fnId=""0"" /><Node type=""out"" id=""16"" fnId=""0"" /><Node type=""hid"" id=""199"" fnId=""0"" /></Nodes><Connections><Con id=""37"" src=""10"" tgt=""15"" wght=""17.950713150203228"" /><Con id=""42"" src=""12"" tgt=""16"" wght=""2.6496202937411319"" /><Con id=""44"" src=""13"" tgt=""16"" wght=""11.645543473392575"" /><Con id=""46"" src=""14"" tgt=""16"" wght=""0.711072886873828"" /><Con id=""60"" src=""15"" tgt=""16"" wght=""0.25754744994692313"" /><Con id=""61"" src=""2"" tgt=""16"" wght=""-9.3341266740427464"" /><Con id=""68"" src=""11"" tgt=""15"" wght=""8.6122131150781538"" /><Con id=""200"" src=""11"" tgt=""199"" wght=""10.0075376336909"" /><Con id=""201"" src=""199"" tgt=""16"" wght=""-0.75516500417253274"" /><Con id=""276"" src=""10"" tgt=""199"" wght=""-4.51026813097507"" /><Con id=""277"" src=""13"" tgt=""199"" wght=""5.6883298978209496"" /><Con id=""377"" src=""12"" tgt=""15"" wght=""6.2588944037887266"" /></Connections></Network></Networks></Root>";
            //string xmldata = @"<?xml version=""1.0"" encoding=""utf-8""?><Root><ActivationFunctions><Fn id=""0"" name=""SteepenedSigmoid"" prob=""1"" /></ActivationFunctions><Networks><Network id=""67138"" birthGen=""32"" fitness=""600592.62347990274""><Nodes><Node type=""bias"" id=""0"" fnId=""0"" /><Node type=""in"" id=""1"" fnId=""0"" /><Node type=""in"" id=""2"" fnId=""0"" /><Node type=""in"" id=""3"" fnId=""0"" /><Node type=""in"" id=""4"" fnId=""0"" /><Node type=""in"" id=""5"" fnId=""0"" /><Node type=""in"" id=""6"" fnId=""0"" /><Node type=""in"" id=""7"" fnId=""0"" /><Node type=""in"" id=""8"" fnId=""0"" /><Node type=""in"" id=""9"" fnId=""0"" /><Node type=""in"" id=""10"" fnId=""0"" /><Node type=""in"" id=""11"" fnId=""0"" /><Node type=""in"" id=""12"" fnId=""0"" /><Node type=""in"" id=""13"" fnId=""0"" /><Node type=""in"" id=""14"" fnId=""0"" /><Node type=""out"" id=""15"" fnId=""0"" /><Node type=""out"" id=""16"" fnId=""0"" /><Node type=""hid"" id=""257"" fnId=""0"" /></Nodes><Connections><Con id=""39"" src=""11"" tgt=""15"" wght=""14.983131706912014"" /><Con id=""41"" src=""12"" tgt=""15"" wght=""-16.656046019044798"" /><Con id=""48"" src=""1"" tgt=""16"" wght=""3.4454722083698406"" /><Con id=""51"" src=""0"" tgt=""16"" wght=""-3.2974852298349577"" /><Con id=""58"" src=""12"" tgt=""16"" wght=""1.01362280202528"" /><Con id=""99"" src=""2"" tgt=""16"" wght=""-19.538866929812958"" /><Con id=""258"" src=""10"" tgt=""257"" wght=""16.774923013485886"" /><Con id=""259"" src=""257"" tgt=""15"" wght=""19.811142552934392"" /></Connections></Network></Networks></Root>";
            StringReader sr = new StringReader(xmldata);


            //NeatGenomeDecoder decoder = new NeatGenomeDecoder(SimpleNeatBrain3.sActivationScheme);
            //NeatGenomeFactory mGenomeFactory = new NeatGenomeFactory(SimpleNeatBrain3.COUNT_INPUT, SimpleNeatBrain3.COUNT_OUTPUT, SimpleNeatBrain3.sGenomeParams);

            NeatGenomeDecoder decoder = new NeatGenomeDecoder(NeatBrain4.sActivationScheme);
            NeatGenomeFactory mGenomeFactory = new NeatGenomeFactory(NeatBrain4.COUNT_INPUT, NeatBrain4.COUNT_OUTPUT, NeatBrain4.sGenomeParams);
            
            
            
            XmlReader reader = XmlReader.Create(sr);
            NeatGenome gen = NeatGenomeXmlIO.ReadCompleteGenomeList(reader, true, mGenomeFactory)[0];
            reader.Close();
            return gen;
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AbstractGenomeView genomeView = new NeatGenomeView(); 



            NeatGenome genome = null;

            OpenFileDialog pfd = new OpenFileDialog();
            pfd.InitialDirectory = @"D:\data\dev\4science\testank\neatai\best";
            DialogResult res = pfd.ShowDialog();
            if(res == DialogResult.OK)
            {
                genome = CreateNeatBrainFromFile(pfd.FileName);
                // Create form.
                GenomeForm _bestGenomeForm = new GenomeForm(pfd.FileName, genomeView, genome);
                _bestGenomeForm.BackColor = System.Drawing.Color.White;

                _bestGenomeForm.Size = new System.Drawing.Size(1280, 720);

                // Show the form.
                //_bestGenomeForm.Show(this);
                _bestGenomeForm.RefreshView();

                Application.Run(_bestGenomeForm);
            }

        }
    }
