using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BbQ.MockLite.Generators;

[Generator]
public class InterfaceMockGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        var compilation = context.Compilation;
        var targets = DiscoverTargets(compilation);

        foreach (var iface in targets)
        {
            var source = GenerateMockSource(compilation, iface);
            var className = GetMockClassName(iface.Name);
            context.AddSource($"{className}.g.cs", source);
        }
    }

    private static IEnumerable<INamedTypeSymbol> DiscoverTargets(Compilation compilation)
    {
        var results = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var tree in compilation.SyntaxTrees)
        {
            var model = compilation.GetSemanticModel(tree, ignoreAccessibility: true);
            var root = tree.GetRoot();

            foreach (var ifaceDecl in root.DescendantNodes().OfType<InterfaceDeclarationSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(ifaceDecl) as INamedTypeSymbol;
                if (symbol is null) continue;

                if (symbol.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(GenerateMockAttribute)))
                    results.Add(symbol);
            }

            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var classSymbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (classSymbol is null) continue;

                foreach (var attr in classSymbol.GetAttributes())
                {
                    var name = attr.AttributeClass?.Name;
                    if (name is null) continue;

                    if (name == nameof(GenerateMockAttribute) && attr.ConstructorArguments.Length == 1)
                    {
                        if (attr.ConstructorArguments[0].Value is INamedTypeSymbol t && t.TypeKind == TypeKind.Interface)
                            results.Add(t);
                    }
                    else if (name.StartsWith(nameof(GenerateMockAttribute)) && attr.AttributeClass?.TypeArguments.Length == 1)
                    {
                        var t = attr.AttributeClass.TypeArguments[0] as INamedTypeSymbol;
                        if (t is { TypeKind: TypeKind.Interface })
                            results.Add(t);
                    }
                }
            }
        }

        return results;
    }

    private static string GetMockClassName(string ifaceName)
        => (ifaceName.Length > 1 && ifaceName[0] == 'I' && char.IsUpper(ifaceName[1]))
            ? $"Mock{ifaceName.Substring(1)}"
            : $"Mock{ifaceName}";

    /// <summary>
    /// Collects all methods from the interface and its entire inheritance hierarchy,
    /// deduplicating by signature hash to handle diamond inheritance correctly.
    /// </summary>
    private static List<IMethodSymbol> GetAllMethods(INamedTypeSymbol iface)
    {
        var seen = new HashSet<string>();
        var result = new List<IMethodSymbol>();

        foreach (var m in iface.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary))
        {
            var hash = SignatureHash(m);
            if (seen.Add(hash))
                result.Add(m);
        }

        foreach (var baseIface in iface.AllInterfaces)
        {
            foreach (var m in baseIface.GetMembers().OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Ordinary))
            {
                var hash = SignatureHash(m);
                if (seen.Add(hash))
                    result.Add(m);
            }
        }

        return result;
    }

    /// <summary>
    /// Collects all properties from the interface and its entire inheritance hierarchy,
    /// deduplicating by name to handle diamond inheritance correctly.
    /// </summary>
    private static List<IPropertySymbol> GetAllProperties(INamedTypeSymbol iface)
    {
        var seen = new HashSet<string>();
        var result = new List<IPropertySymbol>();

        foreach (var p in iface.GetMembers().OfType<IPropertySymbol>())
        {
            if (seen.Add(p.Name))
                result.Add(p);
        }

        foreach (var baseIface in iface.AllInterfaces)
        {
            foreach (var p in baseIface.GetMembers().OfType<IPropertySymbol>())
            {
                if (seen.Add(p.Name))
                    result.Add(p);
            }
        }

        return result;
    }

    private static string GenerateMockSource(Compilation compilation, INamedTypeSymbol iface)
    {
        var ns = iface.ContainingNamespace.IsGlobalNamespace ? null : iface.ContainingNamespace.ToDisplayString();
        var className = GetMockClassName(iface.Name);
        var ifaceDisplay = iface.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using System.Reflection;");
        sb.AppendLine("using BbQ.MockLite;");

        if (!string.IsNullOrEmpty(ns)) sb.AppendLine($"namespace {ns} {{");
        sb.AppendLine("[GeneratedMock]");
        sb.AppendLine($"public sealed class {className} : {ifaceDisplay}");
        sb.AppendLine("{");
        sb.AppendLine("    public List<Invocation> Invocations { get; } = new();");

        // Collect all methods and properties from the interface hierarchy (composite interface support).
        var methods = GetAllMethods(iface);
        var properties = GetAllProperties(iface);

        // Behavior fields per method overload with signature hash
        // Skip generic methods — their type parameters are not in scope at class level.
        foreach (var m in methods)
        {
            if (m.IsGenericMethod) continue;
            var field = BehaviorFieldName(m);
            var delType = BehaviorDelegateType(m);
            sb.AppendLine($"    public {delType}? {field} {{ get; set; }}");
        }

        // Static cached MethodInfo fields for methods (avoids per-call GetMethod reflection).
        foreach (var m in methods)
            sb.Append(EmitMethodInfoField(m));

        // Property behavior fields
        foreach (var p in properties)
        {
            if (p.GetMethod is not null)
                sb.AppendLine($"    public Func<{TypeDisplay(p.Type)}>? {GetBehaviorFieldName(p)} {{ get; set; }}");
            if (p.SetMethod is not null)
                sb.AppendLine($"    public Action<{TypeDisplay(p.Type)}>? {SetBehaviorFieldName(p)} {{ get; set; }}");
            sb.AppendLine($"    private {TypeDisplay(p.Type)}? _{p.Name};");
        }

        // Static cached MethodInfo fields for property accessors.
        foreach (var p in properties)
        {
            if (p.GetMethod is not null)
                sb.AppendLine($"    private static readonly MethodInfo {PropertyGetMethodInfoFieldName(p)} = typeof({TypeDisplay(p.ContainingType)}).GetProperty(\"{p.Name}\")!.GetGetMethod()!;");
            if (p.SetMethod is not null)
                sb.AppendLine($"    private static readonly MethodInfo {PropertySetMethodInfoFieldName(p)} = typeof({TypeDisplay(p.ContainingType)}).GetProperty(\"{p.Name}\")!.GetSetMethod()!;");
        }

        // Implement methods
        foreach (var m in methods)
        {
            sb.Append(EmitMethodImplementation(m));
            // Generic methods cannot have class-level typed behavior fields, so skip
            // Setup/SetupWithMatcher/Returns helpers.  Verify by method name still works.
            if (!m.IsGenericMethod)
            {
                sb.Append(EmitMethodSetup(m, className));
                // Skip matcher overloads for parameterless methods (signatures would be identical).
                if (m.Parameters.Length > 0)
                    sb.Append(EmitMethodSetupWithMatcher(m, className));
                sb.Append(EmitMethodReturns(m, className));
            }
            sb.Append(EmitMethodVerify(m));
            if (m.Parameters.Length > 0 && !m.IsGenericMethod)
                sb.Append(EmitMethodVerifyWithMatcher(m));
        }

        // Implement properties
        foreach (var p in properties)
        {
            sb.Append(EmitPropertyImplementation(p));
            if (p.GetMethod is not null)
            {
                sb.Append(EmitPropertyGetSetup(p, className));
                sb.Append(EmitPropertyGetPhraseStruct(p, className));
                sb.Append(EmitPropertyGetVerify(p));
            }
            if (p.SetMethod is not null)
            {
                sb.Append(EmitPropertySetSetup(p, className));
                sb.Append(EmitPropertySetSetupWithMatcher(p, className));
                sb.Append(EmitPropertySetPhraseStruct(p, className));
                sb.Append(EmitPropertySetVerify(p));
                sb.Append(EmitPropertySetVerifyWithMatcher(p));
            }
        }

        sb.AppendLine("    private static string BuildMessage(string member, Func<int, bool> times, int actual, string? message)");
        sb.AppendLine("    {");
        sb.AppendLine("        var expected = times.Method.Name;");
        sb.AppendLine("        var line = $\"Verification failed for {member}. Expected: {expected}. Actual calls: {actual}.\";");
        sb.AppendLine("        return string.IsNullOrWhiteSpace(message) ? line : $\"{line} {message}\";");
        sb.AppendLine("    }");

        sb.AppendLine("}"); // class
        if (!string.IsNullOrEmpty(ns)) sb.AppendLine("}"); // namespace

        // Emit a [ModuleInitializer] registrar for non-generic interfaces so that
        // Mock.Of<T>() can resolve the generated type via MockTypeRegistry (O(1) lookup)
        // instead of using Type.GetType on every call.
        // Generic interfaces are skipped because open-generic types cannot be registered.
        if (iface.TypeParameters.Length == 0)
        {
            var fullyQualifiedIface = iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var fullyQualifiedMock = string.IsNullOrEmpty(ns)
                ? $"global::{className}"
                : $"global::{ns}.{className}";

            sb.AppendLine($"internal static class {className}_Registrar");
            sb.AppendLine("{");
            sb.AppendLine("    [System.Runtime.CompilerServices.ModuleInitializer]");
            sb.AppendLine($"    internal static void Register()");
            sb.AppendLine($"        => global::BbQ.MockLite.MockTypeRegistry.Register<{fullyQualifiedIface}, {fullyQualifiedMock}>();");
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    private static string TypeDisplay(ITypeSymbol t)
        => t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    // Signature hash to disambiguate overloads and generics
    private static string SignatureHash(IMethodSymbol m)
    {
        var typeParams = m.IsGenericMethod
            ? "<" + string.Join(",", m.TypeParameters.Select(tp => tp.Name)) + ">"
            : "";
        var args = string.Join(",", m.Parameters.Select(p => TypeDisplay(p.Type)));
        return $"{m.Name}{typeParams}({args})";
    }

    private static string BehaviorFieldName(IMethodSymbol m)
    {
        var hash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SignatureHash(m)))
            .TrimEnd('=')
            .Replace('+', '_')
            .Replace('/', '-');
        return $"{m.Name}_{hash}_Behavior";
    }

    private static string BehaviorDelegateType(IMethodSymbol m)
    {
        var returns = TypeDisplay(m.ReturnType);
        var parms = m.Parameters.Select(p => TypeDisplay(p.Type)).ToList();

        if (returns == "void")
            return parms.Count == 0 ? "Action" : $"Action<{string.Join(", ", parms)}>";

        if (returns == "Task")
            return parms.Count == 0 ? "Func<Task>" : $"Func<{string.Join(", ", parms)}, Task>";

        if (returns.StartsWith("Task<"))
        {
            var genericRet = returns.Substring(5, returns.Length - 6);
            parms.Add(returns); // Use the full Task<T> type
            return $"Func<{string.Join(", ", parms)}>";
        }

        if (returns == "ValueTask")
            return parms.Count == 0 ? "Func<ValueTask>" : $"Func<{string.Join(", ", parms)}, ValueTask>";

        if (returns.StartsWith("ValueTask<"))
        {
            var genericRet = returns.Substring(10, returns.Length - 11);
            parms.Add(returns); // Use the full ValueTask<T> type
            return $"Func<{string.Join(", ", parms)}>";
        }

        parms.Add(returns);
        return $"Func<{string.Join(", ", parms)}>";
    }

    private static string EmitMethodImplementation(IMethodSymbol m)
    {
        var name = m.Name;
        var parmsSig = string.Join(", ", m.Parameters.Select(p => $"{TypeDisplay(p.Type)} {p.Name}"));
        var ret = TypeDisplay(m.ReturnType);
        var miField = MethodInfoFieldName(m);
        var sb = new StringBuilder();

        // For the object[] in Invocations.Add, suppress nullable warnings for
        // parameters whose type involves a type parameter (they could be null).
        var invocationArgs = string.Join(", ", m.Parameters.Select(p =>
            ContainsTypeParameter(p.Type) ? $"{p.Name}!" : p.Name));
        // For behavior field invocation, use plain parameter names.
        var behaviorArgs = string.Join(", ", m.Parameters.Select(p => p.Name));

        // Build type parameter clause + constraints for generic methods.
        var typeParamClause = "";
        var constraintsClauses = "";
        if (m.IsGenericMethod)
        {
            typeParamClause = "<" + string.Join(", ", m.TypeParameters.Select(tp => tp.Name)) + ">";
            var constraints = new List<string>();
            foreach (var tp in m.TypeParameters)
            {
                var parts = new List<string>();
                if (tp.HasReferenceTypeConstraint) parts.Add("class");
                if (tp.HasValueTypeConstraint) parts.Add("struct");
                if (tp.HasUnmanagedTypeConstraint) parts.Add("unmanaged");
                if (tp.HasNotNullConstraint) parts.Add("notnull");
                foreach (var ct in tp.ConstraintTypes)
                    parts.Add(TypeDisplay(ct));
                if (tp.HasConstructorConstraint) parts.Add("new()");
                if (parts.Count > 0)
                    constraints.Add($" where {tp.Name} : {string.Join(", ", parts)}");
            }
            constraintsClauses = string.Join("", constraints);
        }

        sb.AppendLine($"    public {ret} {name}{typeParamClause}({parmsSig}){constraintsClauses}");
        sb.AppendLine("    {");
        sb.AppendLine($"        Invocations.Add(new Invocation({miField}, new object[] {{ {invocationArgs} }}));");

        // Generic methods have no class-level behavior field; just return smart defaults.
        if (m.IsGenericMethod)
        {
            if (ret == "void")
            {
                // nothing to return
            }
            else if (ret == "Task")
            {
                sb.AppendLine("        return Task.CompletedTask;");
            }
            else if (ret.StartsWith("Task<"))
            {
                var innerType = ((INamedTypeSymbol)m.ReturnType).TypeArguments[0];
                var innerDisplay = TypeDisplay(innerType);
                var smartDef = SmartDefault(innerType);
                sb.AppendLine($"        return Task.FromResult<{innerDisplay}>({smartDef});");
            }
            else if (ret == "ValueTask")
            {
                sb.AppendLine("        return default;");
            }
            else if (ret.StartsWith("ValueTask<"))
            {
                var innerType = ((INamedTypeSymbol)m.ReturnType).TypeArguments[0];
                var smartDef = SmartDefault(innerType);
                sb.AppendLine($"        return new ValueTask<{TypeDisplay(innerType)}>({smartDef});");
            }
            else
            {
                var smartDef = SmartDefault(m.ReturnType);
                sb.AppendLine($"        return {smartDef};");
            }
        }
        else
        {
            var field = BehaviorFieldName(m);
            if (ret == "void")
            {
                sb.AppendLine($"        {field}?.Invoke({behaviorArgs});");
            }
            else if (ret == "Task")
            {
                sb.AppendLine($"        return {field}?.Invoke({behaviorArgs}) ?? Task.CompletedTask;");
            }
            else if (ret.StartsWith("Task<"))
            {
                var innerType = ((INamedTypeSymbol)m.ReturnType).TypeArguments[0];
                var innerDisplay = TypeDisplay(innerType);
                var smartDef = SmartDefault(innerType);
                sb.AppendLine($"        if ({field} != null) return {field}({behaviorArgs});");
                sb.AppendLine($"        return Task.FromResult<{innerDisplay}>({smartDef});");
            }
            else if (ret == "ValueTask")
            {
                sb.AppendLine($"        return {field}?.Invoke({behaviorArgs}) ?? default;");
            }
            else if (ret.StartsWith("ValueTask<"))
            {
                var innerType = ((INamedTypeSymbol)m.ReturnType).TypeArguments[0];
                var smartDef = SmartDefault(innerType);
                sb.AppendLine($"        if ({field} != null) return {field}({behaviorArgs});");
                sb.AppendLine($"        return new ValueTask<{TypeDisplay(innerType)}>({smartDef});");
            }
            else
            {
                var smartDef = SmartDefault(m.ReturnType);
                sb.AppendLine($"        return {field}?.Invoke({behaviorArgs}) ?? {smartDef};");
            }
        }

        sb.AppendLine("    }");
        return sb.ToString();
    }

    private static string EmitMethodSetup(IMethodSymbol m, string className)
    {
        var field = BehaviorFieldName(m);
        var behaviorType = BehaviorDelegateType(m);
        return $"    public {className} Setup{m.Name}({behaviorType} behavior) {{ {field} = behavior; return this; }}\n";
    }

    private static string EmitMethodSetupWithMatcher(IMethodSymbol m, string className)
    {
        var field = BehaviorFieldName(m);
        var ret = TypeDisplay(m.ReturnType);
        var args = string.Join(", ", m.Parameters.Select(p => p.Name));
        var matcherSig = string.Join(", ", m.Parameters.Select(p => $"Func<{TypeDisplay(p.Type)}, bool> {p.Name}Matcher"));
        var conj = m.Parameters.Length == 0 ? "true" : string.Join(" && ", m.Parameters.Select(p => $"{p.Name}Matcher({p.Name})"));

        var sb = new StringBuilder();
        sb.AppendLine($"    public {className} Setup{m.Name}({matcherSig}{(matcherSig.Length > 0 ? ", " : "")} {BehaviorDelegateType(m)} behavior)");
        sb.AppendLine("    {");
        if (ret == "void")
        {
            sb.AppendLine($"        {field} = ({args}) => {{ if ({conj}) behavior({args}); }};");
        }
        else if (ret == "Task" || ret.StartsWith("Task<") || ret == "ValueTask" || ret.StartsWith("ValueTask<"))
        {
            // For async methods, the behavior already returns the correct async type
            sb.AppendLine($"        {field} = ({args}) => ({conj}) ? behavior({args}) : GetDefault{m.Name}();");
            sb.AppendLine("        return this;");
            sb.AppendLine("    }");
            sb.AppendLine();
            // Add helper to generate default async result using smart defaults
            string def;
            if (ret == "Task")
            {
                def = "Task.CompletedTask";
            }
            else if (ret.StartsWith("Task<") && m.ReturnType is INamedTypeSymbol taskNamed)
            {
                var innerDefault = SmartDefault(taskNamed.TypeArguments[0]);
                def = $"Task.FromResult<{TypeDisplay(taskNamed.TypeArguments[0])}>({innerDefault})";
            }
            else if (ret == "ValueTask")
            {
                def = "default";
            }
            else if (ret.StartsWith("ValueTask<") && m.ReturnType is INamedTypeSymbol vtNamed)
            {
                var innerDefault = SmartDefault(vtNamed.TypeArguments[0]);
                def = $"new ValueTask<{TypeDisplay(vtNamed.TypeArguments[0])}>({innerDefault})";
            }
            else
            {
                def = SmartDefault(m.ReturnType);
            }
            sb.AppendLine($"    private static {ret} GetDefault{m.Name}() => {def};");
            return sb.ToString();
        }
        else
        {
            var def = SmartDefault(m.ReturnType);
            sb.AppendLine($"        {field} = ({args}) => ({conj}) ? behavior({args}) : {def};");
        }
        sb.AppendLine($"        return this;");
        sb.AppendLine("    }");
        return sb.ToString();
    }

    private static string EmitMethodReturns(IMethodSymbol m, string className)
    {
        var field = BehaviorFieldName(m);
        var ret = TypeDisplay(m.ReturnType);
        var args = string.Join(", ", m.Parameters.Select(p => p.Name));
        var sb = new StringBuilder();

        if (ret.StartsWith("Task<"))
        {
            var tArg = ret.Substring(5, ret.Length - 6);
            sb.AppendLine($"    public {className} Returns{m.Name}({tArg} result) {{ {field} = ({args}) => Task.FromResult(result); return this; }}");
        }
        else if (ret == "Task")
        {
            sb.AppendLine($"    public {className} Returns{m.Name}() {{ {field} = ({args}) => Task.CompletedTask; return this; }}");
        }
        else if (ret.StartsWith("ValueTask<"))
        {
            var tArg = ret.Substring(10, ret.Length - 11);
            sb.AppendLine($"    public {className} Returns{m.Name}({tArg} result) {{ {field} = ({args}) => new ValueTask<{tArg}>(result); return this; }}");
        }
        else if (ret == "ValueTask")
        {
            sb.AppendLine($"    public {className} Returns{m.Name}() {{ {field} = ({args}) => default; return this; }}");
        }
        else if (ret != "void")
        {
            sb.AppendLine($"    public {className} Returns{m.Name}({ret} result) {{ {field} = ({args}) => result; return this; }}");
        }
        return sb.ToString();
    }

    private static string EmitMethodVerify(IMethodSymbol m)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"    public void Verify{m.Name}(Func<int, bool> times, string? message = null)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var count = Invocations.Count(i => i.Method.Name == nameof({m.Name}));");
        sb.AppendLine($"        if (!times(count)) throw new VerificationException(BuildMessage(nameof({m.Name}), times, count, message));");
        sb.AppendLine("    }");
        return sb.ToString();
    }

    private static string EmitMethodVerifyWithMatcher(IMethodSymbol m)
    {
        var matcherSig = string.Join(", ", m.Parameters.Select(p => $"Func<{TypeDisplay(p.Type)}, bool> {p.Name}Matcher"));
        var sb = new StringBuilder();
        sb.AppendLine($"    public void Verify{m.Name}({matcherSig}{(matcherSig.Length > 0 ? ", " : "")}Func<int, bool> times, string? message = null)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var count = Invocations.Count(i => i.Method.Name == nameof({m.Name}) && {BuildArgMatcherPredicate(m)});");
        sb.AppendLine($"        if (!times(count)) throw new VerificationException(BuildMessage(nameof({m.Name}), times, count, message));");
        sb.AppendLine("    }");
        return sb.ToString();
    }

    private static string BuildArgMatcherPredicate(IMethodSymbol m)
        => m.Parameters.Length == 0 ? "true"
           : string.Join(" && ", m.Parameters.Select((p, idx) => $"{p.Name}Matcher(({TypeDisplay(p.Type)})i.Arguments[{idx}])"));

    private static string EmitPropertyImplementation(IPropertySymbol p)
    {
        var name = p.Name;
        var type = TypeDisplay(p.Type);
        var sb = new StringBuilder();

        sb.AppendLine($"    public {type} {name}");
        sb.AppendLine("    {");
        if (p.GetMethod is not null)
        {
            sb.AppendLine("        get");
            sb.AppendLine("        {");
            sb.AppendLine($"            Invocations.Add(new Invocation({PropertyGetMethodInfoFieldName(p)}, Array.Empty<object>()));");
            sb.AppendLine($"            if ({GetBehaviorFieldName(p)} is not null) return {GetBehaviorFieldName(p)}();");
            sb.AppendLine($"            return _{name}!;");
            sb.AppendLine("        }");
        }
        if (p.SetMethod is not null)
        {
            sb.AppendLine("        set");
            sb.AppendLine("        {");
            sb.AppendLine($"            Invocations.Add(new Invocation({PropertySetMethodInfoFieldName(p)}, new object[] {{ value }}));");
            sb.AppendLine($"            if ({SetBehaviorFieldName(p)} is not null) {{ {SetBehaviorFieldName(p)}(value); }} else _{name} = value;");
            sb.AppendLine("        }");
        }
        sb.AppendLine("    }");

        return sb.ToString();
    }

    private static string EmitPropertyGetSetup(IPropertySymbol p, string className)
    {
        var type = TypeDisplay(p.Type);
        var sb = new StringBuilder();
        sb.AppendLine($"    public {className} SetupGet{p.Name}(Func<{type}> behavior) {{ {GetBehaviorFieldName(p)} = behavior; return this; }}");
        sb.AppendLine($"    public {className} ReturnsGet{p.Name}({type} value) {{ {GetBehaviorFieldName(p)} = () => value; return this; }}");
        return sb.ToString();
    }

    private static string EmitPropertyGetVerify(IPropertySymbol p)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"    public void VerifyGet{p.Name}(Func<int, bool> times, string? message = null)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var count = Invocations.Count(i => i.Method.Name == \"get_{p.Name}\");");
        sb.AppendLine($"        if (!times(count)) throw new VerificationException(BuildMessage(\"get_{p.Name}\", times, count, message));");
        sb.AppendLine("    }");
        return sb.ToString();
    }

    private static string EmitPropertySetSetup(IPropertySymbol p, string className)
    {
        var type = TypeDisplay(p.Type);
        var sb = new StringBuilder();
        sb.AppendLine($"    public {className} SetupSet{p.Name}(Action<{type}> behavior) {{ {SetBehaviorFieldName(p)} = behavior; return this; }}");
        return sb.ToString();
    }

    private static string EmitPropertySetSetupWithMatcher(IPropertySymbol p, string className)
    {
        var type = TypeDisplay(p.Type);
        var sb = new StringBuilder();
        sb.AppendLine($"    public {className} SetupSet{p.Name}(Func<{type}, bool> matcher, Action<{type}> behavior)");
        sb.AppendLine("    {");
        sb.AppendLine($"        {SetBehaviorFieldName(p)} = v => {{ if (matcher(v)) behavior(v); }};");
        sb.AppendLine("        return this;");
        sb.AppendLine("    }");
        return sb.ToString();
    }

    private static string EmitPropertySetVerify(IPropertySymbol p)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"    public void VerifySet{p.Name}(Func<int, bool> times, string? message = null)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var count = Invocations.Count(i => i.Method.Name == \"set_{p.Name}\");");
        sb.AppendLine($"        if (!times(count)) throw new VerificationException(BuildMessage(\"set_{p.Name}\", times, count, message));");
        sb.AppendLine("    }");
        return sb.ToString();
    }

    private static string EmitPropertySetVerifyWithMatcher(IPropertySymbol p)
    {
        var type = TypeDisplay(p.Type);
        var sb = new StringBuilder();
        sb.AppendLine($"    public void VerifySet{p.Name}(Func<{type}, bool> matcher, Func<int, bool> times, string? message = null)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var count = Invocations.Count(i => i.Method.Name == \"set_{p.Name}\" && matcher(({type})i.Arguments[0]));");
        sb.AppendLine($"        if (!times(count)) throw new VerificationException(BuildMessage(\"set_{p.Name}\", times, count, message));");
        sb.AppendLine("    }");
        return sb.ToString();
    }

    private static string GetBehaviorFieldName(IPropertySymbol p) => $"{p.Name}GetBehavior";
    private static string SetBehaviorFieldName(IPropertySymbol p) => $"{p.Name}SetBehavior";

    // --- Phrase-returning property setup methods (generated equivalents of GetSetupPhrase / SetSetupPhrase) ---

    private static string EmitPropertyGetPhraseStruct(IPropertySymbol p, string className)
    {
        var type = TypeDisplay(p.Type);
        var structName = $"GetSetupPhrase_{p.Name}";
        var sb = new StringBuilder();
        sb.AppendLine($"    public readonly struct {structName}");
        sb.AppendLine("    {");
        sb.AppendLine($"        private readonly {className} _mock;");
        sb.AppendLine($"        internal {structName}({className} mock) => _mock = mock;");
        sb.AppendLine($"        public {className} Returns({type} value) {{ _mock.{GetBehaviorFieldName(p)} = () => value; return _mock; }}");
        sb.AppendLine($"        public {className} Returns(Func<{type}> factory) {{ _mock.{GetBehaviorFieldName(p)} = factory; return _mock; }}");
        sb.AppendLine($"        public {className} Throws(Exception ex) {{ _mock.{GetBehaviorFieldName(p)} = () => throw ex; return _mock; }}");
        sb.AppendLine("    }");
        sb.AppendLine($"    public {structName} SetupGet{p.Name}() => new {structName}(this);");
        return sb.ToString();
    }

    private static string EmitPropertySetPhraseStruct(IPropertySymbol p, string className)
    {
        var type = TypeDisplay(p.Type);
        var structName = $"SetSetupPhrase_{p.Name}";
        var sb = new StringBuilder();
        sb.AppendLine($"    public readonly struct {structName}");
        sb.AppendLine("    {");
        sb.AppendLine($"        private readonly {className} _mock;");
        sb.AppendLine($"        internal {structName}({className} mock) => _mock = mock;");
        sb.AppendLine($"        public {className} Throws(Exception ex) {{ _mock.{SetBehaviorFieldName(p)} = _ => throw ex; return _mock; }}");
        sb.AppendLine($"        public {structName} Callback(Action callback) {{ _mock.{SetBehaviorFieldName(p)} = _ => callback(); return this; }}");
        sb.AppendLine($"        public {structName} Callback(Action<{type}> callback) {{ _mock.{SetBehaviorFieldName(p)} = callback; return this; }}");
        sb.AppendLine("    }");
        sb.AppendLine($"    public {structName} SetupSet{p.Name}() => new {structName}(this);");
        return sb.ToString();
    }

    /// <summary>
    /// Returns the name of the static MethodInfo field for a method (used to avoid
    /// per-call GetMethod reflection in generated mock implementations).
    /// </summary>
    private static string MethodInfoFieldName(IMethodSymbol m)
        => $"_mi_{BehaviorFieldName(m).Replace("_Behavior", "")}";

    /// <summary>
    /// Emits the static readonly MethodInfo field declaration for a method.
    /// When all parameter types are concrete (no open generics), the field is
    /// initialised with a type-safe overload of GetMethod to correctly disambiguate
    /// overloads.  Generic methods fall back to the name-only overload.
    /// </summary>
    private static string EmitMethodInfoField(IMethodSymbol m)
    {
        var iface = TypeDisplay(m.ContainingType);
        var fieldName = MethodInfoFieldName(m);

        // Determine whether every parameter type is a closed / concrete type that can
        // be expressed as typeof(…) in a static context.
        bool hasOpenTypeParam = m.ContainingType.TypeParameters.Length > 0
            || m.IsGenericMethod
            || m.Parameters.Any(p => ContainsTypeParameter(p.Type));

        if (!hasOpenTypeParam)
        {
            var types = m.Parameters.Length == 0
                ? "Array.Empty<Type>()"
                : $"new Type[] {{ {string.Join(", ", m.Parameters.Select(p => $"typeof({TypeDisplay(p.Type)})"))} }}";
            return $"    private static readonly MethodInfo {fieldName} = typeof({iface}).GetMethod(\"{m.Name}\", {types})!;\n";
        }

        // For generic methods, filter by generic arity to avoid AmbiguousMatchException
        // when same-named overloads exist with different arities.
        if (m.IsGenericMethod)
        {
            var arity = m.TypeParameters.Length;
            var paramCount = m.Parameters.Length;
            return $"    private static readonly MethodInfo {fieldName} = typeof({iface}).GetMethods().First(m => m.Name == \"{m.Name}\" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == {arity} && m.GetParameters().Length == {paramCount})!;\n";
        }

        // Generic interface – fall back to name-only GetMethod.
        return $"    private static readonly MethodInfo {fieldName} = typeof({iface}).GetMethod(\"{m.Name}\")!;\n";
    }

    private static string PropertyGetMethodInfoFieldName(IPropertySymbol p) => $"_mi_get_{p.Name}";
    private static string PropertySetMethodInfoFieldName(IPropertySymbol p) => $"_mi_set_{p.Name}";

    /// <summary>
    /// Returns true if the given type symbol contains an open type parameter
    /// (either at the top level or within generic arguments).
    /// </summary>
    private static bool ContainsTypeParameter(ITypeSymbol type)
    {
        if (type is ITypeParameterSymbol) return true;
        if (type is INamedTypeSymbol named)
            return named.TypeArguments.Any(ContainsTypeParameter);
        if (type is IArrayTypeSymbol arr)
            return ContainsTypeParameter(arr.ElementType);
        return false;
    }

    /// <summary>
    /// Returns a C# expression string that produces a smart default for the given type.
    /// For collection interfaces (IEnumerable&lt;T&gt;, IList&lt;T&gt;, IReadOnlyList&lt;T&gt;, etc.)
    /// returns <c>Array.Empty&lt;T&gt;()</c>; otherwise falls back to <c>default(type)!</c>.
    /// </summary>
    private static string SmartDefault(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named && named.IsGenericType && named.TypeArguments.Length == 1)
        {
            var defName = named.OriginalDefinition.ToDisplayString();
            if (defName is "System.Collections.Generic.IEnumerable<T>"
                       or "System.Collections.Generic.IReadOnlyCollection<T>"
                       or "System.Collections.Generic.IReadOnlyList<T>"
                       or "System.Collections.Generic.ICollection<T>"
                       or "System.Collections.Generic.IList<T>")
            {
                var elem = TypeDisplay(named.TypeArguments[0]);
                return $"Array.Empty<{elem}>()";
            }
        }
        return $"default({TypeDisplay(type)})!";
    }
}