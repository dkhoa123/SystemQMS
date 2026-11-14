using SystemQMS.Models;

namespace SystemQMS.Services.Interface
{
    public interface IStickerSvc
    {
        public string AddNew(Tickets sticker);
        byte[] ExportExcel();

    }
}
