using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SRP
{

    class Table
    {
        private Dictionary<string, string[]> actionState;
        private Dictionary<string, string[]> goTO;
        private Dictionary<int, string[]> grammarRule; 

        public Table()
        {
            actionState = new Dictionary<string, string[]>();
            actionState.Add("id",new string[] { "S5", null,     null, null, "S5", null, "S5", "S5", null, null, null, null });
            actionState.Add("+", new string[] { null, "S6",     "R2", "R4", null, "R6", null, null, "S6", "R1", "R3", "R5" });
            actionState.Add("*", new string[] { null, null,     "S7", "R4", null, "R6", null, null, null, "S7", "R3", "R5" });
            actionState.Add("(", new string[] { "S4", null,     null, null, "S4", null, "S4", "S4", null, null, null, null });
            actionState.Add(")", new string[] { null, null,     "R2", "R4", null, "R6", null, null, "S11", "R1", "R3", "R5" });
            actionState.Add("$", new string[] { null, "accept", "R2", "R4", null, "R6", null, null, null, "R1", "R3", "R5" });
            
            goTO = new Dictionary<string, string[]>();
            goTO.Add("E", new string[] { "1", null, null, null, "8", null, null, null, null, null, null, null});
            goTO.Add("T", new string[] { "2", null, null, null, "2", null, "9", null, null, null, null, null });
            goTO.Add("F", new string[] { "3", null, null, null, "3", null, "3", "10", null, null, null, null });

            grammarRule = new Dictionary<int, string[]>();
            grammarRule.Add(1, new string[] { "E", "E + T" });
            grammarRule.Add(2, new string[] { "E", "T" });
            grammarRule.Add(3, new string[] { "T", "T * F" });
            grammarRule.Add(4, new string[] { "T", "F" });
            grammarRule.Add(5, new string[] { "F", "( E )" });
            grammarRule.Add(6, new string[] { "F", "id" });

        }

        public string GetStep(int state, string symbol) 
        {
            string el =  actionState[symbol][state];
            if (el == null)
                throw new Exception("Syntax Error");
            return el;
        }

        public string[] GetRule(int ruleNum)
        {
            return grammarRule[ruleNum];

        }

        public string GoTo(int state, string go2)
        {
            string el =  goTO[go2][state];
            if (el == null)
                throw new Exception("Syntax Error");
            return el; 
        }

    }

    class SRP
    {
        public static string Save(Stack<string> stck)
        {
            string sout = "";
            var rev = stck.ToArray();
            Array.Reverse(rev);
            foreach (string str in rev)
            {
                sout += str;
            }
            return sout;
        }


        public static void Parse (Queue<List<string>> tokenedLexemes,  Table T)
        {
            List<string> savedStacks = new List<string>();

            int state = 0;
            Stack<string> stack = new Stack<string>();
            stack.Push("0");

            Queue<string> buffer = new Queue<string>();
            foreach (var tok_lex in tokenedLexemes) { buffer.Enqueue(tok_lex[1]); }

            string currentAction = buffer.Dequeue(); 
            string currentStep = T.GetStep(state, currentAction);

            Console.WriteLine(currentStep.ToUpper());
            while (currentStep != "accept")
            {
                savedStacks.Add(Save(stack)); 

                // if shift
                if (currentStep[0].ToString() == "S") 
                {
                    stack.Push(currentAction);
                    stack.Push(currentStep.TrimStart(currentStep[0]));
                    state = int.Parse(currentStep.TrimStart(currentStep[0]));

                    currentAction = buffer.Dequeue();
                    currentStep = T.GetStep(state, currentAction);

                }
                // if reduce 
                else if (currentStep[0].ToString() == "R")
                {

                    var rule = T.GetRule(int.Parse(currentStep[1].ToString()));
                    Stack<string> grammer = new Stack<string>();
                    foreach (string str in rule[1].Split(' ')) { grammer.Push(str); }
                     
                    string replace = rule[0];

                    string stack_temp = stack.Pop();
                    string ruleStr = grammer.Pop();
                    while (stack.Count>0)
                    {
                        if (stack_temp == ruleStr)
                        {
                            stack_temp = stack.Peek();
                            if (grammer.Count > 0)
                                ruleStr = grammer.Pop();
                            else break; 
                        }
                        else
                        {
                            stack_temp = stack.Pop();
                        }

                    }
                    state = int.Parse(T.GoTo( int.Parse(stack_temp), replace));
                    stack.Push(replace);
                    stack.Push(state.ToString());

                    currentStep = T.GetStep(state, currentAction);
                }
                Console.WriteLine(currentStep.ToUpper());

            }
            savedStacks.Add(Save(stack));
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Stack");
            Console.WriteLine("-----------------------------------");

            foreach (var el in savedStacks)
            {
                Console.WriteLine(el);
            }

        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Enter an expression");
            string line = Console.ReadLine(); 
            line = Regex.Replace(line, " {2,}", " ");


            Console.WriteLine("-----------------------------------");

            Console.WriteLine("Calling Lexer:");
            Console.WriteLine("-----------------------------------");

            Queue<List<string>> tokenedLexemes = new Queue<List<string>>();
            tokenedLexemes = Lex.SRP_Tokenize(line);

            foreach (var sublist in tokenedLexemes)
            {
                if (sublist[0]!="$")
                    Console.WriteLine(sublist[0] + ": " + sublist[1]);
            }
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Parsing Steps");
            Console.WriteLine("-----------------------------------");
            var T = new Table();

            Parse(tokenedLexemes, T);

        }
    }
}
