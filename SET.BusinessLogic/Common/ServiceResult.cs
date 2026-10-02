namespace BusinessLogic;

/// <summary>
/// A failed service call. <see cref="Body"/> is written as the HTTP response body
/// (a PascalCase error token string, or an object such as <c>{ error }</c> for AI endpoints).
/// </summary>
public sealed record ServiceError( int StatusCode, object Body )
{
    public static ServiceError BadRequest( object body ) => new( 400, body );

    public static ServiceError NotFound( object body ) => new( 404, body );
}

public class ServiceResult
{
    protected ServiceResult( ServiceError? error )
    {
        Error = error;
    }

    public static ServiceResult Success { get; } = new( null );

    public ServiceError? Error { get; }

    public bool IsSuccess => Error is null;

    public static implicit operator ServiceResult( ServiceError error ) => new( error );
}

public sealed class ServiceResult<T> : ServiceResult
{
    private ServiceResult( T? value, ServiceError? error )
        : base( error )
    {
        Value = value;
    }

    public T? Value { get; }

    public static implicit operator ServiceResult<T>( T value ) => new( value, null );

    public static implicit operator ServiceResult<T>( ServiceError error ) => new( default, error );
}
