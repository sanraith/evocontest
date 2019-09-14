using System;
using System.Collections.Generic;
using System.Linq;

namespace evorace.Runner.Common.Messages.Response
{
    [Serializable]
    public class UnitTestResultMessage : AbstractMessage
    {
        public bool IsAllPassed => myTestResults.Values.All(x => x == true);

        public IEnumerable<string> FailedTests => myTestResults.Where(x => x.Value == false).Select(x => x.Key);

        public UnitTestResultMessage(Dictionary<string, bool?> testResults)
        {
            myTestResults = testResults;
        }

        private readonly Dictionary<string, bool?> myTestResults;
    }
}
