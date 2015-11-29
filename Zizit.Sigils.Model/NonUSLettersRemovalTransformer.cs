using System.Text;

namespace Zizit.Sigils.Model
{
    public class NonUSLettersRemovalTransformer : ITextTransformer
    {
        private string _allSupportedLetters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public string Transform(string input)
        {
            var result = new StringBuilder();

            foreach (var letter in input)
            {
                lock (_allSupportedLetters)
                {
                    if (_allSupportedLetters.Contains(letter.ToString()))
                    {
                        result.Append(letter);
                    }
                }
            }

            return result.ToString();
        }
    }
}
