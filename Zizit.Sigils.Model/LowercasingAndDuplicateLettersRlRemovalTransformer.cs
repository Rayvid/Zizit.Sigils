namespace Zizit.Sigils.Model
{
    public class LowercasingAndDuplicateLettersRlRemovalTransformer : ITextTransformer
    {
        public string Transform(string input)
        {
            var result = string.Empty;

            for (var i = input.ToLower().Length - 1; i >= 0; i--)
            {
                if (!result.Contains(input[i].ToString()))
                {
                    result = input[i] + result;
                }
            }

            return result;
        }
    }
}
