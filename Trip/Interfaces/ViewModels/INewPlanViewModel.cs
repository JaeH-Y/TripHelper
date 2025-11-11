using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Trip.ViewModels;

namespace Trip.Interfaces.ViewModels
{
    public interface INewPlanViewModel
    {
        void EditMoveUp(AddPlaceOrPlanViewModel item);
        void EditMoveDown(AddPlaceOrPlanViewModel item);
        void EditRemove(AddPlaceOrPlanViewModel item);
        void AddFavorite(AddPlaceOrPlanViewModel item);

        // 지도 업데이트
        void SendUri(Uri? uri, int? zoom, string? placeId);
        void SendHtml(string html);
    }
}
