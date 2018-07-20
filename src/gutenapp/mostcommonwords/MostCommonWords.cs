using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using interfaces;

namespace Lit
{
    public class MostCommonWords : IProvider
    {
        private Dictionary<string, int> _wordsCount = new Dictionary<string, int>();

        public Dictionary<string, object> GetReport()
        {
            var orderedList = _wordsCount.OrderByDescending( e=> { return e.Value;});
            var report = new Dictionary<string,object>();
            report.Add("words",orderedList);
            report.Add("count",_wordsCount.Count);
            return report;
        }

        public async Task<int> ProcessTextAsync(Task<string> text)
        {
            var line = await text;
            return UpdateWordCounts(line);
        }

        private int UpdateWordCounts(string text)
        {
            var newwords = 0;
            foreach (var word in GetWords(text))
            {
                if (word == string.Empty)
                {
                    continue;
                }
                else if(_wordsCount.ContainsKey(word))
                {
                    _wordsCount[word]++;
                }
                else
                {
                    newwords++;
                    _wordsCount.Add(word,1);
                }
            }
            return newwords;
        }

        private IEnumerable<string> GetWords(string text)
        {
            var wasSpace = false;
            var lengthTest = text.Length -1;
            var start = 0;
            var index = 0;
            var loop = true;

            if (text == string.Empty)
            {
                loop = false;
                yield return string.Empty;
            }
            
            Span<char> d = stackalloc char[255];
            
            while (loop)
            {
                
                var c = text[index];
                var isSpace = c == ' ';
                
                if (index >= lengthTest && index > 0)
                {
                    var span = text.AsSpan(start);
                    span.ToLowerInvariant(d);
                    var loweredWord = d.Slice(0,text.Length).ToString();
                    yield return loweredWord;
                    break;
                }
                else if (isSpace && !wasSpace)
                {
                    var span = text.AsSpan(start, index - start);
                    var loweredWord = span.ToString().ToLowerInvariant();
                    wasSpace = true;
                    start = index + 1;
                    yield return loweredWord;

                }
                else if (!isSpace && wasSpace)
                {
                    wasSpace = false;
                }
                index++;
            }
        }
                
    }
}
