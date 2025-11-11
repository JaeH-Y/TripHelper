using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trip.Models;

namespace Trip.Interfaces.Services
{
    public interface IAccommodationService
    {
        void AddAccommodation(AccommodationModel item);
        List<AccommodationModel> LoadFavorite();
        List<AccommodationModel> ReadJson(string filePath);
        void EditJSON(List<AccommodationModel> editItems);
        void SaveJson(List<AccommodationModel> list);
    }
}
