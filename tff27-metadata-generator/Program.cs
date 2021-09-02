namespace Tff27.Markdown.Metadata.Generator
{
    using CommandLine;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    class Program
    {
        public enum HeaderReadStatus
        {
            NotStarted,
            Reading,
            Finished
        }

        public class Options
        {
            [Option('i', "InputPath", Required = true, HelpText = "Path containing markdown files (*.md).")]
            public string InputPath { get; set; }

            [Option('o', "OutputPath", Required = true, HelpText = "Path where JSON (*.json) metadata will be stored.")]
            public string OutputPath { get; set; }

            [Option('n', "FileName", Required = true, HelpText = "Name for metadata file.")]
            public string FileName { get; set; }

            [Option('f', "SortField", Required = false, HelpText = "Sort metadata by a header field.")]
            public string SortField { get; set; }

            [Option('s', "SortOrder", Required = false, HelpText = "Order Ascending/Descending. Default: Ascending")]
            public string SortOrder { get; set; } = "Ascending";
        }

        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(Run);
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine(exception.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred!");
            }
        }

        static void Run(Options opts)
        {
            if (!Directory.Exists(opts.InputPath))
            {
                throw new ArgumentException(message: $"Input directory does not exists.");
            }

            var files = Directory.GetFiles(opts.InputPath, "*.md");

            if (!files.Any())
            {
                throw new ArgumentException(message: $"No markdown files detected at the input directory.");
            }

            var postList = new List<dynamic>();

            foreach (var file in files)
            {
                postList.Add(CreateMetadataForMdFile(file));
            }

            var options = new JsonSerializerOptions { WriteIndented = true };

            if (opts.SortField != null)
            {
                if (!(string.Equals(opts.SortOrder, "Ascending", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(opts.SortOrder, "Descending", StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new ArgumentException(message: $"The sort order \"{opts.SortOrder}\" is invalid, please use Ascending/Descending.");
                }

                SortMetadata(ref postList, opts.SortField, opts.SortOrder);
            }

            var seriaizedMetadata = JsonSerializer.Serialize(postList, options);
            var fileName = $@"{opts.OutputPath + opts.FileName}.json";

            File.WriteAllText(fileName, seriaizedMetadata);
        }

        static dynamic CreateMetadataForMdFile(string filePath)
        {
            var flag = HeaderReadStatus.NotStarted;
            dynamic post = new ExpandoObject();

            using (var sr = new StreamReader(filePath))
            {
                string line;

                while ((line = sr.ReadLine()) != null && flag != HeaderReadStatus.Finished)
                {
                    if (line.Contains("---"))
                    {
                        flag = flag == HeaderReadStatus.NotStarted ? HeaderReadStatus.Reading : HeaderReadStatus.Finished;
                    }
                    else
                    {
                        var headerRow = line.Split(":");
                        ((IDictionary<string, object>)post).Add(headerRow[0].Trim(), headerRow[1].Trim());
                    }
                }

                //post.Body = sr.ReadToEnd().Trim();
            }

            return post;
        }

        private static void SortMetadata(ref List<dynamic> postList, string SortField, string SortOrder)
        {
            try
            {
                if (string.Equals(SortOrder, "Ascending", StringComparison.InvariantCultureIgnoreCase))
                {
                    postList = postList.OrderBy(post =>
                    {
                        var dic = post as IDictionary<string, object>;
                        return dic[SortField];
                    }).ToList();
                }
                else if (string.Equals(SortOrder, "Descending", StringComparison.InvariantCultureIgnoreCase))
                {
                    postList = postList.OrderByDescending(post =>
                    {
                        var dic = post as IDictionary<string, object>;
                        return dic[SortField];
                    }).ToList();
                }
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(message: $"The sort field \"{SortField}\" doesn't exists on the current object.");
            }
        }
    }
}
