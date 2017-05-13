using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileFormatDetection.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FileFormatDetection.Generator
{
    /*
    Reused https://github.com/KirillOsenkov/RoslynQuoter
    */

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
                var fileFormats = file.LoadFromJsonFile();
                GenerateCsFileRoslyn(directory, fileFormats);
            }
        }

        public static void GenerateCsFileRoslyn(string directory, List<FileFormat> fileFormats)
        {
            #region Local functions

            SyntaxList<UsingDirectiveSyntax> GenerateUsings()
            {
                /*
                using System.Linq;
                using System.Collections.Generic;
                using FileFormatDetection.Core;
                */

                return List(
                    new[]
                    {
                        UsingDirective(IdentifierName($"{nameof(System)}")),
                        UsingDirective(QualifiedName(IdentifierName($"{nameof(System)}"),
                            IdentifierName($"{nameof(System.Linq)}"))),
                        UsingDirective(QualifiedName(
                            QualifiedName(IdentifierName($"{nameof(System)}"),
                                IdentifierName($"{nameof(System.Collections)}")),
                            IdentifierName($"{nameof(System.Collections.Generic)}"))),
                        UsingDirective(QualifiedName(IdentifierName($"{nameof(FileFormatDetection)}"),
                            IdentifierName($"{nameof(Core)}")))
                    });
            }

            PropertyDeclarationSyntax GenerateProperty()
            {
                /*
                public List<FileFormat> FileFormats { get; }
                */

                return PropertyDeclaration(
                        GenericName(Identifier("List"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(IdentifierName($"{nameof(FileFormat)}")))),
                        Identifier("FileFormats"))
                    .WithModifiers(TokenList(Token(PublicKeyword)))
                    .WithAccessorList(AccessorList(SingletonList(AccessorDeclaration(GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SemicolonToken)))));
            }

            ConstructorDeclarationSyntax GenerateConstructor(string formatType)
            {
                /*
                public Audio(string formatsContent)
                {
                    FileFormats = FileFormatsLoader.LoadFromJsonContent(formatsContent);
                }
                */

                return ConstructorDeclaration(Identifier(formatType))
                    .WithModifiers(TokenList(Token(PublicKeyword)))
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList(
                                Parameter(
                                        Identifier("formatsContent"))
                                    .WithType(
                                        PredefinedType(
                                            Token(StringKeyword))))))
                    .WithBody(
                        Block(
                            SingletonList<StatementSyntax>(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SimpleAssignmentExpression,
                                        IdentifierName("FileFormats"),
                                        InvocationExpression(
                                                MemberAccessExpression(
                                                    SimpleMemberAccessExpression,
                                                    IdentifierName($"{nameof(FileFormatsLoader)}"),
                                                    IdentifierName($"{nameof(FileFormatsLoader.LoadFromJsonContent)}")))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SingletonSeparatedList(
                                                        Argument(IdentifierName(
                                                            "formatsContent"))))))))));
            }

            PropertyDeclarationSyntax GeneratePropertyDeclarationForFormat(string formatType, string fileFormat)
            {
                /*
                public FileFormat AudioMp3 => FileFormats.First(x => x.Name.Equals("Mp3", StringComparison.OrdinalIgnoreCase));
                */
                return PropertyDeclaration(
                        IdentifierName($"{nameof(FileFormat)}"),
                        Identifier($"{formatType}{fileFormat}"))
                    .WithModifiers(
                        TokenList(
                            Token(PublicKeyword)))
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            InvocationExpression(
                                    MemberAccessExpression(
                                        SimpleMemberAccessExpression,
                                        IdentifierName("FileFormats"),
                                        IdentifierName("First")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                SimpleLambdaExpression(
                                                    Parameter(Identifier("x")),
                                                    InvocationExpression(
                                                            MemberAccessExpression(
                                                                SimpleMemberAccessExpression,
                                                                MemberAccessExpression(
                                                                    SimpleMemberAccessExpression,
                                                                    IdentifierName(
                                                                        "x"),
                                                                    IdentifierName(
                                                                        "Name")),
                                                                IdentifierName(
                                                                    "Equals")))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                SeparatedList<
                                                                    ArgumentSyntax>(
                                                                    new
                                                                        SyntaxNodeOrToken
                                                                        []
                                                                        {
                                                                            Argument(LiteralExpression(StringLiteralExpression, Literal(fileFormat))),
                                                                            Token(CommaToken),
                                                                            Argument(MemberAccessExpression(SimpleMemberAccessExpression, IdentifierName("StringComparison"), IdentifierName("OrdinalIgnoreCase")))
                                                                        }))))))))))
                    .WithSemicolonToken(
                        Token(SemicolonToken));
            }

            #endregion

            var type = fileFormats.First().Type;

            var classBody = new List<MemberDeclarationSyntax>
            {
                GenerateProperty(),
                GenerateConstructor(type)
            };

            foreach (var fileFormat in fileFormats)
            {
                classBody.Add(GeneratePropertyDeclarationForFormat(fileFormat.Type, fileFormat.Name));
            }

            var classFile = CompilationUnit()
                .WithUsings(GenerateUsings())
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(QualifiedName(IdentifierName($"{nameof(FileFormatDetection)}"),
                                IdentifierName($"{nameof(FileFormatDetection.Signatures)}")))
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(
                                    ClassDeclaration(type)
                                        .WithModifiers(
                                            TokenList(
                                                Token(PublicKeyword)))
                                        .WithMembers(
                                            List(classBody))))))
                .NormalizeWhitespace();

            File.WriteAllText(Path.Combine(directory, type + ".cs"), classFile.ToFullString());
        }
    }
}