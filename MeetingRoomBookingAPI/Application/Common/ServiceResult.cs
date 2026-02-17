using System.Collections.Generic;

namespace MeetingRoomBookingAPI.Application.Common
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static ServiceResult<T> SuccessResult(T data, int statusCode = 200)
        {
            return new ServiceResult<T> { Success = true, Data = data, StatusCode = statusCode };
        }

        public static ServiceResult<T> FailureResult(string errorMessage, int statusCode = 400)
        {
            return new ServiceResult<T> { Success = false, Message = errorMessage, StatusCode = statusCode };
        }
    }
}
