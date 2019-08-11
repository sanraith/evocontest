using NUnit.Framework;

namespace evorace.Submission.Test.Tests
{
    public sealed class SimpleSubmissionTest : TestBase
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
        public void Solve_SingleAbbreviation_IsRemoved()
        {
            const string input = "This is a SA(Single Abbreviation).";
            const string expected = "This is a SA.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_MismatchingCaseAbbreviations_AreReplaced()
        {
            const string input = "Great ABBR(abBRevIaTiOn). Abbreviation is useful.";
            const string expected = "Great ABBR. ABBR is useful.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_AbbreviationWithinWord_IsReplaced()
        {
            const string input = "VLW(VeryLongWord). NiceVeryLongWordCollection.";
            const string expected = "VLW. NiceVLWCollection.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_SingleAbbreviationMultipleOccurence_AllReplaced()
        {
            const string input = "Abbreviations are good. This is an ABBR(Abbreviation). " +
                                 "This is the same ABBR(Abbreviation). Abbreviations should be shortened.";
            const string expected = "ABBRs are good. This is an ABBR. This is the same ABBR. ABBRs should be shortened.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_ConflictingAbbreviations_NotRemovedNotReplaced()
        {
            const string input = "Apple(the iPhone company) makes money. Apple(the red fruit) tastes good. " +
                                 "The iPhone company is not the same as the red fruit.";
            const string expected = input;
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_ComplexAbbreviations_AreReplaced()
        {
            const string input = "SIMPLE(Simple abbreviation). " +
                                 "COMPLEX(complex includes simple abbreviation). " +
                                 "Complex includes SIMPLE.";
            const string expected = "SIMPLE. COMPLEX. COMPLEX.";
            AssertSolve(input, expected);
        }
    }
}
