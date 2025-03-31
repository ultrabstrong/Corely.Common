﻿using Corely.Common.Models;

namespace Corely.Common.UnitTests.Models;

public class DisposeBaseTests
{
    public class MockDisposeBase : DisposeBase
    {
        protected override void DisposeManagedResources() { }

        protected override void DisposeUnmanagedResources() { }
    }

    private readonly Mock<MockDisposeBase> _mockDisposeBase = new() { CallBase = true };

    [Fact]
    public void Dispose_CallsCorrectOverrides()
    {
        _mockDisposeBase.Object.Dispose();
        _mockDisposeBase.Protected().Verify("DisposeManagedResources", Times.Once());
        _mockDisposeBase.Protected().Verify("DisposeUnmanagedResources", Times.Once());
        _mockDisposeBase.Protected().Verify("DisposeAsyncCore", Times.Never());
    }

    [Fact]
    public async Task DisposeAsync_CallsCorrectOverrides_Async()
    {
        await _mockDisposeBase.Object.DisposeAsync();
        _mockDisposeBase.Protected().Verify("DisposeManagedResources", Times.Never());
        _mockDisposeBase.Protected().Verify("DisposeUnmanagedResources", Times.Once());
        _mockDisposeBase.Protected().Verify("DisposeAsyncCore", Times.Once());
    }
}
