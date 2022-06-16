using LiteDB;

namespace EventBroadcaster.LocalDb
{
    public class Sequences
    {
        public ObjectId Id { get; set; }
        public int Version { get; set; }
        public string TableName { get; set; }
        public string Value { get; set; }

    }
}
