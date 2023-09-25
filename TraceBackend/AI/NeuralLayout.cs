using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.IO;

namespace TraceBackend.AI
{
    public abstract class NeuralBlueprint
    {
        public abstract NeuralOperation InstanceFromStream(BinaryReader Reader);
    }

    public class NeuralLayout : NeuralBlueprint
    {
        public int[] Layers;
        public int LayerCount => Layers.Length;
        public NonLinearityFunction Nonlinearity;
        public CostFunction Cost;

        public override NeuralOperation InstanceFromStream(BinaryReader Reader)
        {
            return NeuralNetwork.FromStream(this, Reader);
        }
    }

    public class NonLinearityFunction
    {
        public delegate double Del_Evaluate(double raw);
        public delegate double Del_Derivative(double raw);
        public Del_Evaluate Evaluate;
        public Del_Derivative Derivative;

        public static NonLinearityFunction Sigmoid;
        public static NonLinearityFunction TanH;

        static NonLinearityFunction()
        {
            Sigmoid = new NonLinearityFunction()
            {
                Evaluate = new Del_Evaluate(SpecialFunctions.Logistic),
                Derivative = new Del_Derivative(t => { double d = SpecialFunctions.Logistic(t); return d * (1.0 - d); })
            };
            TanH = new NonLinearityFunction()
            {
                Evaluate = new Del_Evaluate(Trig.Tanh),
                Derivative = new Del_Derivative(t => { double d = Trig.Sech(t); return d * d; })
            };
        }

        public Vector<double> EvaluateAll(Vector<double> raw)
        {
            Vector<double> res = Vector<double>.Build.Dense(raw.Count);
            raw.Map(new Func<double,double>(Evaluate), res);
            return res;
        }
        public Vector<double> DeriveAll(Vector<double> raw)
        {
            Vector<double> res = Vector<double>.Build.Dense(raw.Count);
            raw.Map(new Func<double, double>(Derivative), res);
            return res;
        }
    }

    public class CostFunction
    {
        public delegate double Del_Evaluate(Vector<double> raw, Vector<double> target);
        public delegate double Del_Derivative(Vector<double> raw, Vector<double> target, int i);
        public Del_Evaluate Evaluate;
        public Del_Derivative Derivative;

        public static CostFunction SumSq;

        static CostFunction()
        {
            SumSq = new CostFunction()
            {
                Evaluate = new Del_Evaluate(Distance.SSD),
                Derivative = new Del_Derivative((r, t, i) => 2.0 * (r[i] - t[i]))
            };
        }

        public Vector<double> DeriveAll(Vector<double> raw, Vector<double> target)
        {
            Vector<double> output = CreateVector.Dense<double>(raw.Count);
            for (int i = 0; i < raw.Count; i++)
            {
                output[i] = Derivative(raw, target, i);
            }
            return output;
        }
    }

    public class AbstractNeuralComplex : NeuralBlueprint
    {
        internal List<NeuralBlueprint> OperationBlueprints;
        public List<(int From, int To, bool Slot2)> Links;

        public AbstractNeuralComplex()
        {
            OperationBlueprints = new List<NeuralBlueprint>();
            Links = new List<(int, int, bool)>();
        }

        public int AddBlueprint(NeuralBlueprint Op)
        {
            OperationBlueprints.Add(Op);
            return OperationBlueprints.Count - 1;
        }

        public void AddLink(int From, int To, bool Slot2 = false)
        {
            Links.Add((From, To, Slot2));
        }

        NeuralBlueprint Blueprint(int index)
        {
            if (index < 0) return this;
            return OperationBlueprints[index];
        }

        public void Reorder()
        {
            throw new NotImplementedException();
        }

        public override NeuralOperation InstanceFromStream(BinaryReader Reader)
        {
            return NeuralComplex.FromStream(this, Reader);
        }
    }

    public class NeuralPlusBlueprint : NeuralBlueprint
    {
        public static NeuralPlusBlueprint Default;

        static NeuralPlusBlueprint() { Default = new NeuralPlusBlueprint(); }

        public override NeuralOperation InstanceFromStream(BinaryReader Reader)
        {
            return new NeuralPlus();
        }
    }
    public class NeuralTimesBlueprint : NeuralBlueprint
    {
        public static NeuralTimesBlueprint Default;

        static NeuralTimesBlueprint() { Default = new NeuralTimesBlueprint(); }

        public override NeuralOperation InstanceFromStream(BinaryReader Reader)
        {
            return new NeuralTimes();
        }
    }
    public class NeuralOneMinusBlueprint : NeuralBlueprint
    {
        public static NeuralOneMinusBlueprint Default;

        static NeuralOneMinusBlueprint() { Default = new NeuralOneMinusBlueprint(); }

        public override NeuralOperation InstanceFromStream(BinaryReader Reader)
        {
            return new NeuralOneMinus();
        }
    }
    public class NeuralConcatBlueprint : NeuralBlueprint
    {
        public static NeuralConcatBlueprint Default;

        static NeuralConcatBlueprint() { Default = new NeuralConcatBlueprint(); }

        public override NeuralOperation InstanceFromStream(BinaryReader Reader)
        {
            return new NeuralConcat();
        }
    }

}
