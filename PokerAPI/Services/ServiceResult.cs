namespace PokerAPI.Services
{

    public class ServiceResult<T>
    {
      
        public bool IsSuccess { get; private set; }


        public string Message { get; private set; }


        public T Data { get; private set; }


        private ServiceResult(bool isSuccess, T data, string message)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
        }


        public static ServiceResult<T> Success(T data = default!, string message = "")
        {
            return new ServiceResult<T>(true, data, message);
        }


        public static ServiceResult<T> Failure(string message)
        {
            return new ServiceResult<T>(false, default!, message);
        }
    }


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
