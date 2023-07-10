namespace unfollowers.Services.Dtos
{
    public class Status
    {
        public int Code { get; set; }
        public string ErrorMessage { get; set; }

        public Status()
        {
            Code = StatusCodes.Status200OK;
            ErrorMessage = "";
        }

        public Status(int code, string message)
        {
            Code = code;
            ErrorMessage = message;
        }
    }
}
