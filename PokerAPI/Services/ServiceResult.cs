namespace PokerAPI.Services
{
    /// <summary>
    /// Generic wrapper for service responses, supporting success/failure, data, and messages.
    /// </summary>
    /// <typeparam name="T">Type of the data returned by the service.</typeparam>
    public class ServiceResult<T>
    {
        /// <summary>
        /// Indicates whether the service operation was successful.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Optional message describing the result or error.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The data returned by the service (if any).
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// Private constructor to enforce usage of helper methods.
        /// </summary>
        private ServiceResult(bool isSuccess, T data, string message)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
        }

        /// <summary>
        /// Creates a successful ServiceResult with optional data and message.
        /// </summary>
        public static ServiceResult<T> Success(T data = default!, string message = "")
        {
            return new ServiceResult<T>(true, data, message);
        }

        /// <summary>
        /// Creates a failed ServiceResult with optional message.
        /// </summary>
        public static ServiceResult<T> Failure(string message)
        {
            return new ServiceResult<T>(false, default!, message);
        }
    }

    /// <summary>
    /// Non-generic version for services that don't return data.
    /// </summary>
    public class ServiceResult
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }

        private ServiceResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public static ServiceResult Success(string message = "")
        {
            return new ServiceResult(true, message);
        }

        public static ServiceResult Failure(string message)
        {
            return new ServiceResult(false, message);
        }
    }
}
