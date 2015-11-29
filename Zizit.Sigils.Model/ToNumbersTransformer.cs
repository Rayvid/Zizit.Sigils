using System.Collections.Generic;
using System.Linq;

namespace Zizit.Sigils.Model
{
    public class ToNumbersTransformer : ITextTransformer
    {
        private Dictionary<char, char> _toNumbersMapping = new Dictionary<char, char>
        {
            { 'b', '2' },
            { 'c', '3' },
            { 'd', '4' },
            { 'f', '6' },
            { 'g', '7' },
            { 'h', '8' },
            { 'j', '1' },
            { 'k', '2' },
            { 'l', '3' },
            { 'm', '4' },
            { 'n', '5' },
            { 'p', '7' },
            { 'q', '8' },
            { 'r', '9' },
            { 's', '1' },
            { 't', '2' },
            { 'v', '4' },
            { 'w', '5' },
            { 'x', '6' },
            { 'y', '7' },
            { 'z', '8' },
            { 'B', '2' },
            { 'C', '3' },
            { 'D', '4' },
            { 'F', '6' },
            { 'G', '7' },
            { 'H', '8' },
            { 'J', '1' },
            { 'K', '2' },
            { 'L', '3' },
            { 'M', '4' },
            { 'N', '5' },
            { 'P', '7' },
            { 'Q', '8' },
            { 'R', '9' },
            { 'S', '1' },
            { 'T', '2' },
            { 'V', '4' },
            { 'W', '5' },
            { 'X', '6' },
            { 'Y', '7' },
            { 'Z', '8' }
        };

        public string Transform(string input)
        {
            return new string(input.Select(x => _toNumbersMapping[x]).ToArray());
        }
    }
}
