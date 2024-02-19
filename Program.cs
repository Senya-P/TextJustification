using System;
using System.IO;
using System.Collections.Generic;

namespace TextJuctification
{
    internal class Program
    {
        public class WordReader
        {
            StreamReader file;
            string word;
            int newLineChars = 0;
            char curChar;
            int symbol;
            public bool eof = false;
            public WordReader(StreamReader input)
            {
                file = input;
            }

            public string readWord()
            {
                word = "";
                do
                {
                    symbol = file.Read();
                    if (symbol == -1)
                    {
                        eof = true;
                        return word;
                    }

                    curChar = (char)symbol;
                    if (IsNotEmpty(curChar))
                    {
                        word += curChar;
                        newLineChars = 0;
                    }
                    else if (curChar == '\n')
                    {
                        newLineChars++;

                        if (newLineChars == 2)   // 1 and more than 2 paragraph delimiters are being ignored
                            return "\n";
                    }

                }
                while (IsNotEmpty(curChar));
                return word;
            }
            bool IsNotEmpty(char c)
            {
                if (c == ' ' || c == '\n' || c == '\t')
                    return false;
                return true;
            }
        }

        public class LineWriter
        {
            StreamWriter file;
            int width;
            int charCount = 1;
            List<string> line = new List<string>();
            bool highlight;
            public LineWriter(StreamWriter output, int lineWidth, bool highlight)
            {
                file = output;
                width = lineWidth;
                this.highlight = highlight;
            }
            public void WriteOneLine(string word)
            {

                if (line.Count == 0)
                {
                    if (word != "\n" && charCount == 0)     //exception for the very first line
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (highlight)
                                file.Write("<-");
                            file.Write('\n');
                        }
                    }
                    line.Add(word);
                    charCount = word.Length;
                }
                else
                {
                    if (word == "\n") 
                        PrintLine();
                    else
                    {
                        if (word.Length + 1 + charCount <= width)
                        {
                            charCount = charCount + word.Length + 1;
                            if (highlight)
                                line[line.Count - 1] += '.';
                            else line[line.Count - 1] += ' ';
                            line.Add(word);
                        }
                        else
                        {
                            if (line.Count > 1)
                                AddSpaces();
                            OnEnd();
                            line.Add(word);
                            charCount = word.Length;
                        }
                    }
                }
            }
            void AddSpaces()
            {
                int extraSpaces = (width - charCount) / (line.Count - 1);
                int remainingSpaces = (width - charCount) % (line.Count - 1);
                for (int i = 0; i < line.Count - 1; i++)
                {
                    for (int j = 0; j < extraSpaces; j++)
                    {
                        if (highlight)
                            line[i] += '.';
                        else line[i] += ' ';
                    }
                       

                    if (i < remainingSpaces)
                    {
                        if (highlight)
                            line[i] += '.';
                        else line[i] += ' ';
                    }
                        
                }
            }
            void PrintLine()
            {
                foreach (string w in line)
                {
                    file.Write(w);
                }
                line.Clear();
                charCount = 0;
            }
            public void OnEnd()
            {
                PrintLine();
                if (highlight)
                    file.Write("<-");
                file.Write('\n');
            }
        }
        static void ProcessFile(WordReader wr, LineWriter lw)
        {
            string word;
            while (!wr.eof)
            {
                word = wr.readWord();
                if (word != "")
                {
                    lw.WriteOneLine(word);
                }
            }
        }
        static void Main(string[] args)
        {
            string inputFileName;
            string outputFileName;
            int width;
            bool highlight = false;

            if (args.Length < 3)
            {
                Console.Write("Argument Error");
                return;
            }
            bool isNumeric = int.TryParse(args[args.Length - 1], out width);
            if (!isNumeric || width <= 0)
            {
                Console.Write("Argument Error");
                return;
            }
            outputFileName = args[args.Length - 2];
            int filename_args = 0;
            if (args[0] == "--highlight-spaces")
            {
                highlight = true;
                filename_args = 1;
                if (args.Length == 3)
                {
                    Console.Write("Argument Error");
                    return;
                }
            }
            using (var output = new StreamWriter(outputFileName, true))
            {
                var lineWriter = new LineWriter(output, width, highlight);
                for (int i = filename_args; i < args.Length - 2; i++)
                {
                    inputFileName = args[i];
                    try
                    {
                        using (var input = new StreamReader(inputFileName))
                        {
                            var wordReader = new WordReader(input);
                            ProcessFile(wordReader, lineWriter);
                        }
                    }
                    catch (IOException)
                    { }
                }
                lineWriter.OnEnd();
            }
        }
    }
}
// návrhové vzory: adaptor, wrapper
// ModelViewController