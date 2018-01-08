﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace CodeObfuscation
{
    class CompilationManager
    {
        public CompilationManager() { }

        public void parseCompilationWithProvidedPriorities(Priority.En_Priority[] prioritiesTablo) 
        {
            Tuple<Compilation, int> compilationTuple;
            Array.Sort(prioritiesTablo);

            foreach (Priority.En_Priority priority in prioritiesTablo)
            {
                compilationTuple = CreateCompilation();
                SyntaxNode finalNode = null;

                foreach (SyntaxTree sourceTree in compilationTuple.Item1.SyntaxTrees)
                {
                    CSharpSyntaxRewriter rewriter = Priority.GetInstancePriorityBased(priority, compilationTuple.Item1.GetSemanticModel(sourceTree));
                    if (rewriter != null)
                    {
                        finalNode = rewriter.Visit(sourceTree.GetRoot());
                        if (finalNode != null)
                        {
                            File.WriteAllText(sourceTree.FilePath, finalNode.ToFullString());
                        }
                    }
                    else
                    {

                        continue;

                    }
                }
            }
        }
        private Tuple<Compilation, int> CreateCompilation()
        {
            List<SyntaxTree> sourceTrees = new List<SyntaxTree>();
            SharedContainer instance = SharedContainer.Instance;
            string[] allfiles = System.IO.Directory.GetFiles(instance.path, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                if(info.Extension == "cs")
                {
                    String programPath = info.FullName;
                    String destinationProgramPath = info.FullName;
                    String path
                    switch (instance.mode)
                    {
                        case 1: // In-place
                            programPath = info.FullName;
                            destinationProgramPath = info.FullName;
                            break;
                        case 2: // Random names + different dir
                            path = Path.Combine(instance.outputPath, info.Name + ".cs");
                            File.Copy(info.FullName, path);
                            programPath = path;
                            destinationProgramPath = path;
                            break;
                        case 3: // Different dir only
                            path = Path.Combine(instance.outputPath,instance.RandomFileName()+".cs");
                            File.Copy(info.FullName, path);
                            programPath = path;
                            destinationProgramPath = path;
                            break;
                    }
                    String programText = File.ReadAllText(programPath);
                    SyntaxTree programTree =
                                   CSharpSyntaxTree.ParseText(programText)
                                                   .WithFilePath(destinationProgramPath);
                }
            }

            MetadataReference mscorlib =
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            MetadataReference codeAnalysis =
                    MetadataReference.CreateFromFile(typeof(SyntaxTree).Assembly.Location);
            MetadataReference csharpCodeAnalysis =
                   MetadataReference.CreateFromFile(typeof(CSharpSyntaxTree).Assembly.Location);
            MetadataReference[] references = { mscorlib, codeAnalysis, csharpCodeAnalysis };

            return Tuple.Create(CSharpCompilation.Create("CodeObfuscation",
                                            sourceTrees.ToArray(),
                                            references,
                                            new CSharpCompilationOptions(
                                                    OutputKind.ConsoleApplication)) as Compilation, sourceTrees.Count());
        }
    }
}
