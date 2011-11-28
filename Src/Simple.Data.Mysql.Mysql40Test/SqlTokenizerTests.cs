using NUnit.Framework;
using Simple.Data.Mysql.Mysql40;

namespace Simple.Data.Mysql.Mysql40Test
{
    [TestFixture]
    public class SqlTokenizerTest
    {
        [Test]
        public void ShouldHandleSpaces()
        {
            var tokens = SqlTokenizer.Tokenize("word1 word2 word3");
            CollectionAssert.AreEqual(new[] { "word1", "word2", "word3" }, tokens);
        }

        [Test]
        public void ShouldHandleAngleQuotedToken()
        {
            var tokens = SqlTokenizer.Tokenize("word1 `word2 word3`");
            CollectionAssert.AreEqual(new[] { "word1", "word2 word3" }, tokens);
        }

        [Test]
        public void ShouldHandleBracketQuotedToken()
        {
            var tokens = SqlTokenizer.Tokenize("word1[word2 word3]");
            CollectionAssert.AreEqual(new[] { "word1", "word2 word3" }, tokens);
        }

        [Test]
        public void ShouldHandleAnsiQuoted_AnsiQuotingOff()
        {
            var tokens = SqlTokenizer.Tokenize("word1\"word2 word3\"");
            CollectionAssert.AreEqual(new[] { "word1\"word2", "word3\"" }, tokens);
        }

        [Test]
        public void ShouldHandleAnsiQuoted_AnsiQuotingOn()
        {
            var tokens = SqlTokenizer.Tokenize("word1\"word2 word3\"", ansiQuotes: true);
            CollectionAssert.AreEqual(new[] { "word1", "word2 word3" }, tokens);
        }

        [Test]
        public void ShouldHandleEscapedAngleQuote_BackslashEscapesOn()
        {
            var tokens = SqlTokenizer.Tokenize(@"word1 `\`word2 word3`");
            CollectionAssert.AreEqual(new[] { "word1", @"\`word2 word3" }, tokens);
        }

        [Test]
        public void ShouldHandleEscapedAngleQuote_BackslashEscapesOff()
        {
            var tokens = SqlTokenizer.Tokenize(@"word1 `\`word2 word3`", useBackslashEscaping: false);
            CollectionAssert.AreEqual(new[] { "word1", @"\", "word2", "word3" }, tokens);
        }

        [Test]
        public void ShouldHandleQuotedLiteral()
        {
            var tokens = SqlTokenizer.Tokenize("word1 'word2 word3'");
            CollectionAssert.AreEqual(new[] { "word1", "'word2 word3'" }, tokens);
        }

        [Test]
        public void ShouldHandleAngleQuoteInQuotedLiteral()
        {
            var tokens = SqlTokenizer.Tokenize("word1 'word2` word3'");
            CollectionAssert.AreEqual(new[] { "word1", "'word2` word3'" }, tokens);
        }

        [Test]
        public void ShouldHandleEscapedQuoteInQuotedLiteral()
        {
            var tokens = SqlTokenizer.Tokenize(@"word1 'word2\' word3'");
            CollectionAssert.AreEqual(new[] { "word1", @"'word2\' word3'" }, tokens);
        }

        [Test]
        public void ShouldHandleQuoteInQuotedLiteral()
        {
            var tokens = SqlTokenizer.Tokenize("word1 'word2' word3'");
            CollectionAssert.AreEqual(new[] { "word1", "'word2'", "word3'" }, tokens);
        }

        [Test]
        public void ShouldHandleTwoQuotesInQuotedLiteral()
        {
            var tokens = SqlTokenizer.Tokenize("word1 'word2'' word3'");
            CollectionAssert.AreEqual(new[] { "word1", "'word2'' word3'" }, tokens);
        }

        [Test]
        public void ShouldHandleThreeQuotesInQuotedLiteral()
        {
            var tokens = SqlTokenizer.Tokenize("word1 'word2''' word3'");
            CollectionAssert.AreEqual(new[] { "word1", "'word2'''", "word3'" }, tokens);
        }

        [Test]
        public void ShouldHandleParenthesis()
        {
            var tokens = SqlTokenizer.Tokenize("word1(word2)word3");
            CollectionAssert.AreEqual(new[] { "word1", "(", "word2", ")", "word3" }, tokens);
        }

        [Test]
        public void ShouldHandleParenthesis_WithSpaces()
        {
            var tokens = SqlTokenizer.Tokenize("word1 ( word2 ) word3");
            CollectionAssert.AreEqual(new[] { "word1", "(", "word2", ")", "word3" }, tokens);
        }

        [Test]
        public void ShouldHandleCommas()
        {
            var tokens = SqlTokenizer.Tokenize("word1,word2,word3");
            CollectionAssert.AreEqual(new[] { "word1", ",", "word2", ",", "word3" }, tokens);
        }

        [Test]
        public void ShouldHandleCommas_WithSpaces()
        {
            var tokens = SqlTokenizer.Tokenize("word1 , word2, word3");
            CollectionAssert.AreEqual(new[] { "word1", ",", "word2", ",", "word3" }, tokens);
        }

        [Test]
        public void ShouldHandleBlockComment()
        {
            var tokens = SqlTokenizer.Tokenize("word1/*word2\nword3 word4\n word5*/word6");
            CollectionAssert.AreEqual(new[] { "word1", "/*word2\nword3 word4\n word5*/", "word6" }, tokens);
        }

        [Test]
        public void ShouldHandleBlockComment_WithNewLineBefore()
        {
            var tokens = SqlTokenizer.Tokenize("word1\n/*word2\nword3 word4\n word5*/word6");
            CollectionAssert.AreEqual(new[] { "word1", "/*word2\nword3 word4\n word5*/", "word6" }, tokens);
        }

        [Test]
        public void ShouldHandleBlockComment_WithSpaceBefore()
        {
            var tokens = SqlTokenizer.Tokenize("word1 /*word2\nword3 word4\n word5*/word6");
            CollectionAssert.AreEqual(new[] { "word1", "/*word2\nword3 word4\n word5*/", "word6" }, tokens);
        }

        [Test]
        public void ShouldHandleHashedLineComment()
        {
            var tokens = SqlTokenizer.Tokenize("word1#word2 word3\nword4");
            CollectionAssert.AreEqual(new[] { "word1", "#word2 word3", "word4" }, tokens);
        }

        [Test]
        public void ShouldHandleHashedLineComment_WithNewLineBefore()
        {
            var tokens = SqlTokenizer.Tokenize("word1\n#word2 word3\nword4");
            CollectionAssert.AreEqual(new[] { "word1", "#word2 word3", "word4" }, tokens);
        }

        [Test]
        public void ShouldHandleHashedLineComment_WithSpaceBefore()
        {
            var tokens = SqlTokenizer.Tokenize("word1 #word2 word3\nword4");
            CollectionAssert.AreEqual(new[] { "word1", "#word2 word3", "word4" }, tokens);
        }

        [Test]
        public void ShouldHandleDashedLineComment()
        {
            var tokens = SqlTokenizer.Tokenize("word1--word2 word3\nword4");
            CollectionAssert.AreEqual(new[] { "word1", "--word2 word3", "word4" }, tokens);
        }

        [Test]
        public void ShouldHandleDashedLineComment_WithNewLineBefore()
        {
            var tokens = SqlTokenizer.Tokenize("word1\n--word2 word3\nword4");
            CollectionAssert.AreEqual(new[] { "word1", "--word2 word3", "word4" }, tokens);
        }

        [Test]
        public void ShouldHandleDashedLineComment_WithSpaceBefore()
        {
            var tokens = SqlTokenizer.Tokenize("word1 --word2 word3\nword4");
            CollectionAssert.AreEqual(new[] { "word1", "--word2 word3", "word4" }, tokens);
        }
    }
}