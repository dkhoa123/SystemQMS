using SystemQMS.ViewModels.WebConfigDto;

namespace SystemQMS.Services.Interface
{
    public interface IWebConfigSvc
    {
        Task<WebConfigDto> GetWebConfigAsync(int unitId);

        Task<bool> UpdateWebConfigAsync(int unitId, WebConfigDto dto);

        Task<string> UploadFileAsync(IFormFile file, string folderName);
    }
}
