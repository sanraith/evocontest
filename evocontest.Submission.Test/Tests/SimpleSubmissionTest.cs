using NUnit.Framework;

namespace evocontest.Submission.Test.Tests
{
    [TestFixture]
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

        // TODO enforce in input generation
        [Test]
        public void TODO_Solve_ConflictingAbbreviations_NotRemovedNotReplaced()
        {
            const string input = "Apple(the iPhone company) makes money. Apple(the red fruit) tastes good. " +
                                 "The iPhone company is not the same as the red fruit.";
            const string expected = input;
            AssertSolve(input, expected);
        }

        // TODO consider removing this option
        // Pro:
        //      Simpler sample code
        // Contra:
        //      Simpler solutions => faster completion for everyone?
        //      More complex input generation
        [Test]
        public void TODO_Solve_ComplexAbbreviations_AreReplaced()
        {
            const string input = "SIMPLE(Simple abbreviation). " +
                                 "COMPLEX(complex includes simple abbreviation). " +
                                 "Complex includes SIMPLE.";
            const string expected = "SIMPLE. COMPLEX. COMPLEX.";
            AssertSolve(input, expected);
        }

        // TODO enforce this in input generation
        [Test]
        public void TODO_Solve_RecursiveAbbreviations_MultipleSolution()
        {
            // A D D A D D D A D D A D D
            // C   D C   D D C   D C   D

            // C D C D D C D C D
            // B   B   D B   B

            // B B D B B

            //const string input = "A(B C). B(C D). C(A D). A D D A D.";
            const string input = "A(B C). B(C D). C(A D). B C D D B C D.";
            //const string input = "A(B C). B(C D). C(A D). A D D A D D D A D D A D D."; // TODO Multiple result!!!
            //const string input = "A(B C). B(C D). C(A D). C D C D D C D C D.";
            //const string input = "A(B C). B(C D). C(A D). B B D B B.";


            const string expected = "A. B. C. A.";
            AssertSolve(input, expected);
        }

        [Test]
        public void TODO_Solve_ComplexAbbreviation_MultipleSolution() // May be resolved by longest rule
        {
            const string input = "B(C D). Z(B C D). B C D.";
            const string expected = "B. Z. Z.";

            AssertSolve(input, expected);
        }

        [Test]
        public void TODO_Solve_ComplexAbbreviation_MultipleSolution2()
        {
            const string input = "X(A B). Y(B C). Z(A Y). A B C.";
            const string expected = "X. Y. Z. Z.";

            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_RecursiveAbbreviation_Replaced()
        {
            const string input = "WINE(WINE is not an emulator). WINE is not an emulator is not an emulator is not an emulator is not an emulator.";
            const string expected = "WINE. WINE.";
            AssertSolve(input, expected);
        }
    }
}
