using SystemQMS.Models;

namespace SystemQMS.Services
{
    public interface IStickerSvc
    {
        public string AddNew(Sticker sticker);
        byte[] ExportExcel();

    }
}
