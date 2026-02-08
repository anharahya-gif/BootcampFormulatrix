namespace PokerAPIMPwDB.Common.Results
{
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; } // pakai Message untuk info
        public string? ErrorMessage { get; set; }  // pesan error spesifik 

        // Success tanpa value
        public static ServiceResult Success(string message = "") =>
            new ServiceResult { IsSuccess = true, Message = message };

        public static ServiceResult Fail(string message) =>
            new ServiceResult { IsSuccess = false, Message = message };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Value { get; set; }

        // Success dengan value (dan optional message)
        public static ServiceResult<T> Success(T value, string message = "") =>
            new ServiceResult<T> { IsSuccess = true, Value = value, Message = message };

        public static new ServiceResult<T> Fail(string message) =>
            new ServiceResult<T> { IsSuccess = false, Value = default, Message = message };
    }
}
