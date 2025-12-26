using System;
using System.Collections.Generic;

namespace BbQ.MockLite.Tests;

/// <summary>
/// Unit tests for strongly-typed Setup and OnCall functionality with partial handler signatures.
/// Tests the new overloads that allow handlers to accept 0 to n parameters matching the method signature prefix.
/// </summary>
public class StronglyTypedCallbackTests
{
    // ==================== SETUP WITH PARTIAL PARAMETERS - 0 PARAMS ====================

    [Fact]
    public void Test_Setup_WithZeroParams_ReturnsValue()
    {
        // Arrange
        var mock = Mock.Create<IQueryService>()
            .Setup(x => x.Query("proc", 1, 2), () => "result");

        // Act
        var result = mock.Object.Query("proc", 1, 2);

        // Assert
        Assert.Equal("result", result);
    }

    // ==================== SETUP WITH PARTIAL PARAMETERS - 1 PARAM ====================

    [Fact]
    public void Test_Setup_WithOneParam_ReceivesFirstParameter()
    {
        // Arrange
        var mock = Mock.Create<IQueryService>()
            .Setup(x => x.Query("myproc", 1, 2), (string proc) => $"Result: {proc}");

        // Act
        var result = mock.Object.Query("myproc", 1, 2);

        // Assert
        Assert.Equal("Result: myproc", result);
    }

    [Fact]
    public void Test_Setup_WithOneParam_UsesActualArgument()
    {
        // Arrange
        var mock = Mock.Create<IQueryService>()
            .Setup(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc) => $"Executing: {proc}");

        // Act
        var result = mock.Object.Query("stored_proc", 100, 200);

        // Assert
        Assert.Equal("Executing: stored_proc", result);
    }

    [Fact]
    public void Test_Setup_WithOneParam_MultipleInvocations()
    {
        // Arrange
        var results = new List<string>();
        var mock = Mock.Create<IQueryService>()
            .Setup(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc) => 
                {
                    results.Add(proc);
                    return $"Result for {proc}";
                });

        // Act
        mock.Object.Query("proc1", 1, 2);
        mock.Object.Query("proc2", 3, 4);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains("proc1", results);
        Assert.Contains("proc2", results);
    }

    // ==================== SETUP WITH PARTIAL PARAMETERS - 2 PARAMS ====================

    [Fact]
    public void Test_Setup_WithTwoParams_ReceivesTwoParameters()
    {
        // Arrange
        var mock = Mock.Create<IQueryService>()
            .Setup(x => x.Query("proc", 1, 2), (string proc, int id) => $"{proc}:{id}");

        // Act
        var result = mock.Object.Query("proc", 1, 2);

        // Assert
        Assert.Equal("proc:1", result);
    }

    [Fact]
    public void Test_Setup_WithTwoParams_UsesActualArguments()
    {
        // Arrange
        var mock = Mock.Create<IQueryService>()
            .Setup(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc, int id) => $"{proc}-{id}");

        // Act
        var result = mock.Object.Query("execute", 42, 999);

        // Assert
        Assert.Equal("execute-42", result);
    }

    // ==================== SETUP WITH PARTIAL PARAMETERS - 3 PARAMS ====================

    [Fact]
    public void Test_Setup_WithThreeParams_ReceivesAllThreeParameters()
    {
        // Arrange
        var mock = Mock.Create<IQueryService>()
            .Setup(x => x.Query("proc", 1, 2), 
                (string proc, int id, int count) => $"{proc}:{id}:{count}");

        // Act
        var result = mock.Object.Query("proc", 1, 2);

        // Assert
        Assert.Equal("proc:1:2", result);
    }

    [Fact]
    public void Test_Setup_WithThreeParams_UsesActualArguments()
    {
        // Arrange
        var mock = Mock.Create<IQueryService>()
            .Setup(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc, int id, int count) => $"{proc}[{id},{count}]");

        // Act
        var result = mock.Object.Query("query", 10, 20);

        // Assert
        Assert.Equal("query[10,20]", result);
    }

    // ==================== ONCALL WITH PARTIAL PARAMETERS - 0 PARAMS ====================

    [Fact]
    public void Test_OnCall_WithZeroParams_ExecutesHandler()
    {
        // Arrange
        var callCount = 0;
        var mock = Mock.Create<IQueryService>()
            .OnCall(x => x.Query("proc", 1, 2), () => callCount++);

        // Act
        mock.Object.Query("proc", 1, 2);

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Test_OnCall_WithZeroParams_MultipleInvocations()
    {
        // Arrange
        var callCount = 0;
        var mock = Mock.Create<IQueryService>()
            .OnCall(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                () => callCount++);

        // Act
        mock.Object.Query("proc1", 1, 2);
        mock.Object.Query("proc2", 3, 4);
        mock.Object.Query("proc3", 5, 6);

        // Assert
        Assert.Equal(3, callCount);
    }

    // ==================== ONCALL WITH PARTIAL PARAMETERS - 1 PARAM ====================

    [Fact]
    public void Test_OnCall_WithOneParam_ReceivesFirstParameter()
    {
        // Arrange
        var capturedProc = "";
        var mock = Mock.Create<IQueryService>()
            .OnCall(x => x.Query("myproc", 1, 2), (string proc) => capturedProc = proc);

        // Act
        mock.Object.Query("myproc", 1, 2);

        // Assert
        Assert.Equal("myproc", capturedProc);
    }

    [Fact]
    public void Test_OnCall_WithOneParam_TracksMultipleCalls()
    {
        // Arrange
        var capturedProcs = new List<string>();
        var mock = Mock.Create<IQueryService>()
            .OnCall(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc) => capturedProcs.Add(proc));

        // Act
        mock.Object.Query("proc1", 1, 2);
        mock.Object.Query("proc2", 3, 4);

        // Assert
        Assert.Equal(2, capturedProcs.Count);
        Assert.Contains("proc1", capturedProcs);
        Assert.Contains("proc2", capturedProcs);
    }

    // ==================== ONCALL WITH PARTIAL PARAMETERS - 2 PARAMS ====================

    [Fact]
    public void Test_OnCall_WithTwoParams_ReceivesTwoParameters()
    {
        // Arrange
        var capturedProc = "";
        var capturedId = 0;
        var mock = Mock.Create<IQueryService>()
            .OnCall(x => x.Query("proc", 42, 2), (string proc, int id) =>
            {
                capturedProc = proc;
                capturedId = id;
            });

        // Act
        mock.Object.Query("proc", 42, 2);

        // Assert
        Assert.Equal("proc", capturedProc);
        Assert.Equal(42, capturedId);
    }

    [Fact]
    public void Test_OnCall_WithTwoParams_TracksAllInvocations()
    {
        // Arrange
        var invocations = new List<(string, int)>();
        var mock = Mock.Create<IQueryService>()
            .OnCall(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc, int id) => invocations.Add((proc, id)));

        // Act
        mock.Object.Query("a", 1, 99);
        mock.Object.Query("b", 2, 99);
        mock.Object.Query("c", 3, 99);

        // Assert
        Assert.Equal(3, invocations.Count);
        Assert.Contains(("a", 1), invocations);
        Assert.Contains(("b", 2), invocations);
        Assert.Contains(("c", 3), invocations);
    }

    // ==================== ONCALL WITH PARTIAL PARAMETERS - 3 PARAMS ====================

    [Fact]
    public void Test_OnCall_WithThreeParams_ReceivesAllThreeParameters()
    {
        // Arrange
        var capturedProc = "";
        var capturedId = 0;
        var capturedCount = 0;
        var mock = Mock.Create<IQueryService>()
            .OnCall(x => x.Query("proc", 10, 20), (string proc, int id, int count) =>
            {
                capturedProc = proc;
                capturedId = id;
                capturedCount = count;
            });

        // Act
        mock.Object.Query("proc", 10, 20);

        // Assert
        Assert.Equal("proc", capturedProc);
        Assert.Equal(10, capturedId);
        Assert.Equal(20, capturedCount);
    }

    // ==================== VOID METHOD ONCALL - 0 PARAMS ====================

    [Fact]
    public void Test_OnCall_VoidMethod_WithZeroParams_ExecutesHandler()
    {
        // Arrange
        var callCount = 0;
        var mock = Mock.Create<ITestService>()
            .OnCall(x => x.Process("data", 1, 2), () => callCount++);

        // Act
        mock.Object.Process("data", 1, 2);

        // Assert
        Assert.Equal(1, callCount);
    }

    // ==================== VOID METHOD ONCALL - 1 PARAM ====================

    [Fact]
    public void Test_OnCall_VoidMethod_WithOneParam_ReceivesFirstParameter()
    {
        // Arrange
        var capturedData = "";
        var mock = Mock.Create<ITestService>()
            .OnCall(x => x.Process("testdata", 1, 2), (string data) => capturedData = data);

        // Act
        mock.Object.Process("testdata", 1, 2);

        // Assert
        Assert.Equal("testdata", capturedData);
    }

    // ==================== VOID METHOD ONCALL - 2 PARAMS ====================

    [Fact]
    public void Test_OnCall_VoidMethod_WithTwoParams_ReceivesTwoParameters()
    {
        // Arrange
        var capturedData = "";
        var capturedId = 0;
        var mock = Mock.Create<ITestService>()
            .OnCall(x => x.Process("data", 42, 2), (string data, int id) =>
            {
                capturedData = data;
                capturedId = id;
            });

        // Act
        mock.Object.Process("data", 42, 2);

        // Assert
        Assert.Equal("data", capturedData);
        Assert.Equal(42, capturedId);
    }

    // ==================== VOID METHOD ONCALL - 3 PARAMS ====================

    [Fact]
    public void Test_OnCall_VoidMethod_WithThreeParams_ReceivesAllThreeParameters()
    {
        // Arrange
        var capturedData = "";
        var capturedId = 0;
        var capturedCount = 0;
        var mock = Mock.Create<ITestService>()
            .OnCall(x => x.Process("data", 10, 20), (string data, int id, int count) =>
            {
                capturedData = data;
                capturedId = id;
                capturedCount = count;
            });

        // Act
        mock.Object.Process("data", 10, 20);

        // Assert
        Assert.Equal("data", capturedData);
        Assert.Equal(10, capturedId);
        Assert.Equal(20, capturedCount);
    }

    // ==================== INTEGRATION TESTS ====================

    [Fact]
    public void Test_Setup_And_OnCall_Together()
    {
        // Arrange
        var callLog = new List<string>();
        var mock = Mock.Create<IQueryService>()
            .Setup(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc, int id) => $"Result-{proc}-{id}")
            .OnCall(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc, int id) => callLog.Add($"Called: {proc}, {id}"));

        // Act
        var result1 = mock.Object.Query("proc1", 1, 99);
        var result2 = mock.Object.Query("proc2", 2, 99);

        // Assert
        Assert.Equal("Result-proc1-1", result1);
        Assert.Equal("Result-proc2-2", result2);
        Assert.Equal(2, callLog.Count);
        Assert.Contains("Called: proc1, 1", callLog);
        Assert.Contains("Called: proc2, 2", callLog);
    }

    [Fact]
    public void Test_MultipleOnCall_DifferentSignatures()
    {
        // Arrange
        var zeroParamCount = 0;
        var oneParamLog = new List<string>();
        var twoParamLog = new List<(string, int)>();

        var mock = Mock.Create<IQueryService>()
            .OnCall(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                () => zeroParamCount++)
            .OnCall(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc) => oneParamLog.Add(proc))
            .OnCall(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc, int id) => twoParamLog.Add((proc, id)));

        // Act
        mock.Object.Query("test", 5, 10);

        // Assert
        Assert.Equal(1, zeroParamCount);
        Assert.Single(oneParamLog);
        Assert.Equal("test", oneParamLog[0]);
        Assert.Single(twoParamLog);
        Assert.Equal(("test", 5), twoParamLog[0]);
    }

    [Fact]
    public void Test_Setup_ChainsWithVerify()
    {
        // Arrange
        var mock = Mock.Create<IQueryService>()
            .Setup(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc) => $"Result: {proc}");

        // Act
        mock.Object.Query("test", 1, 2);

        // Assert
        mock.Verify(x => x.Query("test", 1, 2), times => times == 1);
    }

    [Fact]
    public void Test_OnCall_WithComplexLogic()
    {
        // Arrange
        var auditLog = new List<string>();
        var mock = Mock.Create<IQueryService>()
            .OnCall(x => x.Query(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                (string proc, int id, int count) => 
                {
                    if (id > 100)
                        auditLog.Add($"HIGH ID: {proc} with id={id}, count={count}");
                    else
                        auditLog.Add($"NORMAL: {proc}");
                });

        // Act
        mock.Object.Query("proc1", 50, 10);
        mock.Object.Query("proc2", 150, 20);

        // Assert
        Assert.Equal(2, auditLog.Count);
        Assert.Contains("NORMAL: proc1", auditLog);
        Assert.Contains("HIGH ID: proc2 with id=150, count=20", auditLog);
    }

    // ==================== TYPE SAFETY TESTS ====================

    [Fact]
    public void Test_Setup_WithCorrectTypes_CompilesAndWorks()
    {
        // Arrange
        var mock = Mock.Create<ITypedService>()
            .Setup(x => x.Compute("test", 42, true), 
                (string name, int value, bool flag) => name.Length + value + (flag ? 1 : 0));

        // Act
        var result = mock.Object.Compute("test", 42, true);

        // Assert
        Assert.Equal(47, result); // "test".Length (4) + 42 + 1
    }

    [Fact]
    public void Test_OnCall_PreservesParameterTypes()
    {
        // Arrange
        var capturedName = "";
        var capturedValue = 0;
        var capturedFlag = false;
        var mock = Mock.Create<ITypedService>()
            .OnCall(x => x.Compute("test", 100, true), 
                (string name, int value, bool flag) =>
                {
                    capturedName = name;
                    capturedValue = value;
                    capturedFlag = flag;
                });

        // Act
        mock.Object.Compute("test", 100, true);

        // Assert
        Assert.Equal("test", capturedName);
        Assert.Equal(100, capturedValue);
        Assert.True(capturedFlag);
    }

    // Test interfaces
    private interface IQueryService
    {
        string Query(string proc, int id, int count);
    }

    private interface ITestService
    {
        void Process(string data, int id, int count);
    }

    private interface ITypedService
    {
        int Compute(string name, int value, bool flag);
    }
}
