using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CGX_Formatter
{
    public class Token
    {
        public string TypeName { get; set; }
        public string Content { get; set; }
    }

    public class Tokenizer
    {
        public IEnumerable<Token> GetTokens(string source)
        {
            Regex regex = new Regex(@"(?<assignment>=)|(?<separator>;)|(?<null>null)|(?<boolean>false|true)|(?<lp>\()|(?<rp>\))|(?<number>-?\d+)|(?<str>'[\w \(\)=;]+')");
            List<Token> tmp = new List<Token>();
            foreach (Match m in regex.Matches(source))
            {
                for (int i = 1; i < m.Groups.Count; i++)
                {
                    if (m.Groups[i].Length > 0)
                    {
                        tmp.Add(new Token() { TypeName = m.Groups[i].Name, Content = m.Value });
                        break;
                    }
                }
            }
            return tmp;
        }
    }

    public class TokenIndexer
    {
        Stack<int> _stack = new Stack<int>();
        int _position = 0;
        public int Position { get { return _position; } set { _position = value; } }
        List<Token> _tokens;
        string _currentTokenContent = null;
        public TokenIndexer(IEnumerable<Token> tokens)
        {
            _tokens = tokens.ToList();
        }

        public void SavePosition()
        {
            _stack.Push(_position);
        }


        public void PopPosition()
        {
            _stack.Pop();
        }


        public void RestorePosition()
        {
            _position = _stack.Pop();
        }

        public string GetTokenContent
        {
            get { return _currentTokenContent; }
        }

        public bool IsComplete { get { return _position == _tokens.Count(); } }

        public bool Check(string s)
        {
            bool result = false;
            _currentTokenContent = null;
            if (_tokens[_position].TypeName == s)
            {
                _currentTokenContent = _tokens[_position].Content;
                _position++;
                result = true;
            }
            return result;
        }

    }

    public class Parser
    {
        // the implementation of order of gramar expansion is important

        // Start -> ELEMENT
        // ELEMENT -> BLOCK | PRIMITIVE_TYPE | KEY_VALUE
        // BLOCK -> ( SEQUENCE )
        // SEQUENCE -> ELEMENT | ELEMENT;SEQUENCE | EMPTY
        // PRIMITIVE_TYPE -> NUMBER | BOOLEAN | STRING | NULL
        // KEY_VALUE -> STRING = BLOCK | STRING = PRIMITIVE_TYPE

        public bool Parse(string source)
        {
            try
            {
                Tokenizer tokenizer = new Tokenizer();
                var tokens = tokenizer.GetTokens(source);
                TokenIndexer tokenIndexer = new TokenIndexer(tokens);
                bool result = START(tokenIndexer);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool START(TokenIndexer indexer)
        {
            indexer.SavePosition();
            if (ELEMENT(indexer))
            {
                indexer.PopPosition();

                if (indexer.IsComplete)
                {
                    return true;
                }

            }
            indexer.RestorePosition();
            return false;
        }

        public bool ELEMENT(TokenIndexer indexer)
        {
            indexer.SavePosition();
            if (BLOCK(indexer))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (KEY_VALUE(indexer))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (PRIMITIVE_TYPE(indexer))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();


            return false;
        }

        public bool KEY_VALUE(TokenIndexer indexer)
        {
            indexer.SavePosition();
            if (STRING(indexer) && indexer.Check("assignment") && BLOCK(indexer))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (STRING(indexer) && indexer.Check("assignment") && PRIMITIVE_TYPE(indexer))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            return false;
        }



        public bool BLOCK(TokenIndexer indexer)
        {
            indexer.SavePosition();
            if (indexer.Check("lp") && SEQUENCE(indexer) && indexer.Check("rp"))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();
            return false;
        }

        public bool SEQUENCE(TokenIndexer indexer)
        {

            indexer.SavePosition();
            if (ELEMENT(indexer) && indexer.Check("separator") && SEQUENCE(indexer))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (ELEMENT(indexer))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();


            return true;
        }

        public bool PRIMITIVE_TYPE(TokenIndexer indexer)
        {
            indexer.SavePosition();
            if (NUMBER(indexer))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (BOOLEAN(indexer))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (STRING(indexer))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (NULL(indexer))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            return false;
        }

        public bool NUMBER(TokenIndexer indexer)
        {
            bool result = indexer.Check("number");
            if (true == result)
            {
                return true;
            }
            return result;
        }
        public bool BOOLEAN(TokenIndexer indexer)
        {
            bool result = indexer.Check("boolean");
            if (true == result)
            {
                return true;
            }
            return result;
        }

        public bool STRING(TokenIndexer indexer)
        {
            bool result = indexer.Check("str");
            if (true == result)
            {
                return true;
            }
            return result;
        }

        public bool NULL(TokenIndexer indexer)
        {
            bool result = indexer.Check("null");
            if (true == result)
            {
                return true;
            }
            return result;
        }

    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
