using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Trip.Interfaces.ViewModels;
using Trip.Models;

namespace Trip.ViewModels
{
    public class PlanDictonaryViewModel : ViewModelBase
    {
        private string _planName;
        public string PlanName
        {
            get => _planName;
            set => SetProperty(ref _planName, value);
        }
        private int _planNum;
        public int PlanNum
        {
            get => _planNum;
            set => SetProperty(ref _planNum, value);
        }
        private Visibility _editVisibility = Visibility.Collapsed;
        public Visibility EditVisibility
        {
            get => _editVisibility;
            set => SetProperty(ref _editVisibility, value);
        }

        public ObservableCollection<PlaceModel> Places { get; set; } = new();
        private IPlanCabinetViewModel _cabinetViewModel;
        public ICommand RemovePlanCommand { get; set; }
        public PlanDictonaryViewModel(string name, IEnumerable<PlaceModel> places, IPlanCabinetViewModel cabinetViewModel)
        {
            _cabinetViewModel = cabinetViewModel;

            _planName = name;
            foreach (PlaceModel place in places ?? Enumerable.Empty<PlaceModel>())
            {
                Places.Add(place);
            }

            RemovePlanCommand = new RelayCommand(RemovePlan);
        }
        private void RemovePlan()
        {
            _cabinetViewModel.RemovePlan(this);
        }
    }
}
