using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using interfaces;

namespace guttenapp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var books = new Dictionary<string,string> {
                {"Pride and Prejudice", "http://www.gutenberg.org/files/1342/1342-0.txt"},
                {"The War That Will End War", "http://www.gutenberg.org/files/57481/57481-0.txt"},
                {" Alice’s Adventures in Wonderland","http://www.gutenberg.org/files/11/11-0.txt"},
                {"Dracula","http://www.gutenberg.org/cache/epub/345/pg345.txt"},
                {"The Iliad of Homer","http://www.gutenberg.org/cache/epub/6130/pg6130.txt"},
                {"Dubliners","http://www.gutenberg.org/files/2814/2814-0.txt"},
                {"Gulliver's Travels","http://www.gutenberg.org/files/829/829-0.txt"}
                };

            var wordCountPath = @"/Users/rlander/git/dotnet-core-assembly-loading/src/gutenapp/wordcount/bin/Debug/netstandard2.0/wordcount.dll";
            var (wordcountContext, wordCountAsm, wordCount) = ComponentContext.CreateContext<IProvider>(wordCountPath,"Lit.WordCount");
            
            var client = new HttpClient();

            foreach(var book in books)
            {
                var url = book.Value;
                using(var stream = await client.GetStreamAsync(url))
                using(var reader = new StreamReader(stream))
                {
                    Task<string> line;
                    while ((line = reader.ReadLineAsync()) != null)
                    {
                        var i = await wordCount.ProcessTextAsync(line);
                    }

                    
                }
            }
            
        }
    }
}
