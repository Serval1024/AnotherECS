using AnotherECS.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static AnotherECS.Generator.TemplateParser;

namespace AnotherECS.Generator
{
    public static class TemplateParser
    {
        private static readonly Regex _commentStatement = new("\\*\\*\\*.*\\*\\*\\*");
        private static readonly Regex _tabStatement = new("\\-\\-\\-");

        public static string Transform(string template, Variables variables)
        {
            var statements = ParseByTags(RemoveComments(template)).Select(ConvertToStatement).ToList();
            return ConvertToExpressions(statements).ToText(variables);
        }

        public static MetaExpression GetMetaHeader(string template)
        {
            string head = ParseByTags(template, 1).FirstOrDefault();
            if (head != null)
            {
                if (ConvertToStatement(head) is MetaStatement metaStatement)
                {
                    return metaStatement.CutToExpression();
                }
            }
            return null;
        }

        private static GroupExpression ConvertToExpressions(List<Statement> statements)
        {
            var result = new List<Expression>();
            for(int i = 0; i < statements.Count; ++i)
            {
                result.Add(statements[i].CutToExpression(statements));
            }
            return new GroupExpression(result);
        }

        private static Statement ConvertToStatement(string tag)
        {
            if (IfStatement.Is(tag))
            {
                return new IfStatement(tag);
            }
            if (EndStatement.Is(tag))
            {
                return new EndStatement(tag);
            }
            if (ElseStatement.Is(tag))
            {
                return new ElseStatement(tag);
            }
            if (ArrayStatement.Is(tag))
            {
                return new ArrayStatement(tag);
            }
            if (MetaStatement.Is(tag))
            {
                return new MetaStatement(tag);
            }
            if (VariableStatement.Is(tag))
            {
                return new VariableStatement(tag);
            }

            return new TextStatement(tag);
        }

        private static string RemoveComments(string template)
            => _tabStatement.Replace(_commentStatement.Replace(template, string.Empty), string.Empty);

        private static List<string> ParseByTags(string template, int limit = int.MaxValue)
        {
            var result = new List<string>();
            ParseByTags(template, result, limit);
            return result;
        }

        private static void ParseByTags(string template, List<string> tags, int limit)
        {
            var start = FindTagStart(template, 0);
            if (start != -1)
            {
                var end = FindTagEnd(template, start);
                if (end != -1)
                {
                    var beginStr = template[..start];
                    if (beginStr != string.Empty)
                    {
                        tags.Add(beginStr);
                        if (tags.Count == limit)
                        {
                            return;
                        }
                    }

                    tags.Add(template[start..(end + 1)]);
                    if (tags.Count == limit)
                    {
                        return;
                    }
                    ParseByTags(template[(end + 1)..], tags, limit);
                }
                else
                {
                    throw new Exception($"Template is broken. {template} : Position {start}.");
                }
            }
            else
            {
                tags.Add(template);
                if (tags.Count == limit)
                {
                    return;
                }
            }
        }

        private static int FindTagStart(string template, int start)
        {
            for (int i = start; i < template.Length - 1; ++i)
            {
                if (template[i] == '<' && template[i + 1] == '#')
                {
                    return i;
                }
            }
            return -1;
        }

        private static int FindTagEnd(string template, int start)
        {
            for (int i = start; i < template.Length - 1; ++i)
            {
                if (template[i] == '#' && template[i + 1] == '>')
                {
                    return i + 1;
                }
            }
            return -1;
        }

        private static string TrimStartNewLine(string str)
            => str.TrimStart('\r', '\n');

        public abstract class Condition
        {
            public const string TAG_REGEX_AND = "&&";
            public const string TAG_REGEX_OR = "\\|\\|";
            public const string TAG_REGEX_NOT = "\\!";

            public const string TAG_AND = "&&";
            public const string TAG_OR = "||";
            public const string TAG_NOT = "!";

            public static Condition Create(string text)
            {
                var (index, op) = FindFirst();

                if (op == TAG_AND)
                {
                    var strings = Split(text, index, op.Length);
                    return new AndCondition(Create(strings[0]), Create(strings[1]));
                }
                else if (op == TAG_OR)
                {
                    var strings = Split(text, index, op.Length);
                    return new OrCondition(Create(strings[0]), Create(strings[1]));
                }
                else if (op == TAG_NOT)
                {
                    var strings = text[(index + op.Length)..];
                    return new NotCondition(Create(strings));
                }

                return new VariableCondition(text.Trim());



                (int index, string op) FindFirst()
                {
                    var patterns = new[] { $"({TAG_REGEX_AND}|{TAG_REGEX_OR})", $"({TAG_REGEX_NOT})"};

                    foreach(var pattern in patterns)
                    {
                        var match = Regex.Match(text, pattern);
                        if (match.Success)
                        {
                            return (match.Index, match.Value);
                        }
                    }
                    return (-1, string.Empty);
                }
            }

            public abstract bool IsTrue(Variables variables);

            private static string[] Split(string text, int centerIndex, int len)
            {
                var result = new string[2];
                result[0] = text[..centerIndex];
                result[1] = text[(centerIndex + len)..];
                return result;
            }
        }

        public class VariableCondition : Condition
        {
            public string Value { get; private set; }

            public VariableCondition(string value)
            { 
                this.Value = value;
            }

            public override bool IsTrue(Variables variables)
                => variables.TryGetValue(Value, out Func<object> function) && (bool)function();
        }

        public class AndCondition : Condition
        {
            public Condition Left { get; private set; }
            public Condition Right { get; private set; }

            public AndCondition(Condition left, Condition right)
            {
                this.Left = left;
                this.Right = right;
            }

            public override bool IsTrue(Variables variables)
                => Left.IsTrue(variables) && Right.IsTrue(variables);
        }

        public class OrCondition : Condition
        {
            public Condition Left { get; private set; }
            public Condition Light { get; private set; }

            public OrCondition(Condition left, Condition right)
            {
                this.Left = left;
                this.Light = right;
            }

            public override bool IsTrue(Variables variables)
                => Left.IsTrue(variables) || Light.IsTrue(variables);
        }

        public class NotCondition : Condition
        {
            public Condition Condition { get; private set; }

            public NotCondition(Condition left)
            {
                this.Condition = left;
            }

            public override bool IsTrue(Variables variables)
                => !Condition.IsTrue(variables);
        }

        public abstract class Expression 
        {
            public abstract string ToText(Variables variables);
        }

        public class GroupExpression : Expression
        {
            public List<Expression> Childs { get; private set; }

            public GroupExpression(List<Expression> childs)
            {
                this.Childs = childs.Where(p => p != null).ToList();
            }

            public override string ToText(Variables variables)
            {
                var result = new StringBuilder();
                foreach(var child in Childs)
                {
                    result.Append(child.ToText(variables));
                }
                return result.ToString();
            }
        }

        public class VariableExpression : Expression
        {
            public string Head { get; private set; }

            public VariableExpression(string head)
            {
                this.Head = head;
            }

            public override string ToText(Variables variables)    
                => Transform(variables[Head]().ToString(), variables);
        }

        public class IntVariableExpression : VariableExpression, IExpressionToInt
        {
            public IntVariableExpression(string head)
                : base(head) { }

            public int ToInt(Variables variables)
                => int.Parse(ToText(variables));
        }

        public class IntExpression : Expression, IExpressionToInt
        {
            public int Value { get; private set; }

            public IntExpression(string head)
            {
                Value = int.TryParse(head, out int number)
                    ? number
                    : 0;
            }

            public int ToInt(Variables variables)
                => Value;

            public override string ToText(Variables variables)
                => Value.ToString();
        }

        public class TextExpression : Expression
        {
            public string Text { get; private set; }

            public TextExpression(string text)
            {
                this.Text = text;
            }

            public override string ToText(Variables variables)
                => Text;
        }

        public class ArrayExpression : Expression
        {
            public RangeExpression Range { get; private set; }
            public Expression Child { get; private set; }

            public ArrayExpression(string head, Expression child)
            {
                this.Range = new RangeExpression(head);
                this.Child = child;
            }

            public override string ToText(Variables variables)
            {
                var result = new StringBuilder();
                var r = Range.GetRange(variables);
                var deep = variables.IncDeep();
                variables.SetLength(deep, r.End.Value);

                for (var index = r.Start.Value; index < r.End.Value; ++index)
                {
                    variables.SetIndex(deep, index);
                    result.Append(TrimStartNewLine(Child.ToText(variables)));
                }

                variables.DecDeep();

                return result.ToString();
            }
        }

        public class RangeExpression : Expression
        {
            public string Head { get; private set; }
            public IExpressionToInt Min { get; private set; }
            public IExpressionToInt Max { get; private set; }

            public RangeExpression(string head)
            {
                var strings = head.Replace('[', ' ').Replace(']', ' ').Split("..");
                Min = TextToInt(strings[0].Trim());
                Max = TextToInt(strings[1].Trim());
            }

            public Range GetRange(Variables variables)
                => new(Min.ToInt(variables), Max.ToInt(variables));

            public override string ToText(Variables variables)
                => string.Empty;

            private IExpressionToInt TextToInt(string text)
            {
                if (IntStatement.Is(text))
                {
                    return IntStatement.ToExpression(text);
                }
                else
                {
                    return new IntVariableExpression(text);
                }
                throw new ArgumentException("Unknow range format.");
            }
        }

        public class IfExpression : Expression
        {
            public Condition Condition { get; private set; }
            public Expression Left { get; private set; }
            public Expression Right { get; private set; }

            public IfExpression(string head, Expression left, Expression right)
            {
                this.Condition = Condition.Create(head);
                this.Left = left;
                this.Right = right;
            }

            public override string ToText(Variables variables)
            {
                if (Condition.IsTrue(variables))
                {
                    return Left.ToText(variables);
                }
                else if(Right != null)
                {
                    return Right.ToText(variables);
                }

                return string.Empty;
            }
        }

        public class MetaExpression : Expression
        {
            public string Head { get; private set; }
            public string FileName { get; private set; }
            public Type GeneratorType { get; private set; }
            public int N { get; private set; }

            public MetaExpression(string head)
            {
                this.Head = head;

                var filenameArgument = Regex.Match(head, @"(?<=FILENAME)\s*=\s*[\w|.]*[^#\s]");
                if (filenameArgument.Success)
                {
                    FileName = filenameArgument.Value.Trim(' ', '=');
                }
                var generatorArgument = Regex.Match(head, @"(?<=GENERATOR)\s*=\s*[\w|.]*[^#\s]");
                if (generatorArgument.Success)
                {
                    GeneratorType = TypeUtils.FindType(generatorArgument.Value.Trim(' ', '='));
                }

                var nArgument = Regex.Match(head, @"(?<=N)\s*=\s*[\w|.]*[^#\s]");
                if (nArgument.Success && int.TryParse(nArgument.Value.Trim(' ', '='), out int number))
                {
                    N = number;
                }
            }

            public static bool Is(string text)
               => MetaStatement.Is(text);

            public override string ToText(Variables variables)
                => "//" + Head;
        }

        public class Variables : Dictionary<string, Func<object>> 
        {
            private readonly string _INDEX_TAG = "INDEX";
            private readonly string _LEN_TAG = "LEN";

            private readonly List<int> arrayIndex = new();
            private readonly List<int> arrayLength = new();

            internal int Deep
                => arrayIndex.Count - 1;

            public int GetIndex()
                => arrayIndex.Count > 0 ? arrayIndex[^1] : -1;

            public int GetIndex(int deep)
                => deep >= 0 && deep < arrayIndex.Count ? arrayIndex[deep] : -1;

            public int GetLength()
                => arrayLength.Count > 0 ? arrayLength[^1] : -1;

            public int GetLength(int deep)
                => deep >= 0 && deep < arrayIndex.Count ? arrayLength[deep] : -1;

            internal int IncDeep()
            {
                arrayIndex.Add(0);
                arrayLength.Add(0);
                return Deep;
            }

            internal int DecDeep()
            {
                arrayIndex.RemoveAt(arrayIndex.Count - 1);
                arrayLength.RemoveAt(arrayLength.Count - 1);
                return Deep;
            }

            public void SetIndex(int deep, int index)
            {
                arrayIndex[deep] = index;

                var indexStr = GetIndex().ToString();
                var indexDeepStr = GetIndex(Deep).ToString();
                this[_INDEX_TAG] = () => indexStr;
                this[$"{_INDEX_TAG}{Deep}"] = () => indexDeepStr;
            }

            public void SetLength(int deep, int length)
            {
                arrayLength[deep] = length;

                var lenStr = GetLength().ToString();
                var lenDeepStr = GetLength(Deep).ToString();
                this[_LEN_TAG] = () => lenStr;
                this[$"{_LEN_TAG}{Deep}"] = () => lenDeepStr;
            }
        };

        public interface IExpressionToInt
        {
            int ToInt(Variables variables);
        }

        private class IfStatement : Statement
        {
            public const string TAG = "<#IF";

            public override int Nesting => 1;

            public IfStatement(string text)
            {
                this.text = text;
            }

            public static bool Is(string text)
                => text.StartsWith(TAG);

            public override Expression CutToExpression(List<Statement> statements)
            {
                var block = EndStatement.Find(statements, this);
                var nested = EndStatement.CutBlock(statements, block);

                var elseIndex = ElseStatement.Find(nested);
                if (elseIndex != -1)
                {
                    return
                        new IfExpression(
                            TrimHead(text),
                            TemplateParser.ConvertToExpressions(nested.GetRange(0, elseIndex)),
                            TemplateParser.ConvertToExpressions(nested.GetRange(elseIndex + 1, nested.Count - elseIndex - 1))
                            );

                }
                else
                {
                    return new IfExpression(TrimHead(text), TemplateParser.ConvertToExpressions(nested), null);
                }
            }

            private string TrimHead(string text)
                => text[TAG.Length..^TAG_CLOSE.Length].Trim();
        }

        private class EndStatement : Statement
        {
            public const string TAG = "<#END#>";

            public override int Nesting => -1;

            public EndStatement(string text)
            {
                this.text = text;
            }

            public static bool Is(string text)
                => text.StartsWith(TAG);

            public static Range Find(List<Statement> statements, Statement startStatement)
            {
                var start = statements.FindIndex(p => p == startStatement);
                int nesting = 1;
                for (int i = start + 1; i < statements.Count; ++i)
                {
                    nesting += statements[i].Nesting;
                    if (nesting == 0)
                    {
                        return new Range(start + 1, i);
                    }
                }
                throw new Exception($"End tag not found.");
            }

            public static List<Statement> CutBlock(List<Statement> statements, Range slice)
            {
                var nested = statements.GetRange(slice.Start.Value, slice.End.Value - slice.Start.Value);
                statements.RemoveRange(slice.Start.Value, slice.End.Value - slice.Start.Value);
                return nested;
            }

            public override Expression CutToExpression(List<Statement> statements)
                => null;
        }

        private class ElseStatement : Statement
        {
            public const string TAG = "<#ELSE#>";

            public override int Nesting => 0;

            public ElseStatement(string text)
            { 
                this.text = text;
            }

            public static bool Is(string text)
                => text.StartsWith(TAG);

            public static int Find(List<Statement> nested)
            {
                int nesting = 0;
                for (int j = 0; j < nested.Count; ++j)
                {
                    nesting += nested[j].Nesting;
                    if (nesting == 0 && nested[j] is ElseStatement)
                    {
                        return j;
                    }
                }
                return -1;
            }

            public override Expression CutToExpression(List<Statement> statements)
                => null;
        }

        private class ArrayStatement : Statement
        {
            public const string TAG = "<#ARRAY";

            public override int Nesting => 1;

            public ArrayStatement(string text)
            {
                this.text = text;
            }

            public static bool Is(string text)
                => text.StartsWith(TAG);

            public override Expression CutToExpression(List<Statement> statements)
            {
                var block = EndStatement.Find(statements, this);
                var nested = EndStatement.CutBlock(statements, block);

                return new ArrayExpression(TrimHead(text), TemplateParser.ConvertToExpressions(nested));
            }

            private string TrimHead(string text)
                => text[TAG.Length..^TAG_CLOSE.Length].Trim();
        }

        private class IntStatement : Statement
        {
            public override int Nesting => 0;

            public static bool Is(string text)
               => int.TryParse(text, out _);

            public static IntExpression ToExpression(string text)
                => new(text);

            public override Expression CutToExpression(List<Statement> statements)
            {
                throw new NotImplementedException();
            }
        }

        private class VariableStatement : Statement
        {
            public const string TAG = "<#";

            public override int Nesting => 0;

            public VariableStatement(string text)
            {
                this.text = text;
            }

            public static bool Is(string text)
                => text.StartsWith(TAG);

            public override Expression CutToExpression(List<Statement> statements)
                => new VariableExpression(TrimHead(text));

            private string TrimHead(string text)
                => text[TAG.Length..^TAG_CLOSE.Length].Trim();
        }

        private class MetaStatement : Statement
        {
            public const string TAG = "<#META";

            public override int Nesting => 0;

            public MetaStatement(string text)
            { 
                this.text = text;
            }

            public static bool Is(string text)
                => text.StartsWith(TAG);

            public override Expression CutToExpression(List<Statement> statements)
                => CutToExpression();

            public MetaExpression CutToExpression()
                => new(TrimHead(text));

            private string TrimHead(string text)
                => text[TAG.Length..^TAG_CLOSE.Length].Trim();
        }

        private class TextStatement : Statement
        {
            public override int Nesting => 0;

            public TextStatement(string text)
            {
                this.text = text;
            }

            public static bool Is(string text)
                => true;

            public override Expression CutToExpression(List<Statement> statements)
                => new TextExpression(text);
        }

        private abstract class Statement
        {
            public const string TAG_CLOSE = "#>";
            public const char EXSTRA_TAG_MARKER = '#';

            public string text;

            public abstract int Nesting { get; }

            public abstract Expression CutToExpression(List<Statement> statements);
        }
    }

    internal static class VariablesExtension
    {
        public static ushort GetIndexAsId(this Variables variables, int deep = 0)
            => (ushort)(variables.GetIndex(deep) + 1);
    }
}    

