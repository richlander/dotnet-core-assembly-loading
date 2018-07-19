using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Loader;
using System.Threading.Tasks;
using interfaces;
using ComponentHost;
using ComponentResolverStrategies;

namespace guttenapp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var books = new Dictionary<string,string> {
                {"Pride and Prejudice", "http://www.gutenberg.org/files/1342/1342-0.txt"},
                {"The War That Will End War", "http://www.gutenberg.org/files/57481/57481-0.txt"},
                {"Alice’s Adventures in Wonderland","http://www.gutenberg.org/files/11/11-0.txt"},
                {"Dracula","http://www.gutenberg.org/cache/epub/345/pg345.txt"},
                {"The Iliad of Homer","http://www.gutenberg.org/cache/epub/6130/pg6130.txt"},
                {"Dubliners","http://www.gutenberg.org/files/2814/2814-0.txt"},
                {"Gulliver's Travels","http://www.gutenberg.org/files/829/829-0.txt"}
                };

            //var assemblyResolver = new ComponentResolver();
            //assemblyResolver.SetBaseDirectory(typeof(Program).Assembly);

            var wordCountName = "wordcount";
            var wordcountContext = new ComponentContext(
#if DEBUG
                new ComponentResolverBinDirectoryStrategy(wordCountName),
#endif
                new ComponentResolverProductionStrategy(wordCountName)
#if !DEBUG
                ,
                new ComponentResolverProductionStrategy(wordCountName)
#endif

            );

            var mostcommonwordsName = "mostcommonwords";
            var mostcommonwordsContext = new ComponentContext(
#if DEBUG
                new ComponentResolverBinDirectoryStrategy(mostcommonwordsName),
#endif
                new ComponentResolverProductionStrategy(mostcommonwordsName)
#if !DEBUG
                ,
                new ComponentResolverProductionStrategy(mostcommonwordsName)
#endif

            );

            var wordCountAsm = wordcountContext.LoadAssemblyWithResolver("wordcount.dll");
            var mostcommonwordsAsm = mostcommonwordsContext.LoadAssemblyWithResolver("mostcommonwords.dll");
            
            var client = new HttpClient();

            foreach(var book in books)
            {
                var url = book.Value;
                using(var stream = await client.GetStreamAsync(url))
                using(var reader = new StreamReader(stream))
                {
                    var wordCount = (IProvider)wordCountAsm.CreateInstance("Lit.WordCount");
                    var mostcommonwords = (IProvider)mostcommonwordsAsm.CreateInstance("Lit.MostCommonWords");
                    Task<string> line;
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLineAsync();
                        try
                        {
                            var wordCountTask =  wordCount.ProcessTextAsync(line);
                            var mostcommonwordsTask = mostcommonwords.ProcessTextAsync(line);
                            await Task.WhenAll(wordCountTask, mostcommonwordsTask);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            throw e;
                        }
                    }

                    var wordcountReport = wordCount.GetReport();
                    Console.WriteLine($"Book: {book.Key}; Word Count: {wordcountReport["count"]}");
                    Console.WriteLine("Most common words, with count:");
                    var mostcommonwordsReport = mostcommonwords.GetReport();
                    var orderedMostcommonwords = (IOrderedEnumerable<KeyValuePair<string,int>>)mostcommonwordsReport["words"];
                    var mostcommonwordsCount = (int)mostcommonwordsReport["count"];

                    var index = 0;
                    foreach (var word in orderedMostcommonwords)
                    {
                        if (index++ >= 10)
                        {
                            break;
                        }
                        Console.WriteLine($"{word.Key}; {word.Value}");
                    }
                }
            }

            Console.WriteLine();
            foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var context = AssemblyLoadContext.GetLoadContext(asm);
                var def = AssemblyLoadContext.Default;
                var isDefaultContext = context == def;
                var isWordcountContext = context == wordcountContext;
                var isMostcommonwordsContext = context == mostcommonwordsContext;

                if (asm.FullName.StartsWith("System") && isDefaultContext)
                {
                    continue;
                }

                Console.WriteLine($"{asm.FullName}  {asm.Location}");
                Console.WriteLine($"Default: {isDefaultContext}; WordCount: {isWordcountContext}; MostCommonWords: {isMostcommonwordsContext}");
            }
            
        }
    }
}
