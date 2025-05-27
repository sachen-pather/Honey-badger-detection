namespace Honey_badger_detection.Models
{
    public class AnalysisResult
    {
        public bool IsHoneyBadger { get; set; }
        public double Confidence { get; set; }
        public object FullResponse { get; set; }
        public long ProcessingTimeMs { get; set; }
        public string? DropboxPath { get; set; }
        public string? DropboxUploadError { get; set; }
    }
}