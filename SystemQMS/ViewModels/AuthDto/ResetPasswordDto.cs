namespace SystemQMS.ViewModels.AuthDto
{
    public class ResetPasswordDto
    {
        public string email { get; set; }
        public string newPassword { get; set; }
        public string confirmPassword { get; set; }
    }
}
