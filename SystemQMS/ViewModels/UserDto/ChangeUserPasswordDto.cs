namespace SystemQMS.ViewModels.UserDto
{
    public class ChangeUserPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
        public string? ConfirmPassword { get; set; }
    }
}