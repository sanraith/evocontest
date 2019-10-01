﻿using NUnit.Framework;

namespace evocontest.Submission.Test.Core
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
    /// - Text: >=0 sentence long string where sentences are separated by ' '.
    /// 
    /// 3. make rule, that longer abbreviations are replaced first
    /// Longer = number of words, or first occurence
    /// no overlap allowed
    /// </summary>
    public abstract class SubmissionTestBase : TestBase
    {
        [Test]
        public void Solve_EmptyInput_EmptyOutput()
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
            const string input = "aa bb cc dd. aa bb cc dd. aa BCD. ABC dd.";
            const string expected = "ABCD. ABCD. ABCD. ABCD.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_SubsetPhrase_ParentAndChildIsReplaced()
        {
            const string input = "patient data service. patient data service handler. PDS handler.";
            const string expected = "PDS. PDSH. PDSH.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_SubsetPhrase_ParentAndChildIsReplaced2()
        {
            const string input = "patient data service. patient data service handler. patient data service handler.";
            const string expected = "PDS. PDSH. PDSH.";
            AssertSolve(input, expected);
        }
        
        [Test]
        public void Solve_SubsetPhrase_ParentAndChildIsReplaced3()
        {
            const string input = "patient data service handler. patient data service patient data service. patient data service handler.";
            const string expected = "PDSH. PDS PDS. PDSH.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_ConflictingWordAndAcronym_ConflictingWordNotReplaced()
        {
            const string input = "aa bb aa bb. AB cc AB cc. ab cc.";
            const string expected = "AB AB. ABC ABC. ab cc.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_ConflictingAbbreviations_NotReplaced()
        {
            string input = "tim cook. tim cook. total commander. total commander.";
            string expected = "tim cook. tim cook. total commander. total commander.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_ConflictingAbbreviations_NotReplaced2()
        {
            const string input = "apple banana cold. apple banana cold AB connor. AB connor.";
            const string expected = "AB cold. AB cold AB connor. AB connor.";
            AssertSolve(input, expected);
        }

        [Test]
        public void Solve_PartiallyConflictingAbbreviations_AreReplaced()
        {
            const string input = "apple apple pear apple apple pear. aa aa cc aa aa cc.";
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
            // SP would be conflicting for "simple phrase" and "simple phrases"
            const string input = "simple phrase. simple phrase. simple phrases.";
            const string expected = "simple phrase. simple phrase. simple phrases.";
            AssertSolve(input, expected);
        }
    }
}