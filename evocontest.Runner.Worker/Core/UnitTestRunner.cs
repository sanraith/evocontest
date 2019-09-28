using evocontest.Submission.Test.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace evocontest.Runner.Worker.Core
{
    public sealed class UnitTestRunner
    {
        public UnitTestRunner(Type solutionType)
        {
            mySolutionType = solutionType;
        }

        public Dictionary<string, bool?> RunTests()
        {
            var testClass = new SubmissionTestBase();
            testClass.TestedType = mySolutionType;

            var tests = testClass.GetType().GetMethods()
                .Where(x => x.CustomAttributes.Any(x => x.AttributeType.Name == "TestAttribute"))
                .ToList();

            var testResults = tests.ToDictionary(x => x.Name, _ => (bool?)null);
            foreach (var testMethod in tests)
            {
                var success = false;
                try
                {
                    testClass.Setup();
                    testMethod.Invoke(testClass, new object[0]);
                    success = true;
                }
                catch (Exception)
                {
                    break;
                }
                finally
                {
                    testResults[testMethod.Name] = success;
                }
            }

            return testResults;
        }

        private readonly Type mySolutionType;
    }
}
