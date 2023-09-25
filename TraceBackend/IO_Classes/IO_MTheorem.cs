using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TraceBackend
{
    public partial class MTheorem
    {
        public override void ToStream(DocumentLoader DL)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();
            fileID.ToStream(DL);
            DL.SetTheorem(this);

            Statement.ToStream(DL);
            DL.Write(Deduction != null);
            if (Deduction != null)
                Deduction.ToStream(DL);

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("theorem");
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));
            DL.SetTheorem(this);

            DL.Xwr.WriteAttributeString("hasdeduction",(Deduction != null).ToString());
            Statement.ToXML(DL);
            if (Deduction != null)
                Deduction.ToXML(DL);

            DL.Xwr.WriteEndElement(); //theorem
        }

        public new static MTheorem FromStream(DocumentLoader DL)
        {
            //prefix
            DL.Remember();

            TheoremFileID fileID = FileID.FromStream(DL) as TheoremFileID;

            MTheorem T = new MTheorem(fileID, DL.Context) { loaded = true };
            DL.SetTheorem(T);

            T.SetStatement(MStatement.FromStream(DL));
            if (DL.ReadBool())
                T.SetDeduction(MDeduction.FromStream(DL), false);

            //T.Validate();

            return T;
        }
        public new static MTheorem FromStream(XMLDocumentLoader DL, XmlNode node)
        {

            TheoremFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as TheoremFileID;

            MTheorem T = new MTheorem(fileID, DL.Context) { loaded = true };
            DL.SetTheorem(T);

            T.SetStatement(MStatement.FromStream(DL, node.GetChildNode("statement")));
            if (node.GetAttrBool("hasdeduction"))
                T.SetDeduction(MDeduction.FromStream(DL, node.GetChildNode("deduction")), false);

            //T.Validate();

            return T;
        }

        public void Load()
        {
            if (loaded) return;

            GetStatement().Load();
            GetDeduction().Load();

            loaded = true;
        }

        public static MTheorem GetUnloaded(DocumentLoader DL, TheoremFileID ID)
        {
            MTheorem T = new MTheorem(ID.MakeTheoremFileID(), DL.Context) { };
            return T;
        }
        public static MTheorem GetUnloaded(XMLDocumentLoader DL, XmlNode node, TheoremFileID ID)
        {
            MTheorem T = new MTheorem(ID.MakeTheoremFileID(), DL.Context) { };
            return T;
        }

        private void LoadDeduction()
        {
            if (Context.Document.IsXML) { LoadDeductionXML(); return; }

            DocumentLoader DL = Context.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(Context);
            DL.SetTheorem(this);

            DL.CleanSkip(); //Statement
            if (DL.ReadBool())
            {
                DL.Remember();
                DeductionFileID ID = FileID.FromStream(DL) as DeductionFileID;
                loaded = true;
                SetDeduction(MDeduction.GetUnloaded(DL, ID), false);
                loaded = false;
            }

            DL.Close();
        }
        private void LoadDeductionXML()
        {
            XMLDocumentLoader DL = Context.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(Context);
            DL.SetTheorem(this);

            XmlNode thm = DL.currentNode;
            if (thm.GetAttrBool("hasdeduction"))
            {
                XmlNode deduction = thm.GetChildNode("deduction");
                DeductionFileID ID = deduction.ID(DL) as DeductionFileID;
                loaded = true;
                SetDeduction(MDeduction.GetUnloaded(DL, deduction, ID), false);
                loaded = false;
            }
        }

        private void LoadStatement()
        {
            if (Context.Document.IsXML) { LoadStatementXML(); return; }

            DocumentLoader DL = Context.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(Context);
            DL.SetTheorem(this);

            DL.Remember();
            FileID ID = FileID.FromStream(DL);
            Statement = MStatement.GetUnloaded(DL, ID);

            DL.Close();
        }
        private void LoadStatementXML()
        {
            XMLDocumentLoader DL = Context.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(Context);
            DL.SetTheorem(this);

            XmlNode stat = DL.currentNode.GetChildNode("statement");
            Statement = MStatement.GetUnloaded(DL, stat, stat.ID(DL));
        }
    }

    public partial class MDeduction
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();
            fileID.ToStream(DL);

            DL.Write((ushort)Steps.Count);
            foreach (MDeductionStep ds in Steps)
            {
                DL.SetDeductionStep(ds);
                ds.ToStream(DL);
            }
            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("deduction");
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));
            DL.Xwr.WriteAttributeString("stepcount", Steps.Count.ToString());

            foreach (MDeductionStep ds in Steps)
            {
                DL.SetDeductionStep(ds);
                ds.ToXML(DL);
            }
            DL.Xwr.WriteEndElement(); //deduction
        }

        public new static MDeduction FromStream(DocumentLoader DL)
        {
            DL.Remember();
            DeductionFileID fileID = FileID.FromStream(DL) as DeductionFileID;

            MDeduction D = new MDeduction(fileID, DL.Context, DL.Theorem) {  };
            DL.SetDeduction(D);

            int dsCount = DL.ReadUShort();
            for (int i = 0; i < dsCount; i++)
            {
                MDeductionStep DS = MDeductionStep.FromStream(DL);
                D.AddStep(DS);
            }

            D.loaded = true;
            return D;
        }
        public new static MDeduction FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionFileID;

            MDeduction D = new MDeduction(fileID, DL.Context, DL.Theorem) { };
            DL.SetDeduction(D);

            int dsCount = node.GetAttrInt("stepcount");
            for (int i = 0; i < dsCount; i++)
            {
                MDeductionStep DS = MDeductionStep.FromStream(DL, node.ChildNodes[i]);
                D.AddStep(DS);
            }

            D.loaded = true;
            return D;
        }

        public void Load()
        {
            if (loaded) return;

            for (int i = 0; i < Steps.Count; i++)
                GetStep(i).Load();

            loaded = true;
        }

        public static MDeduction GetUnloaded(DocumentLoader DL, DeductionFileID ID)
        {
            MDeduction D = new MDeduction(ID.MakeDeductionFileID(), DL.Context, DL.Theorem) { };

            int dsCount = DL.ReadUShort();
            D.Steps = new MDeductionStep[dsCount].ToList();
            return D;
        }
        public static MDeduction GetUnloaded(XMLDocumentLoader DL, XmlNode node, DeductionFileID ID)
        {
            MDeduction D = new MDeduction(ID.MakeDeductionFileID(), DL.Context, DL.Theorem) { };

            int dsCount = node.GetAttrInt("stepcount");
            D.Steps = new MDeductionStep[dsCount].ToList();
            return D;
        }

        private void LoadStep(int index)
        {
            if (_X.Document.IsXML) { LoadStepXML(index); return; }

            DocumentLoader DL = _X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_X);
            DL.SetTheorem(Theorem);
            DL.SetDeduction(this);
            
            ushort dsCount = DL.ReadUShort();
            if (index >= dsCount) { DL.Close(); return; }

            for (int i = 0; i < index; i++)
            {
                DL.CleanSkip(); // skipping earlierer DeductionSteps
            }

            DL.Remember();
            DL.ReadByte(); //StepType
            DeductionStepFileID ID = FileID.FromStream(DL) as DeductionStepFileID;
            DL.Return();
            Steps[index] = MDeductionStep.GetUnloaded(DL);
            DL.Close();
        }
        private void LoadStepXML(int index)
        {
            XMLDocumentLoader DL = _X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_X);
            DL.SetTheorem(Theorem);
            DL.SetDeduction(this);

            XmlNode ded = DL.currentNode;
            int dsCount = ded.GetAttrInt("stepcount");
            if (index >= dsCount) { throw new Exception(); }

            XmlNode step = ded.GetChildNode("step", index);
            DeductionStepFileID ID = step.ID(DL) as DeductionStepFileID;
            Steps[index] = MDeductionStep.GetUnloaded(DL, step);
        }

        private MDeductionStep LoadStep(DeductionStepFileID ID)
        {
            if (_X.Document.IsXML) { return LoadStepXML(ID); }

            DocumentLoader DL = _X.Document.DL;
            int index = DL.Navigate(ID.MakeDeductionStepFileID());
            DL.SetContext(_X);
            DL.SetTheorem(Theorem);
            DL.SetDeduction(this);

            Steps[index] = MDeductionStep.GetUnloaded(DL);
            DL.Close();
            return Steps[index];
        }
        private MDeductionStep LoadStepXML (DeductionStepFileID ID)
        {
            XMLDocumentLoader DL = _X.Document.XDL;
            int index = DL.Navigate(ID.MakeDeductionStepFileID());
            DL.SetContext(_X);
            DL.SetTheorem(Theorem);
            DL.SetDeduction(this);

            Steps[index] = MDeductionStep.GetUnloaded(DL, DL.currentNode);
            return Steps[index];
        }
    }

    public partial class MDeductionStep
    {
        public new static MDeductionStep FromStream(DocumentLoader DL)
        {
            //prefix
            DL.Remember();

            MDeductionStep DS;
            int type = DL.ReadByte();

            switch (type)
            {
                case 1: // Specification (OBSOLETE)
                    //DS = MSpecificationDeductionStep.FromStream(DL);
                    throw new FileLoadException("Obsolete type of DeductionStep.");
                case 2: // PredicateSpecification
                    DS = MPredicateSpecificationDeductionStep.FromStream(DL);
                    break;
                case 3: // Variable Substitution
                    DS = MVariableSubstitutionDeductionStep.FromStream(DL);
                    break;
                case 4: // Trivialisation
                    DS = MTrivialisationDeductionStep.FromStream(DL);
                    break;
                case 5: // UniversalGeneralisation
                    DS = MUniversalGeneralisationDeductionStep.FromStream(DL);
                    break;
                case 6: // UniversalInstantiation
                    DS = MUniversalInstantiationDeductionStep.FromStream(DL);
                    break;
                case 7: // ExistentialGeneralisation
                    DS = MExistentialGeneralisationDeductionStep.FromStream(DL);
                    break;
                case 8: // ExistentialInstantiation
                    DS = MExistentialInstantiationDeductionStep.FromStream(DL);
                    break;
                case 9: // Formula Substitution
                    DS = MFormulaSubstitutionDeductionStep.FromStream(DL);
                    break;
                case 10: // Term Substitution
                    DS = MTermSubstitutionDeductionStep.FromStream(DL);
                    break;
                case 11: // Reductio Ad Absurdum
                    DS = MRAADeductionStep.FromStream(DL);
                    break;
                case 12: // Assumption
                    DS = MAssumptionDeductionStep.FromStream(DL);
                    break;
                case 13: // Condition Instantiation
                    DS = MConditionInstantiationDeductionStep.FromStream(DL);
                    break;
                default:
                    throw new FileLoadException("Unknown type of DeductionStep.");
            }

            DS.loaded = true;
            return DS;
        }
        public new static MDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            MDeductionStep DS;
            int type = node.GetAttrInt("type");

            switch (type)
            {
                case 1: // Specification (OBSOLETE)
                    //DS = MSpecificationDeductionStep.FromStream(DL);
                    throw new FileLoadException("Obsolete type of DeductionStep.");
                case 2: // PredicateSpecification
                    DS = MPredicateSpecificationDeductionStep.FromStream(DL, node);
                    break;
                case 3: // Variable Substitution
                    DS = MVariableSubstitutionDeductionStep.FromStream(DL, node);
                    break;
                case 4: // Trivialisation
                    DS = MTrivialisationDeductionStep.FromStream(DL, node);
                    break;
                case 5: // UniversalGeneralisation
                    DS = MUniversalGeneralisationDeductionStep.FromStream(DL, node);
                    break;
                case 6: // UniversalInstantiation
                    DS = MUniversalInstantiationDeductionStep.FromStream(DL, node);
                    break;
                case 7: // ExistentialGeneralisation
                    DS = MExistentialGeneralisationDeductionStep.FromStream(DL, node);
                    break;
                case 8: // ExistentialInstantiation
                    DS = MExistentialInstantiationDeductionStep.FromStream(DL, node);
                    break;
                case 9: // Formula Substitution
                    DS = MFormulaSubstitutionDeductionStep.FromStream(DL, node);
                    break;
                case 10: // Term Substitution
                    DS = MTermSubstitutionDeductionStep.FromStream(DL, node);
                    break;
                case 11: // Reductio Ad Absurdum
                    DS = MRAADeductionStep.FromStream(DL, node);
                    break;
                case 12: // Assumption
                    DS = MAssumptionDeductionStep.FromStream(DL, node);
                    break;
                case 13: // Condition Instantiation
                    DS = MConditionInstantiationDeductionStep.FromStream(DL, node);
                    break;
                default:
                    throw new FileLoadException("Unknown type of DeductionStep.");
            }

            DS.loaded = true;
            return DS;
        }

        public static MDeductionStep GetUnloaded(DocumentLoader DL)
        {
            MDeductionStep DS;
            int type = DL.ReadByte();

            switch (type)
            {
                case 1: // Specification (OBSOLETE)
                    //DS = MSpecificationDeductionStep.GetUnloaded(DL);
                    throw new FileLoadException("Obsolete type of DeductionStep.");
                case 2: // PredicateSpecification
                    DS = MPredicateSpecificationDeductionStep.GetUnloaded(DL);
                    break;
                case 3: // Variable Substitution
                    DS = MVariableSubstitutionDeductionStep.GetUnloaded(DL);
                    break;
                case 4: // Trivialisation
                    DS = MTrivialisationDeductionStep.GetUnloaded(DL);
                    break;
                case 5: // UniversalGeneralisation
                    DS = MUniversalGeneralisationDeductionStep.GetUnloaded(DL);
                    break;
                case 6: // UniversalInstantiation
                    DS = MUniversalInstantiationDeductionStep.GetUnloaded(DL);
                    break;
                case 7: // ExistentialGeneralisation
                    DS = MExistentialGeneralisationDeductionStep.GetUnloaded(DL);
                    break;
                case 8: // ExistentialInstantiation
                    DS = MExistentialInstantiationDeductionStep.GetUnloaded(DL);
                    break;
                case 9: // Formula Substitution
                    DS = MFormulaSubstitutionDeductionStep.GetUnloaded(DL);
                    break;
                case 10: // Term Substitution
                    DS = MTermSubstitutionDeductionStep.GetUnloaded(DL);
                    break;
                case 11: // Reductio Ad Absurdum
                    DS = MRAADeductionStep.GetUnloaded(DL);
                    break;
                case 12: // Assumption
                    DS = MAssumptionDeductionStep.GetUnloaded(DL);
                    break;
                case 13: // Condition Instantiation
                    DS = MConditionInstantiationDeductionStep.GetUnloaded(DL);
                    break;
                default:
                    throw new FileLoadException("Unknown type of DeductionStep.");
            }
            
            return DS;
        }
        public static MDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            MDeductionStep DS;
            int type = node.GetAttrInt("type"); ;

            switch (type)
            {
                case 1: // Specification (OBSOLETE)
                    //DS = MSpecificationDeductionStep.GetUnloaded(DL);
                    throw new FileLoadException("Obsolete type of DeductionStep.");
                case 2: // PredicateSpecification
                    DS = MPredicateSpecificationDeductionStep.GetUnloaded(DL, node);
                    break;
                case 3: // Variable Substitution
                    DS = MVariableSubstitutionDeductionStep.GetUnloaded(DL, node);
                    break;
                case 4: // Trivialisation
                    DS = MTrivialisationDeductionStep.GetUnloaded(DL, node);
                    break;
                case 5: // UniversalGeneralisation
                    DS = MUniversalGeneralisationDeductionStep.GetUnloaded(DL, node);
                    break;
                case 6: // UniversalInstantiation
                    DS = MUniversalInstantiationDeductionStep.GetUnloaded(DL, node);
                    break;
                case 7: // ExistentialGeneralisation
                    DS = MExistentialGeneralisationDeductionStep.GetUnloaded(DL, node);
                    break;
                case 8: // ExistentialInstantiation
                    DS = MExistentialInstantiationDeductionStep.GetUnloaded(DL, node);
                    break;
                case 9: // Formula Substitution
                    DS = MFormulaSubstitutionDeductionStep.GetUnloaded(DL, node);
                    break;
                case 10: // Term Substitution
                    DS = MTermSubstitutionDeductionStep.GetUnloaded(DL, node);
                    break;
                case 11: // Reductio Ad Absurdum
                    DS = MRAADeductionStep.GetUnloaded(DL, node);
                    break;
                case 12: // Assumption
                    DS = MAssumptionDeductionStep.GetUnloaded(DL, node);
                    break;
                case 13: // Condition Instantiation
                    DS = MConditionInstantiationDeductionStep.GetUnloaded(DL, node);
                    break;
                default:
                    throw new FileLoadException("Unknown type of DeductionStep.");
            }

            return DS;
        }

        protected void LoadConsequence()
        {
            if (_D._X.Document.IsXML) { LoadConsequenceXML(); return; }

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool(); // reduce 
            FileID.FromStream(DL); //premise

            DL.Remember();
            FileID ID = FileID.FromStream(DL);
            Consequence = MStatement.GetUnloaded(DL, ID);
            if(this is MAssumptionDeductionStep || this is MExistentialInstantiationDeductionStep) Consequence.UpdateAxiom(false);

            DL.Close();
        }
        protected void LoadConsequenceXML()
        {
            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode conseq = DL.currentNode.GetChildNode("consequence").GetChildNode("statement");
            FileID ID = conseq.ID(DL);
            Consequence = MStatement.GetUnloaded(DL, conseq, ID);
            if (this is MAssumptionDeductionStep || this is MExistentialInstantiationDeductionStep) Consequence.UpdateAxiom(false);
        }

        internal virtual void Load()
        {

        }
    }
    
    public partial class MPredicateSpecificationDeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();

            DL.Write((byte)2); // 2 - PredicateSpecification

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);

            DL.Write((ushort)PredicateSpecifications.Count);
            for (int i = 0; i < PredicateSpecifications.Count; i++)
            {
                byte[] path = Premise._F.GetSubPath(PredicateSpecifications.Left[i]);
                DL.Write((ushort)path.Length);
                for (int j = 0; j < path.Length; j++)
                    DL.Write(path[j]);
                DL.PDList = null;
                PredicateSpecifications.Right[i].ToStream(DL);
            }

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "2"); // 2 - PredicateSpecification
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence

            DL.Xwr.WriteStartElement("predspec");
            DL.Xwr.WriteAttributeString("count", PredicateSpecifications.Count.ToString());
            for (int i = 0; i < PredicateSpecifications.Count; i++)
            {
                byte[] path = Premise._F.GetSubPath(PredicateSpecifications.Left[i]);
                DL.Xwr.WriteStartElement("spec");
                DL.Xwr.WriteStartElement("path");
                DL.Xwr.WriteAttributeString("length", path.Length.ToString());
                for (int j = 0; j < path.Length; j++)
                    DL.Xwr.WriteString(path[j].ToString("X2"));
                DL.Xwr.WriteEndElement(); // path
                PredicateSpecifications.Right[i].ToXML(DL);
                DL.Xwr.WriteEndElement(); // spec
            }
            DL.Xwr.WriteEndElement(); //predspec

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MPredicateSpecificationDeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MPredicateSpecificationDeductionStep DS = new MPredicateSpecificationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);

            int psCount = DL.ReadUShort();
            for (int i = 0; i < psCount; i++)
            {
                int plength = DL.ReadUShort();
                byte[] path = new byte[plength];
                for (int j = 0; j < path.Length; j++)
                    path[j] = DL.ReadByte();
                MUndefinedPredicateFormula upred = DS.Premise.GetFormula().GetSub(path) as MUndefinedPredicateFormula;
                DL.PDList = null;
                MFormula form = MFormula.FromStream(DL);

                DS.CreatePredicateSpecification(upred, form, false);
            }

            return DS;
        }
        public new static MPredicateSpecificationDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MPredicateSpecificationDeductionStep DS = new MPredicateSpecificationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL, conseq), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);

            XmlNode predspec = node.GetChildNode("predspec");
            int psCount = predspec.GetAttrInt("count");
            for (int i = 0; i < psCount; i++)
            {
                XmlNode spec = predspec.ChildNodes[i];
                XmlNode p = spec.GetChildNode("path");
                int plength = p.GetAttrInt("length");
                byte[] path = new byte[plength];
                string pathtxt = p.InnerText;
                for (int j = 0; j < path.Length; j++)
                    path[j] = byte.Parse(pathtxt.Substring(j * 2, 2), System.Globalization.NumberStyles.HexNumber);
                MUndefinedPredicateFormula upred = DS.Premise.GetFormula().GetSub(path) as MUndefinedPredicateFormula;
                MFormula form = MFormula.FromStream(DL, spec.GetChildNode("expression"));

                DS.CreatePredicateSpecification(upred, form, false);
            }

            return DS;
        }

        public new static MPredicateSpecificationDeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            MPredicateSpecificationDeductionStep DS = new MPredicateSpecificationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }
        public new static MPredicateSpecificationDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MPredicateSpecificationDeductionStep DS = new MPredicateSpecificationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool(); // reduce

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence

            //data
            int psCount = DL.ReadUShort();
            for (int i = 0; i < psCount; i++)
            {
                int plength = DL.ReadUShort();
                byte[] path = new byte[plength];
                for (int j = 0; j < path.Length; j++)
                    path[j] = DL.ReadByte();
                MUndefinedPredicateFormula upred = Premise.GetFormula().GetSub(path) as MUndefinedPredicateFormula;
                DL.PDList = null;
                MFormula form = MFormula.FromStream(DL);

                CreatePredicateSpecification(upred, form, false);
            }

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            XmlNode data = step.GetChildNode("predspec");
            int psCount = data.Count();
            for (int i = 0; i < psCount; i++)
            {
                XmlNode spec = data.ChildNodes[i];
                XmlNode p = spec.GetChildNode("path");
                int plength = p.GetAttrInt("length");
                byte[] path = new byte[plength];
                string pathtxt = p.InnerText;
                for (int j = 0; j < path.Length; j++)
                    path[j] = byte.Parse(pathtxt.Substring(j * 2, 2), System.Globalization.NumberStyles.HexNumber);
                MUndefinedPredicateFormula upred = Premise.GetFormula().GetSub(path) as MUndefinedPredicateFormula;
                MFormula form = MFormula.FromStream(DL, spec.GetChildNode("expression"));

                CreatePredicateSpecification(upred, form, false);
            }
        }
    }

    public partial class MVariableSubstitutionDeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();

            DL.Write((byte)3); // 3 - Substitutions

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);

            VariableSubstitutions.ToStream(DL);

            DL.EndSubStream();
        }
        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "3"); // 3 - Substitutions
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence

            VariableSubstitutions.ToXML(DL);

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MVariableSubstitutionDeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MVariableSubstitutionDeductionStep DS = new MVariableSubstitutionDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);

            MVariableSubstitutions.FromStream(DL);

            return DS;
        }
        public new static MVariableSubstitutionDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MVariableSubstitutionDeductionStep DS = new MVariableSubstitutionDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL, conseq), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);

            MVariableSubstitutions.FromStream(DL, node.GetChildNode("substitutions"));

            return DS;
        }

        public new static MVariableSubstitutionDeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            MVariableSubstitutionDeductionStep DS = new MVariableSubstitutionDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce};

            return DS;
        }
        public new static MVariableSubstitutionDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MVariableSubstitutionDeductionStep DS = new MVariableSubstitutionDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool();  // reduce

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence

            //data
            MVariableSubstitutions.FromStream(DL);

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            MVariableSubstitutions.FromStream(DL, step.GetChildNode("substitutions"));
        }
    }

    public partial class MTrivialisationDeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();

            DL.Write((byte)4); // 4 - Trivialisation

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);
            DL.Write(Left);
            DL.Write((ushort)TrueFormulas.Count);
            for (int i = 0; i < TrueFormulas.Count; i++)
            {
                TrueFormulas[i].fileID.ToStream(DL);
            }

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "4"); // 4 - Trivialization
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence

            DL.Xwr.WriteStartElement("triv");
            DL.Attr("left", Left.ToString());
            DL.Xwr.WriteAttributeString("count", TrueFormulas.Count.ToString());
            for (int i = 0; i < TrueFormulas.Count; i++)
            {
                DL.Xwr.WriteElementString("tf", TrueFormulas[i].fileID.ToXML(DL.Document));
            }
            DL.Xwr.WriteEndElement(); //triv

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MTrivialisationDeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MTrivialisationDeductionStep DS = new MTrivialisationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL), Reduce = reduce, loaded = true, Left = DL.ReadBool() };
            DL.SetDeductionStep(DS);
            
            int tCount = DL.ReadUShort();
            for (int i = 0; i < tCount; i++)
            {
                MStatement tr = FileID.FromStream(DL).FindStatement(DL);
                DS.CreateTrivialisation(tr, false);
            }

            return DS;
        }
        public new static MTrivialisationDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MTrivialisationDeductionStep DS = new MTrivialisationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL, conseq), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);

            XmlNode trivs = node.GetChildNode("triv");
            DS.Left = trivs.GetAttrBool("left");
            int tCount = trivs.GetAttrInt("count");
            for (int i = 0; i < tCount; i++)
            {
                XmlNode triv = trivs.ChildNodes[i];
                MStatement tr = FileID.FromStream(DL, triv.InnerText).FindStatement(DL);
                DS.CreateTrivialisation(tr, false);
            }

            return DS;
        }

        public new static MTrivialisationDeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            FileID.FromStream(DL); // premise
            DL.CleanSkip(); // consequence

            MTrivialisationDeductionStep DS = new MTrivialisationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce, Left = DL.ReadBool() };

            return DS;
        }
        public new static MTrivialisationDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MTrivialisationDeductionStep DS = new MTrivialisationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce, Left = node.GetChildNode("triv").GetAttrBool("left") };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool(); // reduce

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence
            Left = DL.ReadBool();

            //data
            int tCount = DL.ReadUShort();
            for (int i = 0; i < tCount; i++)
            {
                MStatement tr = FileID.FromStream(DL).FindStatement(DL);
                CreateTrivialisation(tr, false);
            }

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            XmlNode trivs = step.GetChildNode("triv");
            Left = trivs.GetAttrBool("left");
            int tCount = trivs.GetAttrInt("count");
            for (int i = 0; i < tCount; i++)
            {
                XmlNode triv = trivs.ChildNodes[i];
                MStatement tr = FileID.FromStream(DL, triv.InnerText).FindStatement(DL);
                CreateTrivialisation(tr, false);
            }
        }
    }

    public partial class MUniversalGeneralisationDeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();

            DL.Write((byte)5); // 5 - Universal Generalisation

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);

            Quantifier.fileID.ToStream(DL);
            Old.ToStream(DL);
            New.ToStream(DL);

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "5"); // 5 - Universal Generalization
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence

            DL.Xwr.WriteElementString("quantifier", Quantifier.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("old"); Old.ToXML(DL); DL.Xwr.WriteEndElement();
            DL.Xwr.WriteStartElement("new"); New.ToXML(DL); DL.Xwr.WriteEndElement();

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MUniversalGeneralisationDeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MUniversalGeneralisationDeductionStep DS = new MUniversalGeneralisationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL), Reduce = reduce, loaded =true };
            DL.SetDeductionStep(DS);

            DS.Quantifier = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            DS.Old = MTerm.FromStream(DL) as MVariable;
            DS.New = MTerm.FromStream(DL) as MVariable;

            return DS;
        }
        public new static MUniversalGeneralisationDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MUniversalGeneralisationDeductionStep DS = new MUniversalGeneralisationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL, conseq), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);

            XmlNode Qnode = node.GetChildNode("quantifier");
            DS.Quantifier = (FileID.FromStream(DL, Qnode.InnerText) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            DS.Old = MTerm.FromStream(DL, node.GetChildNode("old")) as MVariable;
            DS.New = MTerm.FromStream(DL, node.GetChildNode("new")) as MVariable;

            return DS;
        }

        public new static MUniversalGeneralisationDeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            MUniversalGeneralisationDeductionStep DS = new MUniversalGeneralisationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }
        public new static MUniversalGeneralisationDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MUniversalGeneralisationDeductionStep DS = new MUniversalGeneralisationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool(); DL.ReadBool(); // reduce and valid

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence

            //data
            Quantifier = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            Old = MTerm.FromStream(DL) as MVariable;
            New = MTerm.FromStream(DL) as MVariable;

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            XmlNode Qnode = step.GetChildNode("quantifier");
            Quantifier = (FileID.FromStream(DL, Qnode.InnerText) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            Old = MTerm.FromStream(DL, step.GetChildNode("old")) as MVariable;
            New = MTerm.FromStream(DL, step.GetChildNode("new")) as MVariable;
        }
    }

    public partial class MUniversalInstantiationDeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();

            DL.Write((byte)6); // 6 - Universal Instantiation

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);

            Quantifier.fileID.ToStream(DL);
            T.ToStream(DL);

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "6"); // 6 - Universal Instantiation
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence

            DL.Xwr.WriteElementString("quantifier", Quantifier.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("term"); T.ToXML(DL); DL.Xwr.WriteEndElement();

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MUniversalInstantiationDeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MUniversalInstantiationDeductionStep DS = new MUniversalInstantiationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);

            DS.Quantifier = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            DS.T = MTerm.FromStream(DL);

            return DS;
        }
        public new static MUniversalInstantiationDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MUniversalInstantiationDeductionStep DS = new MUniversalInstantiationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL, conseq), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);

            DS.Quantifier = (FileID.FromStream(DL, node.GetChildNode("quantifier").InnerText) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            DS.T = MTerm.FromStream(DL, node.GetChildNode("term"));

            return DS;
        }

        public new static MUniversalInstantiationDeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            MUniversalInstantiationDeductionStep DS = new MUniversalInstantiationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }
        public new static MUniversalInstantiationDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MUniversalInstantiationDeductionStep DS = new MUniversalInstantiationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool(); // reduce

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence

            //data
            Quantifier = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            T = MTerm.FromStream(DL);

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            Quantifier = (FileID.FromStream(DL, step.GetChildNode("quantifier").InnerText) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            T = MTerm.FromStream(DL, step.GetChildNode("term"));
        }
    }

    public partial class MExistentialGeneralisationDeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();

            DL.Write((byte)7); // 7 - Existential Generalisation

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);

            Quantifier.fileID.ToStream(DL);
            Old.ToStream(DL);
            New.ToStream(DL);

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "7"); // 7 - Existential Generalization
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence

            DL.Xwr.WriteElementString("quantifier", Quantifier.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("old"); Old.ToXML(DL); DL.Xwr.WriteEndElement();
            DL.Xwr.WriteStartElement("new"); New.ToXML(DL); DL.Xwr.WriteEndElement();

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MExistentialGeneralisationDeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MExistentialGeneralisationDeductionStep DS = new MExistentialGeneralisationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);

            DS.Quantifier = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            DS.Old = MTerm.FromStream(DL);
            DS.New = MTerm.FromStream(DL) as MVariable;

            return DS;
        }
        public new static MExistentialGeneralisationDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MExistentialGeneralisationDeductionStep DS = new MExistentialGeneralisationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL, conseq), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);

            DS.Quantifier = (FileID.FromStream(DL, node.GetChildNode("quantifier").InnerText) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            DS.Old = MTerm.FromStream(DL, node.GetChildNode("old"));
            DS.New = MTerm.FromStream(DL, node.GetChildNode("new")) as MVariable;

            return DS;
        }

        public new static MExistentialGeneralisationDeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            MExistentialGeneralisationDeductionStep DS = new MExistentialGeneralisationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }
        public new static MExistentialGeneralisationDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MExistentialGeneralisationDeductionStep DS = new MExistentialGeneralisationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool(); //reduce

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence

            //data
            Quantifier = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            Old = MTerm.FromStream(DL);
            New = MTerm.FromStream(DL) as MVariable;

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            Quantifier = (FileID.FromStream(DL, step.GetChildNode("quantifier").InnerText) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            Old = MTerm.FromStream(DL, step.GetChildNode("old"));
            New = MTerm.FromStream(DL, step.GetChildNode("new")) as MVariable;
        }
    }

    public partial class MExistentialInstantiationDeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");
            DL.BeginSubStream();

            DL.Write((byte)8); // 8 - Existential Instantiation

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);

            Quantifier.fileID.ToStream(DL);
            V.ToStream(DL);

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "8"); // 8 - Existential Instantiation
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence

            DL.Xwr.WriteElementString("quantifier", Quantifier.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("term"); V.ToXML(DL); DL.Xwr.WriteEndElement();

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MExistentialInstantiationDeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MExistentialInstantiationDeductionStep DS = new MExistentialInstantiationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);
            DS.Consequence.UpdateAxiom(false);

            DS.Quantifier = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            DS.V = MTerm.FromStream(DL) as MVariable;

            return DS;
        }
        public new static MExistentialInstantiationDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MExistentialInstantiationDeductionStep DS = new MExistentialInstantiationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL, conseq), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);
            DS.Consequence.UpdateAxiom(false);

            DS.Quantifier = (FileID.FromStream(DL, node.GetChildNode("quantifier").InnerText) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            DS.V = MTerm.FromStream(DL, node.GetChildNode("term")) as MVariable;

            return DS;
        }

        public new static MExistentialInstantiationDeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            MExistentialInstantiationDeductionStep DS = new MExistentialInstantiationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }
        public new static MExistentialInstantiationDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MExistentialInstantiationDeductionStep DS = new MExistentialInstantiationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool();  // reduce 

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence

            //data
            Quantifier = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            V = MTerm.FromStream(DL) as MVariable;

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            Quantifier = (FileID.FromStream(DL, step.GetChildNode("quantifier").InnerText) as DefinitionFileID).FindDefinition(DL) as MQuantifier;
            V = MTerm.FromStream(DL, step.GetChildNode("term")) as MVariable;
        }
    }

    public partial class MFormulaSubstitutionDeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();

            DL.Write((byte)9); // 9 - Formula Substitution

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);
            
            DL.PDList = null;
            Old.ToStream(DL);
            DL.PDList = null;
            New.ToStream(DL);
            Justification.fileID.ToStream(DL);

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "9"); // 9 - Formula Substitution
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence
             
            DL.Xwr.WriteStartElement("old"); Old.ToXML(DL); DL.Xwr.WriteEndElement();
            DL.Xwr.WriteStartElement("new"); New.ToXML(DL); DL.Xwr.WriteEndElement();
            DL.Xwr.WriteElementString("justification", Justification.fileID.ToXML(DL.Document));

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MFormulaSubstitutionDeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MFormulaSubstitutionDeductionStep DS = new MFormulaSubstitutionDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);
            
            DL.PDList = null;
            MFormula Old = MFormula.FromStream(DL);
            DL.PDList = null;
            MFormula New = MFormula.FromStream(DL);
            MStatement Justification = FileID.FromStream(DL).FindStatement(DL);
            DS.Substitute(Old, New, Justification, false);

            return DS;
        }
        public new static MFormulaSubstitutionDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MFormulaSubstitutionDeductionStep DS = new MFormulaSubstitutionDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL, conseq), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);
            
            MFormula Old = MFormula.FromStream(DL, node.GetChildNode("old"));
            MFormula New = MFormula.FromStream(DL, node.GetChildNode("new"));
            MStatement Justification = FileID.FromStream(DL, node.GetChildNode("justification").InnerText).FindStatement(DL);
            DS.Substitute(Old, New, Justification, false);

            return DS;
        }

        public new static MFormulaSubstitutionDeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            MFormulaSubstitutionDeductionStep DS = new MFormulaSubstitutionDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }
        public new static MFormulaSubstitutionDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MFormulaSubstitutionDeductionStep DS = new MFormulaSubstitutionDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool();  // reduce

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence

            //data
            DL.PDList = null;
            MFormula Old = MFormula.FromStream(DL);
            DL.PDList = null;
            MFormula New = MFormula.FromStream(DL);
            MStatement Just = FileID.FromStream(DL).FindStatement(DL);
            Substitute(Old, New, Just, false);

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            MFormula Old = MFormula.FromStream(DL, step.GetChildNode("old"));
            MFormula New = MFormula.FromStream(DL, step.GetChildNode("new"));
            MStatement Justification = FileID.FromStream(DL, step.GetChildNode("justification").InnerText).FindStatement(DL);
            Substitute(Old, New, Justification, false);
        }
    }

    public partial class MTermSubstitutionDeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();

            DL.Write((byte)10); // 10 - Term Substitution

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);
            
            DL.PDList = null;
            Old.ToStream(DL);
            DL.PDList = null;
            New.ToStream(DL);
            Justification.fileID.ToStream(DL);

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "10"); // 10 - Term Substitution
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence

            DL.Xwr.WriteStartElement("old"); Old.ToXML(DL); DL.Xwr.WriteEndElement();
            DL.Xwr.WriteStartElement("new"); New.ToXML(DL); DL.Xwr.WriteEndElement();
            DL.Xwr.WriteElementString("justification", Justification.fileID.ToXML(DL.Document));

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MTermSubstitutionDeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MTermSubstitutionDeductionStep DS = new MTermSubstitutionDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);
            
            DL.PDList = null;
            MTerm Old = MTerm.FromStream(DL);
            DL.PDList = null;
            MTerm New = MTerm.FromStream(DL);
            MStatement Justification = FileID.FromStream(DL).FindStatement(DL);
            DS.Substitute(Old, New, Justification, false);

            return DS;
        }
        public new static MTermSubstitutionDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MTermSubstitutionDeductionStep DS = new MTermSubstitutionDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL, conseq), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);
            
            MTerm Old = MTerm.FromStream(DL, node.GetChildNode("old"));
            MTerm New = MTerm.FromStream(DL, node.GetChildNode("new"));
            MStatement Justification = FileID.FromStream(DL, node.GetChildNode("justification").InnerText).FindStatement(DL);
            DS.Substitute(Old, New, Justification, false);

            return DS;
        }

        public new static MTermSubstitutionDeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            MTermSubstitutionDeductionStep DS = new MTermSubstitutionDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }
        public new static MTermSubstitutionDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MTermSubstitutionDeductionStep DS = new MTermSubstitutionDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool(); // reduce 

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence

            //data
            DL.PDList = null;
            MTerm Old = MTerm.FromStream(DL);
            DL.PDList = null;
            MTerm New = MTerm.FromStream(DL);
            MStatement Just = FileID.FromStream(DL).FindStatement(DL);
            Substitute(Old, New, Just, false);

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            MTerm Old = MTerm.FromStream(DL, step.GetChildNode("old"));
            MTerm New = MTerm.FromStream(DL, step.GetChildNode("new"));
            MStatement Justification = FileID.FromStream(DL, step.GetChildNode("justification").InnerText).FindStatement(DL);
            Substitute(Old, New, Justification, false);
        }
    }

    public partial class MRAADeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();

            DL.Write((byte)11); // 11 - Reductio Ad Absurdum

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);

            DL.PDList = null;
            Condition.fileID.ToStream(DL);

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "11"); // 11 - Reductio Ad Absurdum
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence
            
            DL.Xwr.WriteElementString("condition", Condition.fileID.ToXML(DL.Document));

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MRAADeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MRAADeductionStep DS = new MRAADeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);

            DL.PDList = null;
            MStatement Condition = FileID.FromStream(DL).FindStatement(DL);
            DS.RAA(Condition, false);

            return DS;
        }
        public new static MRAADeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MRAADeductionStep DS = new MRAADeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL, conseq), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);
            
            MStatement Condition = FileID.FromStream(DL, node.GetChildNode("condition").InnerText).FindStatement(DL);
            DS.RAA(Condition, false);

            return DS;
        }

        public new static MRAADeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            MRAADeductionStep DS = new MRAADeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }
        public new static MRAADeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MRAADeductionStep DS = new MRAADeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool(); // reduce 

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence

            //data
            DL.PDList = null;
            MStatement Cond = FileID.FromStream(DL).FindStatement(DL);
            RAA(Cond, false);

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            MStatement Condition = FileID.FromStream(DL, step.GetChildNode("condition").InnerText).FindStatement(DL);
            RAA(Condition, false);
        }
    }

    public partial class MAssumptionDeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();

            DL.Write((byte)12); // 12 - Assumption

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);

            DL.PDList = null;
            Assumption.ToStream(DL);
            DL.Write((byte)RestrictedVars.Count);
            for (int i = 0; i < RestrictedVars.Count; i++)
                DL.Write((byte)RestrictedVars[i]);

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "12"); // 12 - Assumption
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence

            DL.Xwr.WriteStartElement("assumption");
            Assumption.ToXML(DL);
            DL.StartEl("restricted");
            DL.Attr("count", RestrictedVars.Count.ToString());
            string restr = "";
            for (int i = 0; i < RestrictedVars.Count; i++)
                restr += RestrictedVars[i].ToString("X2");
            DL.Xwr.WriteString(restr);
            DL.EndEl(); //restricted
            DL.Xwr.WriteEndElement(); // assumption

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MAssumptionDeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MAssumptionDeductionStep DS = new MAssumptionDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);
            DS.Consequence.UpdateAxiom(false);

            DL.PDList = null;
            MFormula Ass = MFormula.FromStream(DL);
            byte RestrCount = DL.ReadByte();
            List<int> RVars = new List<int>();
            for (int i = 0; i < RestrCount; i++)
                RVars.Add((int)DL.ReadByte());
            DS.Assume(Ass, RVars, false);

            return DS;
        }
        public new static MAssumptionDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MAssumptionDeductionStep DS = new MAssumptionDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            { Consequence = MStatement.FromStream(DL, conseq), Reduce = reduce, loaded = true };
            DL.SetDeductionStep(DS);
            DS.Consequence.UpdateAxiom(false);

            XmlNode assumption = node.GetChildNode("assumption");
            MFormula Ass = MFormula.FromStream(DL, assumption);
            XmlNode restricted = assumption.GetChildNode("restricted");
            int rCount = restricted.Count();
            List<int> RVars = new List<int>();
            for (int i = 0; i<rCount; i++)
            {
                RVars.Add(byte.Parse(restricted.InnerText.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber));
            }
            DS.Assume(Ass, RVars, false);

            return DS;
        }

        public new static MAssumptionDeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            MAssumptionDeductionStep DS = new MAssumptionDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }
        public new static MAssumptionDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MAssumptionDeductionStep DS = new MAssumptionDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool(); // reduce 

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence

            //data
            DL.PDList = null;
            MFormula Ass = MFormula.FromStream(DL);
            byte RestrCount = DL.ReadByte();
            List<int> RVars = new List<int>();
            for (int i = 0; i < RestrCount; i++)
                RVars.Add((int)DL.ReadByte());
            Assume(Ass, RVars, false);

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            XmlNode assumption = step.GetChildNode("assumption");
            MFormula Ass = MFormula.FromStream(DL, assumption);
            XmlNode restricted = assumption.GetChildNode("restricted");
            int rCount = restricted.Count();
            List<int> RVars = new List<int>();
            for (int i = 0; i < rCount; i++)
            {
                RVars.Add(byte.Parse(restricted.InnerText.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber));
            }
            Assume(Ass, RVars, false);
        }
    }

    public partial class MConditionInstantiationDeductionStep
    {
        public override void ToStream(DocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.BeginSubStream();

            DL.Write((byte)13); // 13 - Condition Instantiation

            fileID.ToStream(DL);

            DL.Write(Reduce);
            Premise.fileID.ToStream(DL);
            Consequence.ToStream(DL);
            DL.Write(ImplicationNull);
            if(!ImplicationNull)
            Implication.fileID.ToStream(DL);

            DL.PDList = null;
            Condition.fileID.ToStream(DL);

            DL.EndSubStream();
        }

        public override void ToXML(XMLDocumentLoader DL)
        {
            if (!loaded) throw new InvalidOperationException("Editing or saving unloaded Objects is prohibited.");

            DL.Xwr.WriteStartElement("step");
            DL.Xwr.WriteAttributeString("type", "13"); // 13 - Condition Instantiation
            DL.Xwr.WriteAttributeString("id", fileID.ToXML(DL.Document));

            DL.Xwr.WriteElementString("reduce", Reduce.ToString());
            DL.Xwr.WriteElementString("premise", Premise.fileID.ToXML(DL.Document));
            DL.Xwr.WriteStartElement("consequence");
            Consequence.ToXML(DL);
            DL.Xwr.WriteEndElement(); //consequence

            if (!ImplicationNull)
                DL.Xwr.WriteElementString("implication", Implication.fileID.ToXML(DL.Document));
            DL.Xwr.WriteElementString("condition", Condition.fileID.ToXML(DL.Document));

            DL.Xwr.WriteEndElement(); //step
        }

        public new static MConditionInstantiationDeductionStep FromStream(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;

            bool reduce = DL.ReadBool();
            FileID premiseFileID = FileID.FromStream(DL);

            MConditionInstantiationDeductionStep DS = new MConditionInstantiationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            {
                Consequence = MStatement.FromStream(DL),
                ImplicationNull = DL.ReadBool(),
                Reduce = reduce,
                loaded = true
            };

            DS.Implication = DS.ImplicationNull ? null : (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MBinaryConnective;
            DL.SetDeductionStep(DS);

            DL.PDList = null;
            MStatement Condition = FileID.FromStream(DL).FindStatement(DL);
            DS.Instantiate(Condition, invalidate:false);

            return DS;
        }
        public new static MConditionInstantiationDeductionStep FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL, node.GetAttr("id")) as DeductionStepFileID;

            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);
            FileID premiseFileID = FileID.FromStream(DL, node.GetChildNode("premise").InnerText);
            XmlNode conseq = node.GetChildNode("consequence").FirstChild;

            MConditionInstantiationDeductionStep DS = new MConditionInstantiationDeductionStep(fileID, DL.Deduction, premiseFileID.FindStatement(DL))
            {
                Consequence = MStatement.FromStream(DL, conseq),
                ImplicationNull = !node.HasChildNode("implication"),
                Reduce = reduce,
                loaded = true
            };

            DS.Implication = DS.ImplicationNull ? null : (FileID.FromStream(DL, node.GetChildNode("implication").InnerText) as DefinitionFileID).FindDefinition(DL) as MBinaryConnective;
            DL.SetDeductionStep(DS);
            
            MStatement Condition = FileID.FromStream(DL, node.GetChildNode("condition").InnerText).FindStatement(DL);
            DS.Instantiate(Condition, invalidate: false);

            return DS;
        }

        public new static MConditionInstantiationDeductionStep GetUnloaded(DocumentLoader DL)
        {
            DeductionStepFileID fileID = FileID.FromStream(DL) as DeductionStepFileID;
            bool reduce = DL.ReadBool();

            MConditionInstantiationDeductionStep DS = new MConditionInstantiationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }
        public new static MConditionInstantiationDeductionStep GetUnloaded(XMLDocumentLoader DL, XmlNode node)
        {
            DeductionStepFileID fileID = node.ID(DL) as DeductionStepFileID;
            bool reduce = bool.Parse(node.GetChildNode("reduce").InnerText);

            MConditionInstantiationDeductionStep DS = new MConditionInstantiationDeductionStep(fileID, DL.Deduction, null)
            { Reduce = reduce };

            return DS;
        }

        internal override void Load()
        {
            if (_D._X.Document.IsXML) { LoadXML(); return; }

            if (Consequence == null) LoadConsequence();
            loaded = true;

            DocumentLoader DL = _D._X.Document.DL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            DL.ReadByte(); // type
            FileID.FromStream(DL); //this file ID
            DL.ReadBool(); // reduce 

            Premise = FileID.FromStream(DL).FindStatement(DL); //premise

            DL.CleanSkip(); // Consequence

            //data
            DL.PDList = null;
            ImplicationNull = DL.ReadBool();
            if(!ImplicationNull) Implication = (FileID.FromStream(DL) as DefinitionFileID).FindDefinition(DL) as MBinaryConnective;
            DL.PDList = null;
            MStatement Cond = FileID.FromStream(DL).FindStatement(DL);
            Instantiate(Cond, invalidate: false);

            DL.Close();
        }
        internal void LoadXML()
        {
            if (Consequence == null) LoadConsequence();
            loaded = true;

            XMLDocumentLoader DL = _D._X.Document.XDL;
            DL.Navigate(fileID);
            DL.SetContext(_D._X);
            DL.SetTheorem(_D.Theorem);
            DL.SetDeduction(_D);
            DL.SetDeductionStep(this);

            XmlNode step = DL.currentNode;

            Premise = FileID.FromStream(DL, step.GetChildNode("premise").InnerText).FindStatement(DL); //premise

            //data
            Implication = ImplicationNull ? null : (FileID.FromStream(DL, step.GetChildNode("implication").InnerText) as DefinitionFileID).FindDefinition(DL) as MBinaryConnective;
            DL.SetDeductionStep(this);

            MStatement Condition = FileID.FromStream(DL, step.GetChildNode("condition").InnerText).FindStatement(DL);
            Instantiate(Condition, invalidate: false);
        }
    }

    public partial class MVariableSubstitutions
    {
        internal void ToStream(DocumentLoader DL)
        {
            DL.Write((ushort)Substitutions.Count);
            for (int i = 0; i < Substitutions.Count; i++)
            {
                Substitutions.Left[i].fileID.ToStream(DL);
                Substitutions.Right[i].ToStream(DL);
                Justifications[i].ToStream(DL);
            }
        }

        internal void ToXML(XMLDocumentLoader DL)
        {
            DL.Xwr.WriteStartElement("substitutions");
            DL.Xwr.WriteAttributeString("count", Substitutions.Count.ToString());
            for (int i = 0; i < Substitutions.Count; i++)
            {
                DL.Xwr.WriteStartElement("subs");
                DL.Xwr.WriteElementString("l", Substitutions.Left[i].fileID.ToXML(DL.Document));
                Substitutions.Right[i].ToXML(DL);
                Justifications[i].ToXML(DL);
                DL.Xwr.WriteEndElement(); //subs
            }
            DL.Xwr.WriteEndElement(); //substitutions
        }

        internal static void FromStream(DocumentLoader DL)
        {
            MVariableSubstitutions Subs = (DL.DeductionStep as MVariableSubstitutionDeductionStep).VariableSubstitutions;
            ushort count = DL.ReadUShort();

            for (int i = 0; i < count; i++)
            {
                MVariable Old = (FileID.FromStream(DL) as VariableFileID).FindVariable(DL);
                MTerm New = MTerm.FromStream(DL);
                MSubstitutionJustification J = MSubstitutionJustification.FromStream(DL, Old, New);
                Subs.Substitute(Old, New, J);
            }
        }
        internal static void FromStream(XMLDocumentLoader DL, XmlNode node)
        {
            MVariableSubstitutions Subs = (DL.DeductionStep as MVariableSubstitutionDeductionStep).VariableSubstitutions;
            int count = node.GetAttrInt("count");

            for (int i = 0; i < count; i++)
            {
                XmlNode subs = node.ChildNodes[i];
                MVariable Old = (FileID.FromStream(DL, subs.GetChildNode("l").InnerText) as VariableFileID).FindVariable(DL);
                MTerm New = MTerm.FromStream(DL, subs.ChildNodes[1]);
                MSubstitutionJustification J = MSubstitutionJustification.FromStream(DL, subs.ChildNodes[2], Old, New);
                Subs.Substitute(Old, New, J);
            }
        }
    }

    public partial class MSubstitutionJustification
    {
        internal virtual void ToStream(DocumentLoader DL)
        {
            throw new MissingMethodException("ToStream should not be called on MSubstitutionJustification directly, only on Children.");
        }

        internal virtual void ToXML(XMLDocumentLoader DL)
        {
            throw new MissingMethodException("ToXML should not be called on MSubstitutionJustification directly, only on Children.");
        }

        internal static MSubstitutionJustification FromStream(DocumentLoader DL, MVariable o, MTerm n)
        {
            byte type = DL.ReadByte();
            MSubstitutionJustification J;
            switch (type)
            {
                case 0:
                    J = MAxiomaticSubstitutionJustification.FromStream(DL, o, n);
                    break;
                case 1:
                    J = MEqualitySubstitutionJustification.FromStream(DL, o, n);
                    break;
                default:
                    throw new FileLoadException("Unknown Type of SubstitutionJustification.");
            }
            return J;
        }
        internal static MSubstitutionJustification FromStream(XMLDocumentLoader DL, XmlNode node, MVariable o, MTerm n)
        {
            int type = node.GetAttrInt("t");
            MSubstitutionJustification J;
            switch (type)
            {
                case 0:
                    J = MAxiomaticSubstitutionJustification.FromStream(DL, node, o, n);
                    break;
                case 1:
                    J = MEqualitySubstitutionJustification.FromStream(DL, node, o, n);
                    break;
                default:
                    throw new FileLoadException("Unknown Type of SubstitutionJustification.");
            }
            return J;
        }
    }

    public partial class MAxiomaticSubstitutionJustification
    {
        internal override void ToStream(DocumentLoader DL)
        {
            DL.Write((byte)0); // 0 - Axiomatic
        }

        internal override void ToXML(XMLDocumentLoader DL)
        {
            DL.Xwr.WriteStartElement("j");
            DL.Xwr.WriteAttributeString("t", "0"); // 0 - Axiomatic
            DL.Xwr.WriteEndElement();
        }

        internal new static MAxiomaticSubstitutionJustification FromStream(DocumentLoader DL, MVariable o, MTerm n)
        {
            MAxiomaticSubstitutionJustification J = new MAxiomaticSubstitutionJustification((DL.DeductionStep as MVariableSubstitutionDeductionStep).VariableSubstitutions, o, n);
            return J;
        }
        internal new static MAxiomaticSubstitutionJustification FromStream(XMLDocumentLoader DL, XmlNode node, MVariable o, MTerm n)
        {
            MAxiomaticSubstitutionJustification J = new MAxiomaticSubstitutionJustification((DL.DeductionStep as MVariableSubstitutionDeductionStep).VariableSubstitutions, o, n);
            return J;
        }
    }

    public partial class MEqualitySubstitutionJustification
    {
        internal override void ToStream(DocumentLoader DL)
        {
            DL.Write((byte)1); // 1 - Equality

            Equality.fileID.ToStream(DL);
        }

        internal override void ToXML(XMLDocumentLoader DL)
        {
            DL.Xwr.WriteStartElement("j");
            DL.Xwr.WriteAttributeString("t", "1"); // 1 - Equality

            DL.Xwr.WriteElementString("eq", Equality.fileID.ToXML(DL.Document));
            DL.Xwr.WriteEndElement();
        }

        internal new static MEqualitySubstitutionJustification FromStream(DocumentLoader DL, MVariable o, MTerm n)
        {
            MEqualitySubstitutionJustification J = new MEqualitySubstitutionJustification((DL.DeductionStep as MVariableSubstitutionDeductionStep).VariableSubstitutions, o, n);
            MStatement eq = FileID.FromStream(DL).FindStatement(DL);
            J.Justify(eq);
            return J;
        }
        internal new static MEqualitySubstitutionJustification FromStream(XMLDocumentLoader DL, XmlNode node, MVariable o, MTerm n)
        {
            MEqualitySubstitutionJustification J = new MEqualitySubstitutionJustification((DL.DeductionStep as MVariableSubstitutionDeductionStep).VariableSubstitutions, o, n);
            MStatement eq = FileID.FromStream(DL, node.GetChildNode("eq").InnerText).FindStatement(DL);
            J.Justify(eq);
            return J;
        }
    }

}
