using System;
using System.Threading.Tasks;

namespace Lit
{
    public class WordCount
    {
        int _count = 0;
        public WordCount(){}

        public int CountAdd(string text)
        {
            var wasSpace = false;
            var count = 0;
            foreach(var t in text)
            {
                var isSpace = Char.IsWhiteSpace(t);
                if (isSpace &&
                    !wasSpace)
                {
                    count++;
                    wasSpace = true;
                }
                else if (!isSpace && wasSpace)
                {
                    wasSpace = false;
                }
            }
            _count += count;
            return count;
        }

        public async Task<int> CountAddAsync(Task<string> text)
        {
            var t = await text;
            return CountAdd(t);
        }
    }
}
