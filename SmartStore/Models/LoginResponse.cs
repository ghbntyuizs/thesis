namespace SmartStorePOS.Models
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public User UserInfo { get; set; }
        public bool PasswordChangeNeeded { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
