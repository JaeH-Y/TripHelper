using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Trip.Interfaces.Services;
using Trip.Interfaces.ViewModels;
using Trip.Models;
using Trip.Messages;

namespace Trip.Services
{
    public class FavoritePlaceService : IFavoritePlaceService
    {
        public string FolderPath { get; set; }
        public string FilePath { get; set; }

        private IShowDialogService _dialogS;
        private readonly IMessenger _messenger;
        public FavoritePlaceService(IShowDialogService dialogS, IMessenger messenger)
        {
            _dialogS = dialogS;
            _messenger = messenger;
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Trips", "FavoritePlaces");
            // 폴더 없으면 생성
            Directory.CreateDirectory(FolderPath);
            // 폴더에 파일 없으면 생성 && 파일은 저장할 때 생성함
            FilePath = Path.Combine(FolderPath, "FavoritePlaces.json");
        }
        public void AddFavorite(FavoritePlaceModel item)
        {
            // Json 파싱 할 리스트 생성
            List<FavoritePlaceModel> list;
            if (File.Exists(FilePath))
            {
                list = ReadJson(FilePath);
            }
            else { list = new List<FavoritePlaceModel>(); }

            // 중복 확인
            bool existPlace = list.Any(x =>
            x.PlaceName == item.PlaceName &&
            x.NickName == item.NickName
            );
            if (existPlace)
            {
                return;
            }
            else
            {
                list.Add(item);
            }

            // 저장
            SaveJson(list);
        }
        public void DeleteFavorite(FavoritePlaceModel item)
        {
            if(!File.Exists(FilePath)) return;

            // 주소에 있는 Json을 FavoritePlaceModel 형식에 맞게 읽음
            var list = ReadJson(FilePath);

            // 삭제 대상 확인
            list.RemoveAll(x =>
            x.PlaceName == item.PlaceName &&
            x.NickName == item.NickName);

            // 저장
            SaveJson(list);
        }
        public void EditFavorite(List<FavoritePlaceModel> editItems)
        {
            editItems.RemoveAll(x =>
            x.IsFavorite == false);

            SaveJson(editItems);
        }
        public void SaveFavorite()
        {
            // PDF 같은걸로 저장할까 생각중
        }
        public List<FavoritePlaceModel> LoadFavorite()
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            var list = ReadJson(FilePath);

            return list;
        }
        public List<FavoritePlaceModel> ReadJson(string filepath)
        {
            try
            {
                var list = JsonSerializer.Deserialize<List<FavoritePlaceModel>>(File.ReadAllText(filepath)) ?? new List<FavoritePlaceModel>();
                return list;
            }
            catch(IOException ioex)
            {
                return null;
            }
            catch(JsonException jsex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void SaveJson(List<FavoritePlaceModel> list)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var newJson = JsonSerializer.Serialize(list, options);
            File.WriteAllText(FilePath, newJson);

            _dialogS.NotReturnMessageBoxShow("즐겨찾기 저장", "성공적으로 저장되었습니다!");
            _messenger.Send(new ChangedMessage(ChangeKind.Reload), MessageTokens.FavoritePlaces);
        }
    }
}
