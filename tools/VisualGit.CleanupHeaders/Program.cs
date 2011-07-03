// tools\VisualGit.CleanupHeaders\Program.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VisualGit.CleanupHeaders
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1 || !Directory.Exists(args[0]))
            {
                Console.Error.WriteLine("Provide the root of the project at the command line");
                return 1;
            }

            string absolutePath = Path.GetFullPath(args[0]);

            if (!absolutePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                absolutePath += Path.DirectorySeparatorChar;

            foreach (string filename in Directory.GetFiles(absolutePath, "*.cs", SearchOption.AllDirectories))
            {
                Debug.Assert(filename.StartsWith(absolutePath, StringComparison.OrdinalIgnoreCase), "Filename must start with absolute path");

                FixHeader(absolutePath, filename);
            }

            return 0;
        }

        private static void FixHeader(string absolutePath, string filename)
        {
            // Skip designer files.

            if (filename.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
                return;

            // Build replacement dictionary.

            var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Filename", filename.Substring(absolutePath.Length) },
                { "FullFilename", filename }
            };

            // Parse the file and extract the header and remainder.

            string content = File.ReadAllText(filename);

            // Find the seperation between the header and the body.

            string header;
            string body;

            var match = Regex.Match(content, "^(//.*?\n)(?!//)", RegexOptions.Singleline);

            if (match.Success)
            {
                header = match.Groups[1].Value;

                if (header.IndexOf("Copyright", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    header = null;
                    body = content;
                }
                else
                {
                    body = content.Substring(header.Length);
                }
            }
            else
            {
                header = null;
                body = content;
            }

            // Write the correct content.

            body = body.TrimStart('\r', '\n');

            string newContent = BuildNewHeader(replacements) + Environment.NewLine + body;

            if (content != newContent)
                File.WriteAllText(filename, newContent);
        }

        private static string BuildNewHeader(Dictionary<string, string> replacements)
        {
            var regex = new Regex("\\$(" + String.Join("|", replacements.Keys.Select(p => Regex.Escape(p))) + ")\\$");

            using (var stream = typeof(Program).Assembly.GetManifestResourceStream("VisualGit.CleanupHeaders.Header.txt"))
            using (var reader = new StreamReader(stream))
            {
                string header = reader.ReadToEnd();

                header = regex.Replace(header, m => replacements[m.Groups[1].Value]);

                return header.TrimEnd() + Environment.NewLine;
            }
        }
    }
}
