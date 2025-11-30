using System;

namespace BbQ.MockLite;

/// <summary>
/// Marks a class as an automatically generated mock implementation.
/// </summary>
/// <remarks>
/// This attribute is applied internally by the MockLite code generator to all generated
/// mock classes. It serves as a marker to identify which classes are auto-generated mocks,
/// allowing analyzers and tools to treat them specially (e.g., skip certain code analysis rules).
/// 
/// You typically do not need to apply this attribute manually; it is added automatically
/// during the code generation process.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GeneratedMockAttribute : Attribute { }