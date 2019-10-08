using NUnit.Framework;

namespace evocontest.Submission.Test.Core
{
    /// <summary>
    /// Rules:
    /// - Word: >=2 character long string of lowercase [a-z] letters.
    /// - Acronym: >=2 character long string formed from words and acronyms. Only UPPERCASE [A-Z] letters.
    ///     - Acronyms can be formed from words by taking the first letter of each in Uppercase.
    ///         E.g.: apple + pear = AP
    ///     - Acronyms can be joined to form a new acronym.
    ///         E.g.: AB + CD = ABCD
    ///     - Acronyms and words can also be joined to form a new acronym.
    ///         E.g.: apple + BC + date = ABCD
    ///     - An acronym can only be formed, when it occurs multiple times throughout the text.
    /// - Sentence: >=1 word long string terminated by '.', where words are separated by ' '.
    /// - Text: >=0 sentence long string where sentences are separated by ' '.
    /// 
    /// Input data:
    /// - Different acronyms will not overlap
    /// </summary>
    public abstract class SubmissionTestBase : TestBase
    {
        [Test]
        public void Solve_EmptyInput_EmptyOutput()
        {
            AssertSolve(string.Empty, string.Empty);
        }

        [Test]
        public void Solve_NoAcronym_SameOutput()
        {
            const string input = "sample text without abbreviation.";
            const string expected = input;
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_SinglePhrase_IsReplaced()
        {
            const string input = "the goal is to abbreviate multi word phrases. " +
                                 "multi word phrases cause longer text.";
            const string expected = "the goal is to abbreviate MWP. MWP cause longer text.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_MultiplePhrases_AreReplaced()
        {
            const string input = "first phrase. second phrase. first phrase. second phrase.";
            const string expected = "FP. SP. FP. SP.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_ComplexPhrases_AreReplaced()
        {
            const string input = "aa bb cc dd. aa BCD. ABC dd.";
            const string expected = "ABCD. ABCD. ABCD.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_LowercaseAcronym_NotReplaced()
        {
            const string input = "aa pp px ll ee. APPLE. appl ee.";
            const string expected = "APPLE. APPLE. appl ee.";

            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_ConflictingAcronyms_NotReplaced()
        {
            string input = "tim cook. tim cook. total commander.";
            string expected = "tim cook. tim cook. total commander.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_ConflictingAcronyms_NotReplaced2()
        {
            // "ABC" is conflictiong for "AB cold" and "AB connor"
            // "AB" is not conflicting.

            const string input = "apple banana cold. apple banana cold AB connor. apple banana connor.";
            const string expected = "AB cold. AB cold AB connor. AB connor.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_PartiallyConflictingAcronyms_ConflictedPartNotReplaced()
        {
            // "BC" is conflicting for "bb cc", "bx cx".
            // "ABC" is not conflicting.

            const string input = "aa BC. aa bb cc. bx cx. bx cx.";
            const string expected = "ABC. ABC. bx cx. bx cx.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_PartiallyConflictingButDifferentAcronyms_AreReplaced()
        {
            // "AA" is conflicting for "apple applet", "aa aat"
            // "AAP" and "AAC" are not conflicting.

            const string input = "apple applet pear apple applet pear. aa aat cc aa aat cc.";
            const string expected = "AAP AAP. AAC AAC.";
            AssertSolve(input, expected);
        }
        
        [Test]
        public void Solve_PhraseWithPrefix_PrefixedWordNotReplaced()
        {
            const string input = "simple phrase. simple phrase. megasimple phrase.";
            const string expected = "SP. SP. megasimple phrase.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_PhraseWithSuffix_NoneReplaced()
        {
            // "SP" would be a conflicting acronym for both "simple phrase" and "simple phrases"

            const string input = "simple phrase. simple phrase. simple phrases.";
            const string expected = "simple phrase. simple phrase. simple phrases.";
            AssertSolve(input, expected);
        }
    }
}
