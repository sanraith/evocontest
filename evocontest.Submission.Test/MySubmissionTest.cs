using evocontest.Submission.Test.Core;
using NUnit.Framework;

namespace evocontest.Submission.Test
{
    /// <summary>
    /// Tests your submission. 
    /// Test cases are defined in the base class.
    /// </summary>
    [TestFixture]
    public sealed class MySubmissionTest : SubmissionTestBase
    {
        public MySubmissionTest()
        {
            TestedType = typeof(MySubmission);
        }
    }
}
