using evorace.Submission.Test.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace evorace.Runner.Worker.Core
{
    public sealed class UnitTestRunner 
    {
        public UnitTestRunner(Type solutionType)
        {
            mySolutionType = solutionType;
        }

        public void RunTests()
        {
            var testClass = new SimpleSubmissionTest();
            testClass.SubmissionType = mySolutionType;

            //var tests = typeof(SimpleSubmissionTest).GetMethods().Where(x=>x.GetCustomAttributes(false)) // TODO get tests
        }

        private readonly Type mySolutionType;
    }
}
