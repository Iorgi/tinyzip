namespace TinyZipper.Application.Core
{
    public class QueueItem
    {
        public int Order { get; }

        public byte[] Data { get; }

        public QueueItem(int order, byte[] data)
        {
            Order = order;
            Data = data;
        }
    }
}