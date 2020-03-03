using System.Collections.Concurrent;
using System.Collections.Generic;
using CrossinformTask.Core;
using Xunit;

namespace CrossunformTask.Tests
{

    public class ParallelTripletsFinderTests
    {
        private string text = "abracadabra";
        private string path = "./Resources/test.txt";

        private ConcurrentDictionary<string, int> result = new ConcurrentDictionary<string, int>()
        {
            ["abr"] = 2,
            ["bra"] = 2,
            ["rac"] = 1,
            ["aca"] = 1,
            ["cad"] = 1,
            ["ada"] = 1,
            ["dab"] = 1,
        };

        [Fact]
        public void TestFindInString()
        {
            var res = new ConcurrentDictionary<string, int>();

            ParallelTripletsFinder.FindInString(text, res);

            Assert.False(res.Count != result.Count);
            Assert.Equal(result, res);
        }

        [Fact]
        public void TestFindInFile()
        {
            var res = new ConcurrentDictionary<string, int>();

            ParallelTripletsFinder.FindInFile(path, res);

            Assert.False(res.Count != result.Count);
            Assert.Equal(result, res);
        }

        [Fact]
        public void TestFindInBigFile()
        {
            var res = new ConcurrentDictionary<string, int>();

            ParallelTripletsFinder.FindInBigFile(path, res, 4);

            Assert.False(res.Count != result.Count);
            Assert.Equal(result, res);
        }
    }
}