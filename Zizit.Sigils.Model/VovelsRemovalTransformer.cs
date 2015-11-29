using System.Text;

namespace Zizit.Sigils.Model
{
    public class VovelsRemovalTransformer : ITextTransformer
    {
        private string _notSupportedLetters = "aeiouAEIOU";

        public string Transform(string input)
        {
            var result = new StringBuilder();

            foreach (var letter in input)
            {
                lock (_notSupportedLetters)
                {
                    if (!_notSupportedLetters.Contains(letter.ToString()))
                    {
                        result.Append(letter);
                    }
                }
            }

            return result.ToString();
        }
    }
}
