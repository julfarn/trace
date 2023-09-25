using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.IO;

namespace TraceBackend.AI
{
    public static class DefinitionReader
    {
        public const int MEM_SIZE = 1024;
        public const int STEPWISE_INPUT_SIZE = 16;
        public static NeuralLayout _Forget, _Input, _GMind, _Mix, _LMind;
        static NeuralNetwork Forget, Input, GMind, Mix; // These are global
        static NeuralComplex NCNegation, NCEquality, NCTrue, NCFalse, NCVariable, NCUPred; // These are global
        static int concat, forget, input, gmind, mix, times7, oneminus, times9, plus10, times11, times12, plus13;
        public static int lmind;
        static NeuralConcat Concat;
        static NeuralOneMinus OneMinus;
        static NeuralTimes Times7, Times9, Times11, Times12;
        static NeuralPlus Plus10, Plus13;
        static AbstractNeuralComplex _LSTMComplex;

        static DefinitionReader()
        {
            // Layout of NNs
            _Forget = new NeuralLayout()
            {
                Layers = new int[] { MEM_SIZE + STEPWISE_INPUT_SIZE, 64, 64, MEM_SIZE },
                Nonlinearity = NonLinearityFunction.Sigmoid,
                Cost = CostFunction.SumSq
            };
            _Input = new NeuralLayout()
            {
                Layers = new int[] { MEM_SIZE + STEPWISE_INPUT_SIZE, 64, 64, MEM_SIZE },
                Nonlinearity = NonLinearityFunction.Sigmoid,
                Cost = CostFunction.SumSq
            };
            _GMind = new NeuralLayout()
            {
                Layers = new int[] { MEM_SIZE + STEPWISE_INPUT_SIZE, 512, 256, 512, MEM_SIZE },
                Nonlinearity = NonLinearityFunction.TanH,
                Cost = CostFunction.SumSq
            };
            _Mix = new NeuralLayout()
            {
                Layers = new int[] { MEM_SIZE + STEPWISE_INPUT_SIZE, 64, 64, MEM_SIZE },
                Nonlinearity = NonLinearityFunction.Sigmoid,
                Cost = CostFunction.SumSq
            };
            _LMind = new NeuralLayout()
            {
                Layers = new int[] { MEM_SIZE + STEPWISE_INPUT_SIZE, 512, 256, 512, MEM_SIZE },
                Nonlinearity = NonLinearityFunction.TanH,
                Cost = CostFunction.SumSq
            };

            // Layout of Encoder
            _LSTMComplex = new AbstractNeuralComplex();
            concat = _LSTMComplex.AddBlueprint(NeuralConcatBlueprint.Default);
            Concat = new NeuralConcat();
            forget = _LSTMComplex.AddBlueprint(_Forget);
            input = _LSTMComplex.AddBlueprint(_Input);
            gmind = _LSTMComplex.AddBlueprint(_GMind);
            mix = _LSTMComplex.AddBlueprint(_Mix);
            lmind = _LSTMComplex.AddBlueprint(_LMind);
            times7 = _LSTMComplex.AddBlueprint(NeuralTimesBlueprint.Default);
            Times7 = new NeuralTimes();
            oneminus = _LSTMComplex.AddBlueprint(NeuralOneMinusBlueprint.Default);
            OneMinus = new NeuralOneMinus();
            times9 = _LSTMComplex.AddBlueprint(NeuralTimesBlueprint.Default);
            Times9 = new NeuralTimes();
            plus10 = _LSTMComplex.AddBlueprint(NeuralPlusBlueprint.Default);
            Plus10 = new NeuralPlus();
            times11 = _LSTMComplex.AddBlueprint(NeuralTimesBlueprint.Default);
            Times11 = new NeuralTimes();
            times12 = _LSTMComplex.AddBlueprint(NeuralTimesBlueprint.Default);
            Times12 = new NeuralTimes();
            plus13 = _LSTMComplex.AddBlueprint(NeuralPlusBlueprint.Default);
            Plus13 = new NeuralPlus();

            _LSTMComplex.AddLink(-1, concat);
            _LSTMComplex.AddLink(-2, concat, true);
            _LSTMComplex.AddLink(-1, times12);
            _LSTMComplex.AddLink(concat, forget);
            _LSTMComplex.AddLink(concat, input);
            _LSTMComplex.AddLink(concat, gmind);
            _LSTMComplex.AddLink(concat, mix);
            _LSTMComplex.AddLink(concat, lmind);

            _LSTMComplex.AddLink(forget, times12, true);
            _LSTMComplex.AddLink(input, times11);
            _LSTMComplex.AddLink(gmind, times7);
            _LSTMComplex.AddLink(mix, times7, true);
            _LSTMComplex.AddLink(mix, oneminus);
            _LSTMComplex.AddLink(oneminus, times9);
            _LSTMComplex.AddLink(lmind, times9, true);

            _LSTMComplex.AddLink(times7, plus10);
            _LSTMComplex.AddLink(times9, plus10, true);
            _LSTMComplex.AddLink(plus10, times11, true);

            _LSTMComplex.AddLink(times12, plus13);
            _LSTMComplex.AddLink(times11, plus13, true);

            _LSTMComplex.AddLink(plus13, -1);

            // Global Networks
            Forget = NeuralNetwork.FromFileOrRandom(_Forget, Properties.Settings.Default.AIDirectory + "\\DEF_RD_FORGET.mind");
            Input = NeuralNetwork.FromFileOrRandom(_Input, Properties.Settings.Default.AIDirectory + "\\DEF_RD_INPUT.mind");
            GMind = NeuralNetwork.FromFileOrRandom(_GMind, Properties.Settings.Default.AIDirectory + "\\DEF_RD_GMIND.mind");
            Mix = NeuralNetwork.FromFileOrRandom(_Mix, Properties.Settings.Default.AIDirectory + "\\DEF_RD_MIX.mind");

            NCNegation = FromFile(Properties.Settings.Default.AIDirectory + "\\DEF_RD_NEGATION.mind");
            NCEquality = FromFile(Properties.Settings.Default.AIDirectory + "\\DEF_RD_EQUALITY.mind");
            NCTrue = FromFile(Properties.Settings.Default.AIDirectory + "\\DEF_RD_TRUE.mind");
            NCFalse = FromFile(Properties.Settings.Default.AIDirectory + "\\DEF_RD_FALSE.mind");
            NCVariable = FromFile(Properties.Settings.Default.AIDirectory + "\\DEF_RD_VARIABLE.mind");
            NCUPred = FromFile(Properties.Settings.Default.AIDirectory + "\\DEF_RD_UPRED.mind");
        }

        private static NeuralComplex GetLocalMind(MExpression E, Validity val = null)
        {
            if (E is MNegationFormula)
            {
                return NCNegation;
            }
            else if (E is MPlaceholderFormula)
            {
                throw new Exception();
            }
            else if (E is MPlaceholderTerm)
            {
                throw new Exception();
            }
            else if (E is MUndefinedPredicateFormula)
            {
                return NCUPred;
            }
            else if (E is MTrivialFormula tr)
            {
                if (tr._V) return NCTrue;
                return NCFalse;
            }
            else if (E is MEqualityFormula)
            {
                return NCEquality;
            }
            else if (E is MVariable V)
            {
                if (V.HasAxioms || (val != null && V.HasDependencies(val)))
                    return V.Mind;
                else return NCVariable;
            }
            else //Things with Definition!
            {
                return E._D.Mind;
            }
        }

        public static (NeuralComplex Chain, List<Vector<double>> Inputs) BuildStatementEncoder(MStatement S)
        {
            return BuildStatementEncoder(S._F, S.valid);
        }
        public static (NeuralComplex Chain, List<Vector<double>> Inputs) BuildStatementEncoder(MExpression E, Validity val = null)
        {
            AbstractNeuralComplex A = new AbstractNeuralComplex();
            NeuralComplex C = new NeuralComplex(A);

            int itercnt = -1;
            List<Vector<double>> Inputs = new List<Vector<double>>();
            Iterate(E);


            return (C, Inputs);

            void Iterate(MExpression expr)
            {
                itercnt++;

                for(int i = 0; i< expr.SubCount; i++)
                {
                    Iterate(expr[i]);
                }
                A.AddBlueprint(_LSTMComplex);
                C.Operations.Add(GetLocalMind(expr, val));
                if (E != expr) A.AddLink(itercnt - 1, itercnt);
                Inputs.Add(makeInputs(expr));
            }

            Vector<double> makeInputs(MExpression expr)
            {
                return Vector<double>.Build.DenseOfArray(new double[]
                {
                    expr.SubCount == 0 ? 1.0 : 0.0,                     // raise Level
                    expr.ID,                                            // ID (to keep variables, upreds apart)
                    expr is MTerm ? 1.0 : 0.0,                          // Term/Formula
                    expr is MNegationFormula ? 1.0 : 0.0,               // Negation
                    expr is MUndefinedPredicateFormula ? 1.0 : 0.0,     // Upred
                    expr is MVariable ? 1.0 : 0.0,                      // Var
                    expr is MBoundVariable || (expr is MVariable V && !(V.HasAxioms || (val != null && V.HasDependencies(val)))) ? 1.0 : 0.0, // Free Variable
                    expr is MTrivialFormula T && T._V ? 1.0 : 0.0,      // Just True
                    expr is MTrivialFormula F && !F._V ? 1.0 : 0.0,     // Just False
                    expr is MEqualityFormula ? 1.0 : 0.0,               // Equality
                    0,  // 10
                    0,  // 11
                    0,  // 12
                    0,  // 13
                    0,  // 14
                    0   // 15
                });
            }
        }

        public static NeuralComplex FromFile(string path)
        {
            NeuralComplex NC = new NeuralComplex(_LSTMComplex);
            NC.SetOperation(concat, Concat);
            NC.SetOperation(forget, Forget);
            NC.SetOperation(input, Input);
            NC.SetOperation(gmind, GMind);
            NC.SetOperation(mix, Mix);
            NC.SetOperation(lmind, NeuralNetwork.FromFileOrRandom(_LMind, path));
            NC.SetOperation(times7, Times7);
            NC.SetOperation(oneminus, OneMinus);
            NC.SetOperation(times9, Times9);
            NC.SetOperation(plus10, Plus10);
            NC.SetOperation(times11, Times11);
            NC.SetOperation(times12, Times12);
            NC.SetOperation(plus13, Plus13);
            return NC;
        }
    }
}
