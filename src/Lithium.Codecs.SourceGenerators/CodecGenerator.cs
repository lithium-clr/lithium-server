using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Lithium.Codecs.SourceGenerators;

[Generator]
public sealed class CodecGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all classes marked with the [CodecAttribute] attribute
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxForAttribute(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static bool IsSyntaxForAttribute(SyntaxNode s) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    continue;

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                var fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName is "Lithium.Codecs.CodecAttribute")
                    return classDeclarationSyntax;
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty) return;

        var distinctClasses = classes.Where(c => c is not null).Distinct();

        foreach (var classSyntax in distinctClasses)
        {
            var semanticModel = compilation.GetSemanticModel(classSyntax!.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(classSyntax) is not INamedTypeSymbol classSymbol) continue;

            var sourceCode = GenerateCodecForClass(classSymbol);
            context.AddSource($"{classSymbol.Name}Codec.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private static string GenerateCodecForClass(INamedTypeSymbol classSymbol)
    {
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var className = classSymbol.Name;
        var fullTypeName = classSymbol.ToDisplayString();

        var properties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetMethod is not null && p.SetMethod is not null &&
                        p.DeclaredAccessibility is Accessibility.Public)
            .ToList();

        var sb = new StringBuilder();

        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System.Buffers;");
        sb.AppendLine("using Lithium.Codecs;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine($"public sealed partial class {className}Codec(ICodecRegistry registry) : ICodec<{fullTypeName}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public void Encode({fullTypeName} value, IBufferWriter<byte> writer)");
        sb.AppendLine("    {");
     
        foreach (var prop in properties)
        {
            var propType = prop.Type.ToDisplayString();
            sb.AppendLine($"        registry.Get<{propType}>().Encode(value.{prop.Name}, writer);");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    public {fullTypeName} Decode(ref SequenceReader<byte> reader)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var instance = new {fullTypeName}();");
        
        foreach (var prop in properties)
        {
            var propType = prop.Type.ToDisplayString();
            sb.AppendLine($"        instance.{prop.Name} = registry.Get<{propType}>().Decode(ref reader);");
        }

        sb.AppendLine();
        sb.AppendLine("        return instance;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("public static partial class GeneratedCodecsExtensions");
        sb.AppendLine("{");
        sb.AppendLine($"    public static IServiceCollection Add{className}Codec(this IServiceCollection services)");
        sb.AppendLine("    {");
        sb.AppendLine($"        services.AddSingleton<ICodec<{fullTypeName}>, {className}Codec>();");
        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}