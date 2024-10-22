namespace Backend.Common.Results;

public class ErrorDataResult<T> : DataResult<T>
{
    public ErrorDataResult(string message) : base(default, false, message)
    {
    }

    public ErrorDataResult() : base(default, false)
    {
    }
}