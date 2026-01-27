namespace ViaTradeBackend.Models.Auth
{
    public class LoginRequest
    {
        public required string Login {  get; set; }
        public required string Password { get; set; }
    }
}
