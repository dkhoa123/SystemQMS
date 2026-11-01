using SystemQMS.Models;

namespace SystemQMS.Services.Interface
{
    public interface IStickerSvc
    {
        public string AddNew(Sticker sticker);
        byte[] ExportExcel();
        Task<Sticker?> CallNext(int counterId);

        Task<List<Sticker>> CallMultiple(int counterId, int quantity);
        Task<Sticker?> CallAgain(int counterId);
        Task<Sticker?> Skip(int counterId);
        Task<Sticker?> TransferCounter(
        string sttId,
        int targetCounterId);
        Task<List<Sticker>> TransferSelected(  List<string> sttIds, int targetCounterId);
    }

}
