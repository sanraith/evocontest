using evocontest.Common;
using NUnit.Framework;
using System;

namespace evocontest.Submission.Test.Core
{
    public abstract class TestBase
    {
        public Type? TestedType { get; set; }

        protected ISolution? Submission { get; private set; }

        protected TestBase()
        { }

        protected TestBase(Type submissionType)
        {
            TestedType = submissionType;
        }

        [SetUp]
        public virtual void Setup()
        {
            if (TestedType == null) { throw new ArgumentNullException(nameof(TestedType)); }
            Submission = (ISolution?)Activator.CreateInstance(TestedType);
        }

        protected void AssertSolve(string input, string expected)
        {
            if (Submission == null) { throw new ArgumentNullException(nameof(Submission)); }

            var output = Submission.Solve(input);
            Assert.That(output, Is.EqualTo(expected));
        }
    }
}
