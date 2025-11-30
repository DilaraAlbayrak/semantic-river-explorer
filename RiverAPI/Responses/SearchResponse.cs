using RiverAPI.Domain;

namespace RiverAPI.Responses
{
    public class SearchResponse
    {
        public double SimilarityScore { get; set; }
        public River River { get; set; }
    }
}
