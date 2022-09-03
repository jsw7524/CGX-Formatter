using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Regex regex = new Regex(@"(?<assignment>=)|(?<separator>;)|(?<null>null)|(?<boolean>false|true)|(?<lp>\()|(?<rp>\))|(?<number>-?\d+)|(?<str>'[^']*')");
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
            if (_position >= _tokens.Count )
            {
                return false;
            }
            if (_tokens[_position].TypeName == s)
            {
                _currentTokenContent = _tokens[_position].Content;
                _position++;
                result = true;
            }
            return result;
        }

    }


    public interface IElementCGX
    {
        public string ToString(int offset);
    }
    public class Starter : IElementCGX
    {
        IElementCGX _element;
        public Starter(IElementCGX element)
        {
            _element = element;
        }
        public string ToString(int offset)
        {
            return _element.ToString(0).Substring(1);
        }
    }

    public class Block : IElementCGX
    {
        IEnumerable<IElementCGX> _elements;
        public Block(IEnumerable<IElementCGX> elements)
        {
            _elements = elements;
        }
        public string ToString(int offset)
        {
            List<IElementCGX> elmts = _elements?.ToList() ?? new List<IElementCGX>();
            StringBuilder paddingSpace = new StringBuilder(); ;
            for (int i = 0; i < offset; i++)
            {
                paddingSpace.Append(' ');
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("\n" + paddingSpace + "(");
            for (int i = 0; i < elmts.Count; i++)
            {
                if (i < elmts.Count - 1)
                {
                    sb.Append(elmts[i].ToString(offset + 4) + ";");
                }
                else
                {
                    sb.Append(elmts[i].ToString(offset + 4));
                }
            }
            sb.Append("\n" + paddingSpace + ")");
            return sb.ToString();
        }
    }
    public class PrimitiveType : IElementCGX
    {
        string _content;

        public PrimitiveType(string content)
        {
            _content = content;
        }
        public string ToString(int offset)
        {
            StringBuilder paddingSpace = new StringBuilder(); ;
            for (int i = 0; i < offset; i++)
            {
                paddingSpace.Append(' ');
            }
            return "\n" + paddingSpace + _content;
        }
    }

    public class KeyValuePrimitiveType : IElementCGX
    {
        string _content;
        public KeyValuePrimitiveType(string content)
        {
            _content = content.Replace("\n", "");
        }
        public string ToString(int offset)
        {
            return _content;
        }
    }

    public class KeyValue : IElementCGX
    {
        IElementCGX _name;
        IElementCGX _value;

        public KeyValue(IElementCGX name, IElementCGX value)
        {
            _name = name;
            _value = value;
        }
        public string ToString(int offset)
        {
            StringBuilder paddingSpace = new StringBuilder(); ;
            for (int i = 0; i < offset; i++)
            {
                paddingSpace.Append(' ');
            }
            return  _name.ToString(offset) + "=" + _value.ToString(offset);
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

        public IElementCGX Parse(string source)
        {
            try
            {
                Tokenizer tokenizer = new Tokenizer();
                var tokens = tokenizer.GetTokens(source);
                TokenIndexer tokenIndexer = new TokenIndexer(tokens);
                IElementCGX element = null;
                bool result = START(tokenIndexer, out element);
                return element;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool START(TokenIndexer indexer, out IElementCGX element)
        {
            element = null;
            IElementCGX content;
            indexer.SavePosition();
            if (ELEMENT(indexer, out content))
            {
                element = new Starter(content);
                indexer.PopPosition();

                if (indexer.IsComplete)
                {
                    return true;
                }

            }
            indexer.RestorePosition();
            return false;
        }

        public bool ELEMENT(TokenIndexer indexer, out IElementCGX element)
        {
            element = null;
            indexer.SavePosition();
            if (BLOCK(indexer, out element))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (KEY_VALUE(indexer, out element))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (PRIMITIVE_TYPE(indexer, out element))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();


            return false;
        }

        public bool KEY_VALUE(TokenIndexer indexer, out IElementCGX element)
        {
            element = null;
            IElementCGX name;
            IElementCGX block;
            IElementCGX value;
            indexer.SavePosition();
            if (STRING(indexer, out name) && indexer.Check("assignment") && BLOCK(indexer, out block))
            {
                element = new KeyValue(name, block);
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (STRING(indexer, out name) && indexer.Check("assignment") && PRIMITIVE_TYPE(indexer, out value))
            {
                element = new KeyValue(name, new KeyValuePrimitiveType(value.ToString(0)));
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            return false;
        }



        public bool BLOCK(TokenIndexer indexer, out IElementCGX element)
        {
            element = null;
            IEnumerable<IElementCGX> sq;
            indexer.SavePosition();
            if (indexer.Check("lp") && SEQUENCE(indexer, out sq) && indexer.Check("rp"))
            {
                element = new Block(sq);
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();
            return false;
        }

        public bool SEQUENCE(TokenIndexer indexer, out IEnumerable<IElementCGX> sq)
        {
            sq = null;
            IEnumerable<IElementCGX> latter;
            IElementCGX element;
            indexer.SavePosition();
            if (ELEMENT(indexer, out element) && indexer.Check("separator") && SEQUENCE(indexer, out latter))
            {
                List<IElementCGX> tmp = new List<IElementCGX>();
                tmp.Add(element);
                tmp.AddRange(latter);
                sq = tmp;
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (ELEMENT(indexer, out element))
            {
                List<IElementCGX> tmp = new List<IElementCGX>();
                tmp.Add(element);
                sq = tmp;
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();
            return true;
        }

        public bool PRIMITIVE_TYPE(TokenIndexer indexer, out IElementCGX element)
        {
            indexer.SavePosition();
            if (NUMBER(indexer, out element))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (BOOLEAN(indexer, out element))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (STRING(indexer, out element))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            indexer.SavePosition();
            if (NULL(indexer, out element))
            {
                indexer.PopPosition();
                return true;
            }
            indexer.RestorePosition();

            return false;
        }

        public bool NUMBER(TokenIndexer indexer, out IElementCGX element)
        {
            element = null;
            bool result = indexer.Check("number");
            if (true == result)
            {
                element = new PrimitiveType(indexer.GetTokenContent);
                return true;
            }
            return result;
        }
        public bool BOOLEAN(TokenIndexer indexer, out IElementCGX element)
        {
            element = null;
            bool result = indexer.Check("boolean");
            if (true == result)
            {
                element = new PrimitiveType(indexer.GetTokenContent);
                return true;
            }
            return result;
        }

        public bool STRING(TokenIndexer indexer, out IElementCGX element)
        {
            element = null;
            bool result = indexer.Check("str");
            if (true == result)
            {
                element = new PrimitiveType(indexer.GetTokenContent);
                return true;
            }
            return result;
        }

        public bool NULL(TokenIndexer indexer, out IElementCGX element)
        {
            element = null;
            bool result = indexer.Check("null");
            if (true == result)
            {
                element = new PrimitiveType(indexer.GetTokenContent);
                return true;
            }
            return result;
        }

    }


    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
     **/
    class Solution
    {
        static void Main(string[] args)
        {
            int N = int.Parse(Console.ReadLine());
            List<string> input = new List<string>();
            for (int i = 0; i < N; i++)
            {
                input.Add(Console.ReadLine());
            }

            Parser parser = new Parser();
            var result = parser.Parse(String.Join("",input));
            var formation = result.ToString(0);
            Console.WriteLine(formation);
        }
    }
}
