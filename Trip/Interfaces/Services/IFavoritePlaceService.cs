using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trip.Models;

namespace Trip.Interfaces.Services
{
    public interface IFavoritePlaceService
    {
        string FolderPath { get; set; }
        string FilePath { get; set; }
        void AddFavorite(FavoritePlaceModel item);
        void DeleteFavorite(FavoritePlaceModel item);
        void EditFavorite(List<FavoritePlaceModel> editItems);
        void SaveFavorite();
        List<FavoritePlaceModel> LoadFavorite();
    }
}
