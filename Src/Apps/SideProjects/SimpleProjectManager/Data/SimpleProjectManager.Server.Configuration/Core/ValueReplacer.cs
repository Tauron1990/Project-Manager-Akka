using System.Collections.Immutable;
using System.Text;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace SimpleProjectManager.Server.Configuration.Core;

public static class ValueReplacer
{
    private static readonly Tokenizer<ValueToken> Tokenizer =
        new TokenizerBuilder<ValueToken>()
           .Match(Character.EqualTo('{'), ValueToken.OpenBrace)
           .Match(Character.EqualTo('}'), ValueToken.CloseBrace)
           .Match(Span.WithoutAny(c => c is '{' or '}'), ValueToken.Text)
           .Build();

    private static readonly TokenListParser<ValueToken, ReferenceNode> RefNodeParser =
        from open in Token.EqualTo(ValueToken.OpenBrace)
        from id in Token.EqualTo(ValueToken.Text)
        from close in Token.EqualTo(ValueToken.CloseBrace)
        select new ReferenceNode(id.Span.ToStringValue());

    private static readonly TokenListParser<ValueToken, TextNode> TextNodeParser =
        from text in Token.EqualTo(ValueToken.Text)
        select new TextNode(text.Span.ToStringValue());

    private static readonly TokenListParser<ValueToken, PropertyValue> PropertyParser =
        from values in Parse.OneOf(
            RefNodeParser.Cast<ValueToken, ReferenceNode, NodeBase>().Try(),
            TextNodeParser.Cast<ValueToken, TextNode, NodeBase>()
        ).Many()
        select new PropertyValue(values.ToImmutableList());

    private static TokenListParserResult<ValueToken, PropertyValue> ParseValue(string input)
        => PropertyParser(Tokenizer.Tokenize(input));

    private static ImmutableDictionary<string, string> ParseValue(ref ImmutableList<string> processed, ImmutableDictionary<string, string> dic, string key)
    {
        var value = ParseValue(dic[key]);

        if(!value.HasValue)
            throw new InvalidOperationException(value.ErrorMessage);

        var builder = new StringBuilder();
        foreach (NodeBase node in value.Value.Nodes)
        {
            switch (node)
            {
                case TextNode txt:
                    builder.Append(txt.Text);

                    break;
                case ReferenceNode refNode:
                    if(!processed.Contains(refNode.Name, StringComparer.Ordinal))
                    {
                        dic = ParseValue(ref processed, dic, refNode.Name);
                        processed = processed.Add(refNode.Name);
                    }

                    builder.Append(dic[refNode.Name]);

                    break;
            }

            dic = dic.SetItem(key, builder.ToString());
        }

        return dic;
    }

    public static ImmutableDictionary<string, string> ExpandPropertys(ImmutableDictionary<string, string> input)
    {
        var processd = ImmutableList<string>.Empty;
        foreach (string key in input.Keys)
        {
            if(processd.Contains(key, StringComparer.Ordinal))
                continue;

            input = ParseValue(ref processd, input, key);

            processd = processd.Add(key);
        }

        return input;
    }
}