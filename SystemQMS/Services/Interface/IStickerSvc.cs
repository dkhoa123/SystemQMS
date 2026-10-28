using SystemQMS.Models;

namespace SystemQMS.Services.Interface
{
    public interface IStickerSvc
    {
        public string AddNew(Sticker sticker);
        byte[] ExportExcel();

    }
}
