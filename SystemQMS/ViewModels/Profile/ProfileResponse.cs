namespace SystemQMS.ViewModels.Profile
{
    public class ProfileResponse
    {
        public int userId { get; set; }
        public string? username { get; set; }
        public string? email { get; set; }
        public string? numberPhone { get; set; }
        public byte[]? avatar { get; set; }
        public int? unitId { get; set; }
        public int roleId { get; set; }
        public int? counterId { get; set; }
        public bool isActive { get; set; }
        public bool isVerified { get; set; }
        public DateTimeOffset? createdAt { get; set; }
    }
}