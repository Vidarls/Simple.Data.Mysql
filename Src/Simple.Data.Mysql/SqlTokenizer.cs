using System;
using System.Collections.Generic;

namespace Simple.Data.Mysql
{
    internal static class SqlTokenizer
    {
        private enum ShiftOffset
        {
            IndexPlus1,
            EndIndex
        }

        public static IEnumerable<String> Tokenize(String input, Boolean ansiQuotes = false, Boolean useBackslashEscaping = true)
        {
            var startIndex = 0;
            var currentChar = default(Char);
            var quoteEndChar = default(Char);

            var isEscaping = false;
            var inQuotes = false;
            var inLiteral = false;
            var inBlockComments = false;
            var inLineComment = false;

            for (int index = 0; index < input.Length; index++)
            {
                if (isEscaping)
                {
                    isEscaping = false;
                    continue;
                }

                var lastChar = currentChar;
                currentChar = input[index];
                if ((currentChar == '\\') && useBackslashEscaping)
                {
                    isEscaping = true;
                    continue;
                }
                var yieldCurrentChar = false;
                var shiftOffset = ShiftOffset.IndexPlus1;
                var endIndex = index;

                if (inQuotes)
                {
                    if (currentChar == quoteEndChar)
                    {
                        inQuotes = false;
                        quoteEndChar = default(Char);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (currentChar == '\'')
                    {
                        inLiteral = !inLiteral;
                        continue;
                    }

                    if (inLiteral)
                    {
                        continue;
                    }

                    if (inBlockComments)
                    {
                        if ((currentChar == '/') && (lastChar == '*'))
                        {
                            inBlockComments = false;
                            endIndex++;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if ((currentChar == '*') && (lastChar == '/'))
                    {
                        inBlockComments = true;
                        shiftOffset = ShiftOffset.EndIndex;
                        endIndex--;
                    }
                    else if (inLineComment)
                    {
                        if (currentChar == '\n')
                        {
                            inLineComment = false;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (currentChar == '#')
                    {
                        inLineComment = true;
                        shiftOffset = ShiftOffset.EndIndex;
                    }
                    else if ((currentChar == '-') && (lastChar == '-'))
                    {
                        inLineComment = true;
                        shiftOffset = ShiftOffset.EndIndex;
                        endIndex--;
                    }
                    else if (currentChar == '`' || currentChar == '[' || (currentChar == '\"' && ansiQuotes))
                    {
                        inQuotes = true;
                        if (currentChar == '[')
                        {
                            quoteEndChar = ']';
                        }
                        else
                        {
                            quoteEndChar = currentChar;
                        }
                    }
                    else if ((currentChar == ',') || (currentChar == '(') || (currentChar == ')'))
                    {
                        yieldCurrentChar = true;
                    }
                    else if (!char.IsWhiteSpace(currentChar))
                    {
                        continue;
                    }
                }

                if (startIndex < endIndex)
                {
                    yield return input.Substring(startIndex, endIndex - startIndex);
                }
                if (yieldCurrentChar)
                {
                    yield return currentChar.ToString();
                }
                switch (shiftOffset)
                {
                    case ShiftOffset.IndexPlus1:
                        startIndex = index + 1;
                        break;
                    case ShiftOffset.EndIndex:
                        startIndex = endIndex;
                        break;
                }
            }
            if (startIndex < input.Length)
            {
                yield return input.Substring(startIndex);
            }
        }
    }
}
