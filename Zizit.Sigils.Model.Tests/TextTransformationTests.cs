using System.Linq;
using NUnit.Framework;

namespace Zizit.Sigils.Model.Tests
{
    [TestFixture]
    public class Text_transformations
    {
        [TestCase("abcdefghijklmnopqrstuvwxyz", ExpectedResult = "bcdfghjklmnpqrstvwxyz")]
        [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ", ExpectedResult = "BCDFGHJKLMNPQRSTVWXYZ")]
        [TestCase("abcedefaghijklmnaeopqrstuveawxyz", ExpectedResult = "bcdfghjklmnpqrstvwxyz")]
        public string Vovels_removal(string input)
        {
            var vovelsRemovalTransformer = new VovelsRemovalTransformer();

            return vovelsRemovalTransformer.Transform(input);
        }

        [TestCase("\tabc defghijklmn__&%^&^%opqrs tuvwx yz", ExpectedResult = "abcdefghijklmnopqrstuvwxyz")]
        [TestCase("AB\nCD  EF,.,.GHIJKL\tMNOPQRST&*^UVW\rXYZ", ExpectedResult = "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [TestCase("   aĘĖČĘĖbce\r\r\r\r\r\n\n\n\t\t\t\tdefaghijklmnaeopqr++-/*stuveawxyz\n\r", ExpectedResult = "abcedefaghijklmnaeopqrstuveawxyz")]
        public string Non_US_letters_removal(string input)
        {
            var nonUSLettersTransformer = new NonUSLettersRemovalTransformer();

            return nonUSLettersTransformer.Transform(input);
        }

        [TestCase("Thisprogramshouldwork", ExpectedResult = "Tipgamshuldwork")]
        public string Lowercasing_and_duplicate_letters_rl_removal(string input)
        {
            var lowercasingAndDuplicateLettersRlRemovalTransformer = new LowercasingAndDuplicateLettersRlRemovalTransformer();

            return lowercasingAndDuplicateLettersRlRemovalTransformer.Transform(input);
        }

        [TestCase("bcdfghjklmnpqrstvwxyz", ExpectedResult = "234678123457891245678")]
        [TestCase("BCDFGHJKLMNPQRSTVWXYZ", ExpectedResult = "234678123457891245678")]
        public string Convert_to_numbers(string input)
        {
            var toNumbersTransformer = new ToNumbersTransformer();

            return toNumbersTransformer.Transform(input);
        }

        [TestCase("This program should work", ExpectedResult = "27741834592")]
        public string Full_cycle(string input)
        {
            var transformers = (new ITextTransformer[]
            {
                new NonUSLettersRemovalTransformer(),
                new VovelsRemovalTransformer(),
                new LowercasingAndDuplicateLettersRlRemovalTransformer(),
                new ToNumbersTransformer()
            }).ToList();

            transformers.ForEach(x => input = x.Transform(input));
            return input;
        }
    }
}
