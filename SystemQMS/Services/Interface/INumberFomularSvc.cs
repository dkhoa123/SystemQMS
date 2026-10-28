using SystemQMS.Models;

namespace SystemQMS.Services.Interface
{
    public interface INumberFomularSvc
    {
        public string AddNew( NumberFomular numberFomular);
        public List<NumberFomular> GetNumberFomular();

        public string Update(NumberFomular numberFomular);

        public string Preview(int serviceId);
        Task<bool> ConvertToShared(List<int> serviceIds);
        public string ReSet(int fomulaId);
    }
}
