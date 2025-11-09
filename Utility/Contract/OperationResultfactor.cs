namespace Utility.Contract
{
    public class OperationResultfactor
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public OperationResultfactor(bool success, string? message = "", string? url = "")
        {
            Success = success;
            Message = message;
            Url = url;
        }
    }
}
