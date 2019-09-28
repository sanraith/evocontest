using evocontest.Submission.Sample;
using evocontest.Submission.Test.Core;
using NUnit.Framework;

namespace evocontest.Submission.Test
{
    /// <summary>
    /// Tests the sample submission code. 
    /// Test cases are defined in the base class.
    /// </summary>
    [TestFixture]
    public sealed class SampleSubmissionTest : SubmissionTestBase
    {
        public SampleSubmissionTest()
        {
            TestedType = typeof(SampleSubmission);
        }
    }
}
