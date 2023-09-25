using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.IO;

namespace TraceBackend.AI
{
    public abstract class NeuralOperation
    {
        public abstract Vector<double> InputState { get; set; }
        public abstract Vector<double> InputState2 { get; set; }
        public abstract Vector<double> OutputState { get; set; }
        public abstract void Compute();

        public abstract (Vector<double> GradI1, Vector<double> GradI2) Backpropagate(Vector<double> GradTarget, double rate);
    }

    public class NeuralComplex : NeuralOperation
    {
        public override Vector<double> InputState { get; set; }
        public override Vector<double> InputState2 { get; set; } // To add Link from InputState2, use From = -2
        public override Vector<double> OutputState { get; set; }

        internal List<NeuralOperation> Operations;

        public AbstractNeuralComplex Abstract;

        public NeuralComplex(AbstractNeuralComplex A)
        {
            Abstract = A;
            Operations = new NeuralOperation[A.OperationBlueprints.Count].ToList();
        }

        public static NeuralComplex FromStream(AbstractNeuralComplex A, BinaryReader Reader)
        {
            NeuralComplex NC = new NeuralComplex(A);

            for (int i = 0; i < NC.Operations.Count; i++)
            {
                NC.Operations[i] = NC.Abstract.OperationBlueprints[i].InstanceFromStream(Reader);
            }

            return NC;
        }

        public void SetOperation(int index, NeuralOperation Op)
        {
            Operations[index] = Op;
        }

        int Index(NeuralOperation Op)
        {
            return (Op == this) ? -1 : Operations.IndexOf(Op);
        }

        public NeuralOperation Operation(int index)
        {
            if (index < 0) return this;
            return Operations[index];
        }

        public void Reorder()
        {
            throw new NotImplementedException();
        }

        private List<(int From, int To, bool Slot2)> FilterFrom(NeuralOperation From)
        {
            return Abstract.Links.FindAll(L => L.From == Index(From));
        }

        private List<(int From, int To, bool Slot2)> FilterFrom(int From)
        {
            return Abstract.Links.FindAll(L => L.From == From);
        }

        private List<(int From, int To, bool Slot2)> FilterTo(NeuralOperation To)
        {
            return Abstract.Links.FindAll(L => L.To == Index(To));
        }

        public override void Compute() // This relies on proper ordering of the Operations.
        {
            PerformLinks(FilterFrom(-1));
            PerformLinks(FilterFrom(-2));

            foreach (NeuralOperation Op in Operations)
            {
                Op.Compute();
                PerformLinks(FilterFrom(Op));
            }

            OutputState = Operation(FilterTo(this)[0].From).OutputState;

            void PerformLinks(List<(int From, int To, bool Slot2)> Ls)
            {
                foreach (var L in Ls)
                {
                    if (L.Slot2) Operation(L.To).InputState2 = InputState;
                    else Operation(L.To).InputState2 = InputState;
                }
            }
        }

        public void ComputeRecursive(List<Vector<double>> Inputs, int cutoff = -1)
        {
            PerformLinks(FilterFrom(-1));
            PerformLinks(FilterFrom(-2));

            if (cutoff == -1) cutoff = Operations.Count - 1;

            for(int i = 0; i<=cutoff; i++)
            {
                Operations[i].InputState2 = Inputs[i];
                Operations[i].Compute();
                PerformLinks(FilterFrom(Operations[i]));
            }

            OutputState = Operation(FilterTo(this)[0].From).OutputState;

            void PerformLinks(List<(int From, int To, bool Slot2)> Ls)
            {
                foreach (var L in Ls)
                {
                    if (L.Slot2) Operation(L.To).InputState2 = InputState;
                    else Operation(L.To).InputState2 = InputState;
                }
            }
        }

        public override (Vector<double> GradI1, Vector<double> GradI2) Backpropagate(Vector<double> GradTarget, double rate)
        {
            var GradList = new (Vector<double> GradI1, Vector<double> GradI2)[Operations.Count];

            for (int i = Operations.Count - 1; i >= 0; i--)
            {
                GradList[i] = IterBackpop(i);
            }

            return (IterBackpop(-1).GradI1, IterBackpop(-2).GradI1);

            (Vector<double> GradI1, Vector<double> GradI2) IterBackpop(int index)
            {
                var FromList = FilterFrom(index);
                Vector<double> GradI1 = null; Vector<double> GradI2 = null;

                foreach (var (From, To, Slot2) in FromList)
                {
                    if (To == -1) return Operation(index).Backpropagate(GradTarget, rate);

                    var GTo = Slot2 ? GradList[To].GradI2 : GradList[To].GradI1;

                    if (index < 0)
                    {
                        if (GradI1 == null) GradI1 = GTo;
                        else GradI1 += GTo;
                    }
                    else
                    {
                        if (GradI1 == null) (GradI1, GradI2) = Operation(index).Backpropagate(GTo, rate); //TODO: Custom Learning Rates
                        else
                        {
                            (Vector<double> GradI1, Vector<double> GradI2) Gr = Operation(index).Backpropagate(GTo, rate);
                            GradI1 += Gr.GradI1; GradI2 += Gr.GradI2;
                        }
                    }
                }

                return (GradI1, GradI2);
            }
        }

        public (Vector<double> GradI1, Vector<double> GradI2) BackpropagateRecursive(List<Vector<double>> Inputs, Vector<double> GradTarget, double rate)
        {
            var GradList = new (Vector<double> GradI1, Vector<double> GradI2)[Operations.Count];

            for(int i = Operations.Count-1; i>= 0; i--)
            {
                ComputeRecursive(Inputs, i);
                GradList[i] = IterBackpop(i);
            }

            return (IterBackpop(-1).GradI1, IterBackpop(-2).GradI1);

            (Vector<double> GradI1, Vector<double> GradI2) IterBackpop(int index)
            {
                var FromList = FilterFrom(index);
                Vector<double> GradI1 = null; Vector<double> GradI2 = null;

                foreach (var (From, To, Slot2) in FromList)
                {
                    if (To == -1) return Operation(index).Backpropagate(GradTarget, rate);

                    var GTo = Slot2 ? GradList[To].GradI2 : GradList[To].GradI1;

                    if (index < 0)
                    {
                        if (GradI1 == null) GradI1 = GTo;
                        else GradI1 += GTo;
                    }
                    else
                    {
                        if (GradI1 == null) (GradI1, GradI2) = Operation(index).Backpropagate(GTo, rate); //TODO: Custom Learning Rates
                        else
                        {
                            (Vector<double> GradI1, Vector<double> GradI2) Gr = Operation(index).Backpropagate(GTo, rate);
                            GradI1 += Gr.GradI1; GradI2 += Gr.GradI2;
                        }
                    }
                }

                return (GradI1, GradI2);
            }
        }
    }

    public class NeuralNetwork : NeuralOperation
    {
        NeuralLayout Layout;
        public string FileName;

        public Vector<double>[] Bias;
        public Matrix<double>[] Weights;

        public Vector<double>[] State;
        public Vector<double>[] PreNLState;
        public override Vector<double> InputState { get { return State[0]; } set { State[0] = value; } }
        public override Vector<double> OutputState { get { return State[Layout.LayerCount - 1]; } set { State[Layout.LayerCount - 1] = value; } }
        public override Vector<double> InputState2 { get => null; set { } }

        public Vector<double>[] BiasMomentum;
        public Matrix<double>[] WeightsMomentum;

        public static NeuralNetwork FromFileOrRandom(NeuralLayout Layout, string file)
        {
            if (File.Exists(file))
            {
                using (FileStream stream = File.OpenRead(file))
                {
                    using (BinaryReader Reader = new BinaryReader(stream))
                    {
                        NeuralNetwork ret = FromStream(Layout, Reader);
                        Reader.Close();
                        stream.Close();
                        ret.FileName = file;
                        return ret;
                    }
                }
            }
            else
            {
                NeuralNetwork ret = WithRandoms(Layout);
                ret.FileName = file;
                return ret;
            }
        }
        public static NeuralNetwork FromStream(NeuralLayout Layout, BinaryReader Reader)
        {
            NeuralNetwork NN = WithZeros(Layout);

            for (int Layer = 0; Layer < Layout.LayerCount - 1; Layer++)
            {
                for(int column = 0; column < Layout.Layers[Layer + 1]; column++)
                {
                    NN.Bias[Layer][column] = Reader.ReadDouble();
                    for(int row = 0; row < Layout.Layers[Layer]; row++)
                    {
                        NN.Weights[Layer][row, column] = Reader.ReadDouble();
                    }
                }
            }

            return NN;
        }

        public void ToFile(string file = "")
        {
            if (file == "") file = FileName;
            using (FileStream stream = File.OpenWrite(file))
            using (BinaryWriter Writer = new BinaryWriter(stream))
            {
                ToStream(Writer);
                Writer.Close();
                stream.Close();
            }
        }
        public void ToStream(BinaryWriter Writer)
        {
            for (int Layer = 0; Layer < Layout.LayerCount - 1; Layer++)
            {
                for (int column = 0; column < Layout.Layers[Layer + 1]; column++)
                {
                    Writer.Write(Bias[Layer][column]);
                    for (int row = 0; row < Layout.Layers[Layer]; row++)
                    {
                        Writer.Write(Weights[Layer][row, column]);
                    }
                }
            }
        }

        public static NeuralNetwork WithZeros(NeuralLayout Layout)
        {
            NeuralNetwork NN = new NeuralNetwork(Layout);

            for (int Layer = 0; Layer < Layout.LayerCount - 1; Layer++)
            {
                NN.Bias[Layer] = Vector<double>.Build.Dense(Layout.Layers[Layer + 1]);
                NN.BiasMomentum[Layer] = Vector<double>.Build.Dense(Layout.Layers[Layer + 1]);
                NN.Weights[Layer] = Matrix<double>.Build.Dense(Layout.Layers[Layer], Layout.Layers[Layer + 1]);
                NN.WeightsMomentum[Layer] = Matrix<double>.Build.Dense(Layout.Layers[Layer], Layout.Layers[Layer + 1]);
            }

            NN.InputState = Vector<double>.Build.Dense(Layout.Layers[0]);

            return NN;
        }

        public static NeuralNetwork WithRandoms(NeuralLayout Layout)
        {
            NeuralNetwork NN = new NeuralNetwork(Layout);
            MathNet.Numerics.Distributions.Normal NDist = new MathNet.Numerics.Distributions.Normal(0.0, 2.0);

            for (int Layer = 0; Layer < Layout.LayerCount - 1; Layer++)
            {
                NN.Bias[Layer] = Vector<double>.Build.Random(Layout.Layers[Layer + 1], NDist);
                NN.BiasMomentum[Layer] = Vector<double>.Build.Dense(Layout.Layers[Layer + 1]);
                NN.Weights[Layer] = Matrix<double>.Build.Random(Layout.Layers[Layer], Layout.Layers[Layer + 1], NDist);
                NN.WeightsMomentum[Layer] = Matrix<double>.Build.Dense(Layout.Layers[Layer], Layout.Layers[Layer + 1]);
            }

            NN.InputState = Vector<double>.Build.Dense(Layout.Layers[0]);

            return NN;
        }

        private NeuralNetwork(NeuralLayout L)
        {
            Layout = L;
            Bias = new Vector<double>[L.LayerCount - 1];
            Weights = new Matrix<double>[L.LayerCount - 1];
            State = new Vector<double>[L.LayerCount];
            PreNLState = new Vector<double>[L.LayerCount];

            BiasMomentum = new Vector<double>[L.LayerCount - 1];
            WeightsMomentum = new Matrix<double>[L.LayerCount - 1];
        }

        public override void Compute()
        {
            for(int Layer = 1; Layer < Layout.LayerCount; Layer++)
            {
                PreNLState[Layer] = (Weights[Layer - 1] * State[Layer - 1]) + Bias[Layer];
                State[Layer] = Layout.Nonlinearity.EvaluateAll(PreNLState[Layer]);
            }
        }

        public override (Vector<double> GradI1, Vector<double> GradI2) Backpropagate(Vector<double> GradTarget, double rate)
        {
            Vector<double> StateGradSig = GradTarget; //So far this is only the state gradient

            for(int Layer = Layout.LayerCount-2; Layer>= 0; Layer--)
            {
                StateGradSig = StateGradSig.PointwiseMultiply(Layout.Nonlinearity.DeriveAll(PreNLState[Layer])); // We always multiply it by sigma'

                BiasMomentum[Layer] += rate * StateGradSig; // Keep in Mind that Bias[Layer] corresponds to State[Layer+1]
                WeightsMomentum[Layer] += rate * StateGradSig.OuterProduct(State[Layer]);

                StateGradSig = StateGradSig * Weights[Layer]; // This is again only the state Gradient
            }

            return (StateGradSig, null);
        }

        public void LearnBatch(Vector<double>[] Inputs, Vector<double>[] Targets, double LearnRate, double Fatigue)
        {
            for(int i = 0; i< Inputs.Length; i++)
            {
                InputState = Inputs[i];
                Compute();
                Backpropagate(Layout.Cost.DeriveAll(OutputState, Targets[i]), LearnRate / Inputs.Length);
            }

            for(int Layer = 0; Layer < Layout.LayerCount -1; Layer++)
            {
                Bias[Layer] += BiasMomentum[Layer];
                Weights[Layer] += WeightsMomentum[Layer];

                BiasMomentum[Layer] = Fatigue * BiasMomentum[Layer];
                WeightsMomentum[Layer] = Fatigue * WeightsMomentum[Layer];
            }
        }
    }

    public class NeuralPlus : NeuralOperation
    {
        public static NeuralPlus Default;
        static NeuralPlus() { Default = new NeuralPlus(); }

        public override Vector<double> InputState { get; set; }
        public override Vector<double> OutputState { get; set; }
        public override Vector<double> InputState2 { get; set; }

        public override (Vector<double> GradI1, Vector<double> GradI2) Backpropagate(Vector<double> GradTarget, double rate)
        {
            return (GradTarget, GradTarget);
        }

        public override void Compute()
        {
            OutputState = InputState + InputState2;
        }
    }

    public class NeuralTimes : NeuralOperation
    {
        public static NeuralTimes Default;
        static NeuralTimes() { Default = new NeuralTimes(); }

        public override Vector<double> InputState { get; set; }
        public override Vector<double> OutputState { get; set; }
        public override Vector<double> InputState2 { get; set; }

        public override (Vector<double> GradI1, Vector<double> GradI2) Backpropagate(Vector<double> GradTarget, double rate)
        {
            return (GradTarget.PointwiseMultiply(InputState2), GradTarget.PointwiseMultiply(InputState));
        }

        public override void Compute()
        {
            OutputState = InputState.PointwiseMultiply(InputState2);
        }
    }

    public class NeuralOneMinus : NeuralOperation
    {
        public static NeuralOneMinus Default;
        static NeuralOneMinus() { Default = new NeuralOneMinus(); }

        public override Vector<double> InputState { get; set; }
        public override Vector<double> OutputState { get; set; }
        public override Vector<double> InputState2 { get => null; set { } }

        public override (Vector<double> GradI1, Vector<double> GradI2) Backpropagate(Vector<double> GradTarget, double rate)
        {
            return (1 - GradTarget, null);
        }

        public override void Compute()
        {
            OutputState = 1 - InputState;
        }
    }

    public class NeuralConcat : NeuralOperation
    {
        public static NeuralConcat Default;
        static NeuralConcat() { Default = new NeuralConcat(); }

        public override Vector<double> InputState { get; set; }
        public override Vector<double> OutputState { get; set; }
        public override Vector<double> InputState2 { get; set; }

        public override (Vector<double> GradI1, Vector<double> GradI2) Backpropagate(Vector<double> GradTarget, double rate)
        {
            if (InputState.Count + InputState2.Count != GradTarget.Count) throw new Exception();
            return (GradTarget.SubVector(0, InputState.Count), GradTarget.SubVector(InputState.Count, InputState2.Count));
        }

        public override void Compute()
        {
            OutputState = Vector<double>.Build.DenseOfEnumerable(InputState.Concat(InputState2));
        }
    }
}
