namespace FTP_Client.Helpers
{
    public class TransferProgressTracker
    {
        public long TotalSize { get; }
        public long TransferredBytes { get; set; }

        public TransferProgressTracker(long totalSize)
        {
            TotalSize = totalSize;
            TransferredBytes = 0;
        }
    }
}