using System.ComponentModel.DataAnnotations;

namespace EventBroadcaster.Entities
{
    public class SequenceNumber
    {
        [Key]
        public int Id { get; set; }
        public string TableName { get; set; }
        public int CurrentIndex { get; set; }
        public DateTime TimeStamp { get; set; }

    }
}
