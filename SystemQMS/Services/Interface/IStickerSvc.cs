using SystemQMS.Models;

namespace SystemQMS.Services.Interface
{
    public interface IStickerSvc
    {
        public string AddNew(Ticket sticker);
        byte[] ExportExcel();

    }
}
