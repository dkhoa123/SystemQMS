namespace SystemQMS.ViewModels.AuthDto
{
    public class RegisterDto
    {
        public string name { get; set; }
        public string email { get; set; }
        public string numberPhone { get; set; }
        public bool isActive { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string rePassword { get; set; }
        public int roleId { get; set; }
    }
}
