namespace Honey_badger_detection.Models
{
    public class CustomVisionResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Project { get; set; } = string.Empty;
        public string Iteration { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public List<Prediction> Predictions { get; set; } = new List<Prediction>();
    }

    public class Prediction
    {
        public string TagId { get; set; } = string.Empty;
        public string TagName { get; set; } = string.Empty;
        public double Probability { get; set; }
    }
}
