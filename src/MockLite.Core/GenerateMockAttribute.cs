using System;

namespace MockLite;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class GenerateMockAttribute(Type interfaceType) : Attribute {
    public Type Type { get; } = interfaceType;
}

// Optional config holder to reference external interfaces:
// [GenerateMock(typeof(IMyInterface))] public partial class Mocks {}
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GenerateMockAttribute<T> : Attribute { }
