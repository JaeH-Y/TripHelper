using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trip.Interfaces.Services;
using Trip.Messages;
using Trip.Models;

namespace Trip.Services
{
    public class AccommodationService : IAccommodationService
    {
        private readonly IMessenger _messenger;
        private readonly IShowDialogService _dialogS;
        private string FolderPath;
        private string FilePath;
        public AccommodationService(IMessenger messenger, IShowDialogService dialogS)
        {
            _messenger = messenger;
            _dialogS = dialogS;

            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Trips", "Accommodations");
            // 폴더 없으면 생성
            Directory.CreateDirectory(FolderPath);
            // 폴더에 파일 없으면 생성 && 파일은 저장할 때 생성함
            FilePath = Path.Combine(FolderPath, "Accommodations.json");
        }
        public void AddAccommodation(AccommodationModel item)
        {
            List<AccommodationModel> list;
            if (File.Exists(FilePath))
            {
                list = ReadJson(FilePath);
            }
            else { list = new List<AccommodationModel>(); }

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
        public List<AccommodationModel> LoadFavorite()
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            var list = ReadJson(FilePath);

            return list;
        }
        public List<AccommodationModel> ReadJson(string filePath)
        {
            try
            {
                var list = JsonSerializer.Deserialize<List<AccommodationModel>>(File.ReadAllText(filePath)) ?? new List<AccommodationModel>();
                return list;
            }
            catch (IOException ioex)
            {
                return null;
            }
            catch (JsonException jsex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void EditJSON(List<AccommodationModel> editItems)
        {
            editItems.RemoveAll(x =>
            x.IsFavorite == false);

            SaveJson(editItems);
        }
        public void SaveJson(List<AccommodationModel> list)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var newJson = JsonSerializer.Serialize(list, options);
            File.WriteAllText(FilePath, newJson);

            _dialogS.NotReturnMessageBoxShow("숙소 저장", "성공적으로 저장되었습니다!");
            _messenger.Send(new ChangedMessage(ChangeKind.Reload), MessageTokens.Accommodation);
        }
    }
}
