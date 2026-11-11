using SystemQMS.Models;

using SystemQMS.ViewModels.NumberFomular;

namespace SystemQMS.Services.Interface
{
    public interface INumberFomularSvc
    {
        public string AddNew(CreateNumberFomularDto dto);
        Task<List<NumberFomulaListDto>> GetAll();
        Task<string> Delete(int id);


        Task<string> Update(int id, UpdateNumberFomularDto dto);

        Task<string> Preview(int serviceId);
        //Task<bool> ConvertToShared(List<int> serviceIds);
        //public string ReSet(int fomulaId);
    }
}
