# Trace
Trace is a work-in-progress Mathematical WYSIWYG-notebook with proof verification.
A document consists of Definitions, Axioms, and Theorems. Additionally, rich-text annotations can be made anywhere. 
Simple theorems which can be proved by checking truth-tables need no proof, and are automatically verified:

![alt text](https://github.com/julfarn/trace/blob/main/Trace_screen_tautologiees.PNG?raw=true)

In general however, a proof is a chain of <i>Deduction Steps</i>. In the following example, the famous statement that Russel's class is not a set ([Russel's paradox](https://en.wikipedia.org/wiki/Russell%27s_paradox)) is proved in 10 Deduction Steps. 
There are several Types of Deduction Steps; one of them is <i>Deduction by Universal Instantiation</i>, which is presented in expanded form below.

![alt text](https://github.com/julfarn/trace/blob/main/Trace_screen_russel.PNG?raw=true)

Universal Instantiation allows one to obtain a concrete statement from a "For All"-statement. In this case, the <i>premise</i> of the Deduction Step was a statement about all variables x, and the <i>consequence</i> is a statement about the specific variable Ru. 
The particular proof shown is a proof by contradiction, which means that an assumption was made. Assumptions, and statements deduced from assumptions, are marked blue in Trace. 
The very first Deduction Step is an assumption, and we later arrive (in the second-to-last step) at a contradiction: We managed to prove the statement "False". Thus, the negation of the assumption must be true, which is the consequence of the last deduction step. 
A proof is complete if the consequence of the last deduction step matches the statement of the theorem. 

Trace aims to produce both readable and easily editable documents. For this reason, its environment for displaying and editing mathematical expressions is built from scratch, without relying on systems such as LaTeX. It is, however, possible to export a trace document to .tex. 

![alt text](https://github.com/julfarn/trace/blob/main/Trace_screen_expressioneditor.png?raw=true)

When making a new definition, the user is given wide control over the appearance of the new, say, function by means of <i>Visualizations</i>. Symbols and arguments can be freely arranged, and if needed, there is an integrated vector-based symbol editor. 

![alt text](https://github.com/julfarn/trace/blob/main/Trace_screen_symboleditor.PNG?raw=true)
