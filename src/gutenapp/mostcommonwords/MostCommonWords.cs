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
            var wasSpace = false;
            var lengthTest = text.Length -1;
            var start = 0;
            var index = 0;
            var newwords = 0;

            if (text == string.Empty)
            {
                return 0;
            }
            
            Span<char> buffer = stackalloc char[100];
            
            while (true)
            {
                
                var c = text[index];
                var isSpace = c == ' ';
                string loweredWord = String.Empty;

                if (index >= lengthTest && index > 0)
                {
                    var span = text.AsSpan(start);

                    if (text.Length >100)
                    {
                        loweredWord = span.ToString().ToLowerInvariant();
                    }
                    else
                    {
                        span.ToLowerInvariant(buffer);
                        loweredWord = buffer.Slice(0, text.Length).ToString();
                    }
                    newwords += AddWord(loweredWord);
                    break;
                }
                else if (isSpace && !wasSpace)
                {
                    var end = index - start;
                    var span = text.AsSpan(start, end);
                    if (text.Length > 100)
                    {
                        loweredWord = span.ToString().ToLowerInvariant();
                    }
                    else
                    {
                        span.ToLowerInvariant(buffer);
                        loweredWord = buffer.Slice(0, end).ToString();
                    }
                    wasSpace = true;
                    start = index + 1;
                    newwords += AddWord(loweredWord);
                }
                else if (!isSpace && wasSpace)
                {
                    wasSpace = false;
                }
                index++;
            }
            return newwords;
        }

        private int AddWord(string word)
        {
            if (word == string.Empty)
            {
                return 0;
            }
            else if (_wordsCount.ContainsKey(word))
            {
                _wordsCount[word]++;
                return 1;
            }
            else
            {
                _wordsCount.Add(word, 1);
                return 0;
            }
        }
    }
}
