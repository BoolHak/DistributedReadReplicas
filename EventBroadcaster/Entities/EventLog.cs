using System.ComponentModel.DataAnnotations;

namespace EventBroadcaster.Entities
{
    public class EventLog
    {
        [Key]
        public int Id { get; set; }
        public string TableName { get; set; }
        public string TableId { get; set; }
        public string EventType { get; set; }
        public DateTime ModificationDate { get; set; }


    }
}
