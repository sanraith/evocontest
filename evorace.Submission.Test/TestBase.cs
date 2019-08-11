using evorace.Common;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace evorace.Submission.Test
{
    public abstract class TestBase
    {
        protected Type? SubmissionType { get; private set; }

        protected ISolution? Submission { get; private set; }

        public TestBase()
        { }

        public TestBase(Type submissionType)
        {
            SubmissionType = submissionType;
        }

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            if (SubmissionType != null)
            {
                return;
            }

            var config = new ConfigurationBuilder().AddJsonFile("testsettings.json", true, false).Build();
            var targetAssemblyPath = config["TargetAssembly"];
            var assembly = Assembly.LoadFrom(targetAssemblyPath);
            var submissionTypes = assembly.GetTypes().Where(x => x.GetInterfaces().Any(i => i == typeof(ISolution))).ToList();

            Assert.That(submissionTypes.Count, Is.GreaterThan(0), $"No valid {nameof(ISolution)} implementations found in {assembly.FullName}!");
            Assert.That(submissionTypes.Count, Is.EqualTo(1), $"Too many {nameof(ISolution)} implementations found in {assembly.FullName}!");
            SubmissionType = submissionTypes.Single();
        }

        [SetUp]
        public virtual void Setup()
        {
            if (SubmissionType == null) { throw new ArgumentNullException(nameof(SubmissionType)); }
            Submission = (ISolution?)Activator.CreateInstance(SubmissionType);
        }

        protected void AssertSolve(string input, string expected)
        {
            if (Submission == null) { throw new ArgumentNullException(nameof(Submission)); }

            var output = Submission.Solve(input);
            Assert.That(output, Is.EqualTo(expected));
        }
    }
}
