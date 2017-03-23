using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileFormatDetection.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace FileFormatDetection.Generator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GenerateCsFiles(args[0]);
        }

        public static void GenerateCsFiles(string directory)
        {
            var files = Directory.GetFiles(directory, "*.Signatures.json");

            foreach (var file in files)
            {
                var fileFormats = FileFormatsLoader.LoadFromJsonFile(file);
                GenerateCsFile(directory, fileFormats);
            }
        }

        public static void GenerateCsFile(string directory, List<FileFormat> fileFormats)
        {
            var type = fileFormats.First().Type;

            var lines = new List<string>();

            foreach (var file in fileFormats)
            {
                lines.Add($"public FileFormat {type}{file.Name} => FileFormats.First(x => x.Name.Equals(\"{file.Name}\", StringComparison.OrdinalIgnoreCase));");
            }

            var stringLines = lines.Aggregate((f, s) => $"{f}{Environment.NewLine}{Environment.NewLine}        {s}");

            var classToWrite = $@"using System;
using System.Linq;
using System.Collections.Generic;
using FileFormatDetection.Core;

namespace FileFormatDetection.Signatures
{{
    public class {type}
    {{
        public List <FileFormat> FileFormats {{ get; }}

        public {type}(string formatsContent)
        {{
            FileFormats = FileFormatsLoader.LoadFromJsonContent(formatsContent);
        }}

        {stringLines}
    }}
}}";

            File.WriteAllText(Path.Combine(directory, type + ".cs"), classToWrite);
        }


        public static void GenerateCsFileRoslyn(string directory, List<FileFormat> fileFormats)
        {
            var type = fileFormats.First().Type;

            var pds = new List<MemberDeclarationSyntax>();

            foreach (var file in fileFormats)
            {
                var pd = SF.PropertyDeclaration(SF.ParseTypeName("FileFormat"), $"{type}{file.Name}")
                    .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .AddBodyStatements(SF.ParseStatement($"return FileFormats.First(x => x.Name.Equals(\"{file.Name}\", StringComparison.OrdinalIgnoreCase));"))
                    )
                    .NormalizeWhitespace();

                pds.Add(pd);
            }

            var classFile = SF.CompilationUnit()
                .AddUsings(
                    //SyntaxFactory.UsingDirective(SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("System"), SyntaxFactory.IdentifierName("Linq")))
                    SF.UsingDirective(SF.IdentifierName("System")).NormalizeWhitespace(),
                    SF.UsingDirective(SF.IdentifierName("System.Linq")).NormalizeWhitespace(),
                    SF.UsingDirective(SF.IdentifierName("System.Collections.Generic")).NormalizeWhitespace(),
                    SF.UsingDirective(SF.IdentifierName($"{nameof(FileFormatDetection)}.{nameof(FileFormatDetection.Core)}")).NormalizeWhitespace()
                ).NormalizeWhitespace()
                .AddMembers(SF.NamespaceDeclaration(SF.IdentifierName($"FileFormatDetection.Signatures")).NormalizeWhitespace()
                    .AddMembers(SF.ClassDeclaration(type).AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                        .AddMembers(
                            SF.PropertyDeclaration(SF.ParseTypeName("List<FileFormat>"), "FileFormats")
                                .AddAccessorListAccessors(SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)))
                                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                                .NormalizeWhitespace(),
                            SF.ConstructorDeclaration(type).AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                                .AddParameterListParameters(SF.Parameter(SF.Identifier("formatsContent")).WithType(SF.ParseTypeName("string")))
                                .AddBodyStatements(SF.ParseStatement("FileFormats = FileFormatsLoader.LoadFromJsonContent(formatsContent);"))
                        )
                        .AddMembers(pds.ToArray()).NormalizeWhitespace()
                    ));

            File.WriteAllText(Path.Combine(directory, type + ".cs"), classFile.ToFullString());
        }
    }
}