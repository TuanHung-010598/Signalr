namespace ezCloud.SignalR.Models
{
    public class UserSessionInfoModel
    {
        public int UserSessionId { get; set; }
        public string OwnerUserId { get; set; }
        public string AuthToken { get; set; }
        public DateTime ExpirationDateTime { get; set; }
        public DateTime? LastActiveDate { get; set; }
        public int HotelId { get; set; }
    }
}
