using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;




class Lex
{
    private static char[] operators = { '(', ')', '/', '+', '-', '*' };

    private static bool IsOperator(char c)
    {
        return operators.Contains(c);
    }

    private static bool IsValidVarName(string str)
    {
        return Regex.IsMatch(str, @"^[a-zA-Z0-9_]+$");

    }

    public static Queue<List<string>> Tokenize(string str)
    {

        if (str.Length == 0 || (str[0] == ' ' && str.Length == 1))
            throw new Exception("Empty string provided");

        string prevLexeme = "";
        int numOpenParens = 0;
        Queue<List<string>> tokenedLexemes = new Queue<List<string>>();

        int currChar;
        for (currChar = 0; currChar < str.Length; currChar++)
        {
            // if current char is an operator or space
            if (IsOperator(str[currChar]) || str[currChar] == ' ')
            {
                // parentheses logic 
                if (str[currChar] == '(' || str[currChar] == ')')
                {
                    if (str[currChar] == '(')
                    {
                        if ((prevLexeme.Length > 0 && !IsOperator(prevLexeme[0]) && prevLexeme[0] != ' ') || prevLexeme == ")")
                            throw new Exception("Left parentheses must be preceded by an operator other than ')'");
                        numOpenParens++;

                    }
                    else // current char is ')'
                    {
                        if (IsOperator(prevLexeme[0]) && prevLexeme[0] != ')')
                            throw new Exception("Right parentheses must not be preceded by an operator other than itself" + prevLexeme);
                        numOpenParens--;
                    }

                    if (numOpenParens < 0)
                        throw new Exception("Parentheses must be opened before being closed");
                }
                else if (prevLexeme.Length == 1 && str[currChar] != ' ' && IsOperator(prevLexeme[0]) && prevLexeme[0] != ')')
                    throw new Exception("Operators musn't be preceded by another operator except ')'");

                // add previous lexeme to queue
                if (prevLexeme.Length > 0 && prevLexeme[0] != ' ')
                {
                    if (prevLexeme.All(char.IsDigit))
                        tokenedLexemes.Enqueue(new List<string> { prevLexeme, "INT_LIT" });
                    else if (IsValidVarName(prevLexeme))
                        tokenedLexemes.Enqueue(new List<string> { prevLexeme, "IDENT" });
                    else if (!IsOperator(prevLexeme[0]))
                        throw new Exception("Invalid Character : " + prevLexeme);
                }

                // add current operator to queue 
                switch (str[currChar])
                {
                    case '+':
                        tokenedLexemes.Enqueue(new List<string> { "+", "ADD_OP" });
                        break;
                    case '-':
                        tokenedLexemes.Enqueue(new List<string> { "-", "SUB_OP" });
                        break;
                    case '/':
                        tokenedLexemes.Enqueue(new List<string> { "/", "DIV_OP" });
                        break;
                    case '*':
                        tokenedLexemes.Enqueue(new List<string> { "*", "MUL_OP" });
                        break;
                    case '(':
                        tokenedLexemes.Enqueue(new List<string> { "(", "LEFT_PAREN" });
                        break;
                    case ')':
                        tokenedLexemes.Enqueue(new List<string> { ")", "RIGHT_PAREN" });
                        break;
                    case ' ':
                        break;
                    default:
                        throw new Exception("Serious Error");
                }

                // reset the previous lexeme to current operator  
                prevLexeme = str[currChar].ToString();
            }
            else if (prevLexeme.Length > 0 && System.Char.IsDigit(prevLexeme[0]) && System.Char.IsLetter(str[currChar]))
                throw new Exception("Variable names must not begin with a number");
            else // a valid non-operator 
            {
                if (prevLexeme.Length == 1 && (IsOperator(prevLexeme[0]) || prevLexeme[0] == ' '))
                    prevLexeme = str[currChar].ToString(); // reset when after operators
                else
                    prevLexeme += str[currChar].ToString(); // append when still in lexeme 
            }

        }

        if (numOpenParens > 0)
            throw new System.Exception("All parentheses must be closed");

        if (IsOperator(str[currChar - 1]))
        {
            if (str[currChar - 1] != ')')
                throw new System.Exception("Operators must have right operands except for ')'");
        }

        else // add last lexeme 
        {
            if (prevLexeme.All(char.IsDigit))
                tokenedLexemes.Enqueue(new List<string> { prevLexeme, "INT_LIT" });
            else if (prevLexeme[0] != ' ')
                tokenedLexemes.Enqueue(new List<string> { prevLexeme, "IDENT" });
        }
        return tokenedLexemes;
    }

}



public class Program
{
    private string nextToken;
    private Queue<List<string>> tokenedLexemes;

    public void setProgram(Queue<List<string>> tokenedLexemes)
    {
        this.tokenedLexemes = tokenedLexemes;
        string lexeme;
        List<string> tmp = this.tokenedLexemes.Dequeue();
        lexeme = tmp[0];
        nextToken = tmp[1];

        Console.WriteLine("NextToken: " + nextToken + "");
    }


    void lex()
    {
        if (this.tokenedLexemes.Count > 0)
        {
            string lexeme;
            List<string> tmp = this.tokenedLexemes.Dequeue();
            lexeme = tmp[0];
            nextToken = tmp[1];
            Console.WriteLine("NextToken " + nextToken);
        }
        else
            Console.WriteLine("NextToken: EOF");
    }

    void error(string e = "Error")
    {
        throw new Exception(e);
    }

    void expr()
    {
        Console.WriteLine("Enter < expr >");
        term();
        while (nextToken == "ADD_OP" || nextToken == "SUB_OP")
        {
            lex();
            term();
        }
        Console.WriteLine("Exit < expr >");
    }

    void term()
    {
        Console.WriteLine("Enter < term >");
        factor();
        while (nextToken == "MUL_OP" || nextToken == "DIV_OP")
        {
            lex();
            factor();
        }
        Console.WriteLine("Exit < term >");
    }

    void factor()
    {
        Console.WriteLine("Enter < factor >");
        if (nextToken == "IDENT" || nextToken == "INT_LIT")
            lex();
        else
        {
            if (nextToken == "LEFT_PAREN")
            {
                lex();
                expr();
                if (nextToken == "RIGHT_PAREN")
                    lex();
                else
                    error();
            }
            else
                error();
        } //End of else
        Console.WriteLine("Exit < factor >");
    }


    public static void Main(string[] args)
    {

        Console.WriteLine("Enter an expression");
        string line = Console.ReadLine(); //.Replace(" ", String.Empty);
        line = Regex.Replace(line, " {2,}", " ");

        //Console.WriteLine(line);
        Console.WriteLine("-----------------------------------");

        Console.WriteLine("Calling Lexer:");
        Queue<List<string>> tokenedLexemes = new Queue<List<string>>();
        tokenedLexemes = Lex.Tokenize(line);

        foreach (var sublist in tokenedLexemes)
        {
            Console.WriteLine(sublist[0] + ": " + sublist[1]);
        }

        Console.WriteLine("-----------------------------------");
        Console.WriteLine("Calling the Recursive Descent Parser:");

        Program prog = new Program();
        prog.setProgram(tokenedLexemes);
        prog.expr();
    }
}