namespace evocontest.Common
{
    /// <summary>
    /// Represents a submission to the contest.
    /// Only a SINGLE class should implement this interface within a DLL.
    /// </summary>
    public interface ISolution
    {
        /// <summary>
        /// Solves the given input by replacing repeating phrases with acronyms.
        /// </summary>
        /// <param name="input">The input text.</param>
        /// <returns>The resulting text.</returns>
        string Solve(string input);
    }
}
