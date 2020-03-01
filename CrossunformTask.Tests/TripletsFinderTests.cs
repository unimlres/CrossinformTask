using System.Collections.Generic;
using CrossinformTask.Core;
using Xunit;

namespace CrossunformTask.Tests
{
    public class TripletsFinderTests
    {
        private string text = "abracadabra";
        private string path = "./Resources/test.txt";

        private Dictionary<string, int> result = new Dictionary<string, int>()
        {
            ["abr"] = 2,
            ["bra"] = 2,
            ["rac"] = 1,
            ["aca"] = 1,
            ["cad"] = 1,
            ["ada"] = 1,
            ["dab"] = 1
        };

        [Fact]
        public void TestFindInString()
        {
            var res = new Dictionary<string, int>();

            TripletsFinder.FindInString(text, res);

            Assert.False(res.Count != result.Count);
            Assert.Equal(result, res);
        }

        [Fact]
        public void TestFindInFile()
        {
            var res = new Dictionary<string, int>();

            TripletsFinder.FindInFile(path, res);

            Assert.False(res.Count != result.Count);
            Assert.Equal(result, res);
        }
    }
}
