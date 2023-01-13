using System.Text;
using JetBrains.Annotations;

namespace Tauron.Localization;

[PublicAPI]
public class EscapeHelper
{
    public static string Ecode(string input) => Coder.Encode(input);

    public static string Decode(string? input)
    {
        if(string.IsNullOrEmpty(input)) return string.Empty;

        var data = input.AsSpan();

        return Coder.Decode(ref data);
    }

    private static class Coder
    {
        private const char EscapeStart = '@';

        private static readonly Dictionary<string, char> Parts
            = new(StringComparer.Ordinal)
              {
                  { "001", '\r' },
                  { "002", '\t' },
                  { "003", '\n' },
                  { "004", ':' },
              };

        private static string? GetPartforChar(char @char)
            => (from part in Parts
                where part.Value == @char
                select part.Key).FirstOrDefault();

        private static char? GetPartforSequence(string @char)
        {
            if(Parts.TryGetValue(@char, out char escape))
                return escape;

            return null;
        }

        private static void TryAddPart(StringBuilder builder, string seq)
        {
            char? part = GetPartforSequence(seq);

            if(part is null) builder.Append(EscapeStart, 2).Append(seq);
            else builder.Append(part.Value);
        }

        internal static string Encode(IEnumerable<char> toEncode)
        {
            var builder = new StringBuilder();
            foreach (char @char in toEncode)
            {
                string? part = GetPartforChar(@char);
                if(part is null) builder.Append(@char);
                else builder.Append(EscapeStart, 2).Append(part);
            }

            return builder.ToString();
        }

        #pragma warning disable EPS06
        internal static string Decode(ref ReadOnlySpan<char> data)
        {
            var builder = new StringBuilder(data.Length);

            var pos = 0;
            var currentBatch = data;

            while (pos != data.Length)
            {
                int index = currentBatch.IndexOf(EscapeStart);
                if(index == -1)
                {
                    builder.Append(currentBatch);

                    break;
                }

                if(currentBatch.Length > 1 && currentBatch[index + 1] == EscapeStart)
                {
                    var seq = currentBatch.Slice(index + 1, 3).ToString();
                    TryAddPart(builder, seq);

                    pos += index + 5;
                    currentBatch = currentBatch[(index + 5)..];

                    continue;
                }

                pos += index;
                currentBatch = currentBatch[index..];
            }

            return builder.ToString();
        }
        #pragma warning restore EPS06

        /*internal static string DecodeOld(IEnumerable<char>? toDecode)
        {
            if (toDecode is null)
                return string.Empty;

            var builder = new StringBuilder();

            var flag = false;
            var flag2 = false;
            var pos = 0;
            var sequence = string.Empty;
            var temp = string.Empty;

            foreach (var @char in toDecode)
            {
                if (flag2)
                {
                    sequence += @char;
                    pos++;

                    if (pos != 3) continue;

                    var part = GetPartforSequence(sequence);
                    if (part == null) builder.Append(temp).Append(sequence);
                    else builder.Append(part);

                    flag = false;
                    flag2 = false;
                    pos = 0;
                    sequence = string.Empty;
                    temp = string.Empty;
                }
                else if (flag)
                {
                    flag2 = @char == EscapeStart;
                    if (!flag2)
                    {
                        builder.Append(EscapeStart).Append(@char);
                        flag = false;
                    }
                    else
                    {
                        temp += @char;
                    }
                }
                else
                {
                    flag = @char == EscapeStart;

                    if (!flag)
                        builder.Append(@char);
                    else temp += @char;
                }
            }

            return builder.ToString();
        }*/
    }
}