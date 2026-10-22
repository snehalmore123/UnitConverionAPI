using UnitConversionApi.Common;

namespace UnitConversionApi.Tests;

public class ResultTests
{
    private static readonly Error SampleError = Error.Validation("Test.Code", "Test message");

    [Fact]
    public void Success_HasValue()
    {
        var result = Result.Success(42);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_ExposesCodeMessageAndType()
    {
        var result = Result.Failure<int>(SampleError);
        Assert.True(result.IsFailure);
        Assert.Equal("Test.Code", result.Error.Code);
        Assert.Equal("Test message", result.Error.Message);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void FailedResult_CannotBeConsumedAsValue() =>
        Assert.Throws<InvalidOperationException>(() => Result.Failure<int>(SampleError).Value);

    [Fact]
    public void SuccessfulResult_HasNoError()
    {
        Assert.Throws<InvalidOperationException>(() => Result.Success().Error);
        Assert.Throws<InvalidOperationException>(() => Result.Success("x").Error);
    }

    [Fact]
    public void NonGenericResult_Works()
    {
        Assert.True(Result.Success().IsSuccess);
        Assert.Equal("Test.Code", Result.Failure(SampleError).Error.Code);
    }

    [Fact]
    public void Failure_WithNullError_Throws() =>
        Assert.Throws<ArgumentNullException>(() => Result.Failure<int>(null!));
}
