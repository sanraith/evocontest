using NUnit.Framework;

namespace evocontest.Submission.Test.Tests
{
    /// <summary>
    /// Rules:
    /// - Word: >=2 character long string of lowercase [a-z] letters.
    /// - Acronym: >=2 character long string formed from words and acronyms. Only UPPERCASE [A-Z] letters.
    ///     - Acronyms can be formed from words by taking the first letter of each.
    ///         E.g.: apple + pear = AP
    ///     - Acronyms can be joined to form a new acronym.
    ///         E.g.: AB + CD = ABCD
    ///     - Acronyms and words can be joined to form a new acronym.
    ///         E.g.: apple + BC + date = ABCD
    /// - Sentence: >=1 word long string terminated by '.', where words are separated by ' '.
    /// - Text: >=1 sentence long string where sentences are separated by ' '.
    /// </summary>
    public sealed class NewSubmissionTest : TestBase
    {
        [Test]
        public void Solve_EmptyInput_SameOutput()
        {
            AssertSolve(string.Empty, string.Empty);
        }

        [Test]
        public void Solve_NoAbbreviation_SameOutput()
        {
            const string input = "sample text without abbreviation.";
            const string expected = input;
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_SinglePhrase_Replaced()
        {
            const string input = "the goal is to abbreviate multi word phrases. " +
                                 "multi word phrases cause longer text.";
            const string expected = "the goal is to abbreviate MWP. MWP cause longer text.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_MultiplePhrases_Replaced()
        {
            const string input = "first phrase. second phrase. first phrase. second phrase.";
            const string expected = "FP. SP. FP. SP.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_SubsetPhrase_ParentAndChildReplaced()
        {
            const string input = "multi word long phrase. long phrase. multi word long phrase.";
            const string expected = "MWLP. LP. MWLP.";
            AssertSolve(input, expected);
        }

        // [Test] 
        public void TODO_Solve_PhraseWithSuffx_Replaced() //TODO
        {
            const string input = "simple phrase. simple phrases.";
            const string expected = "SP. SPs.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_RecursivePhrases_Replaced()
        {
            const string input = "patient data service. patient data service handler. PDS handler.";
            const string expected = "PDS. PDSH. PDSH.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_ComplexRecursivePhrases_Replaced()
        {
            const string input = "aa bb cc dd. aa bb cc dd. aa BCD. ABC dd.";
            const string expected = "ABCD. ABCD. ABCD. ABCD.";
            AssertSolve(input, expected);
        }

        //[Test]
        public void TODO_Solve_ConflictingAbbreviations_NotReplaced()
        {
            const string input = "tim cook. tim cook. total commander. total commander.";
            const string expected = "tim cook. tim cook. total commander. total commander.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_ConflictingWordAndAcronym_WordNotReplaced()
        {
            const string input = "aa bb aa bb. AB cc AB cc. ab cc.";
            const string expected = "AB AB. ABC ABC. ab cc.";
            AssertSolve(input, expected);
        }

        //[Test]
        public void TODO_Solve_OverlappingPhrases_LongerReplaced()
        {
            const string input = "multi word long phrase. multi word long. long phrase.";
            const string expected = "MWL phrase. MWL. long phrase.";
            AssertSolve(input, expected);
        }
    }
}
