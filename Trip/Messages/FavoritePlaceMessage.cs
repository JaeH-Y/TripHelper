using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trip.Messages
{
    public enum ChangeKind { Added, Deleted, Edited, Reload }

    public sealed record MapNavigateMessage(Uri? Uri, int? Zoom = null, string? SourceId = null);
    public sealed record MapHtmlMessage(string? htmlMessage);
    public sealed record ChangedMessage(ChangeKind Kind);
    // 채널(토큰) - 같은 타입의 메시지를 도메인별로 격리하고 싶을 때 사용
    public static class MessageTokens
    {
        public const string FavoritePlaces = "FavoritePlaces";
        public const string Accommodation = "Accommodation";
        public const string PlanCabinet = "PlanCabinet";
        public const string NewPlanPageOpen = "NewPlanPageOpen";
        public const string MapReload = "MapReload";
    }
    public class FavoritePlaceMessage
    {
        
    }
}
