namespace Backend.Common.Results;

public interface IDataResult<T> : IResult
{
    T Data { get; }
}
