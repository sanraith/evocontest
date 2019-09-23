using NUnit.Framework;

namespace evocontest.Submission.Test.Tests
{
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
            const string input = "Sample text without abbreviation.";
            const string expected = input;
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_SinglePhrase_Replaced()
        {
            const string input = "The goal is to abbreviate Multi Word Phrases. " +
                                 "Multi word phrases cause longer text.";
            const string expected = "The goal is to abbreviate MWP. MWP cause longer text.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_MultiplePhrases_Replaced()
        {
            const string input = "First Phrase. Second phrase. First phrase. Second phrase.";
            const string expected = "FP. SP. FP. SP.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_SubsetPhrase_ParentAndChildReplaced()
        {
            const string input = "Multi word long phrase. Long phrase. Multi word long phrase.";
            const string expected = "MWLP. LP. MWLP.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_PhraseWithSuffx_NotReplaced()
        {
            const string input = "Simple phrase. Simple phrases.";
            const string expected = "Simple phrase. Simple phrases.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_RecursivePhrases_Replaced()
        {
            const string input = "Patient Data Service. Patient Data Service Handler. PDS Handler.";
            const string expected = "PDS. PDSH. PDSH.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_ConflictingAbbreviations_NotReplaced()
        {
            const string input = "Tim Cook. Tim Cook. Total Commander. Total Commander.";
            const string expected = "Tim Cook. Tim Cook. Total Commander. Total Commander.";
            AssertSolve(input, expected);
        }

        [Test]
        public void TODO_Solve_OverlappingPhrases_LongerReplaced()
        {
            const string input = "Multi word long phrase. Multi word long. Long phrase.";
            const string expected = "MWL phrase. MWL. Long phrase.";
            AssertSolve(input, expected);
        }

        [Test]
        public void TODO_Solve_ConflictingWordAndAcronym_WordNotReplaced()
        {
            const string input = "Aa bb cc aa bb cc. ABC dd ABC dd. Abc dd.";
            const string expected = "ABC ABC. ABCD ABCD. Abc delta.";
            AssertSolve(input, expected);
        }
    }
}
