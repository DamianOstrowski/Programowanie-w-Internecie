namespace HelpDeskHero.Shared.Contracts.Common;

public sealed class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ApiErrorDto? Error { get; set; }

    public static ApiResponseDto<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ApiResponseDto<T> Fail(ApiErrorDto error) =>
        new() { Success = false, Error = error };
}
