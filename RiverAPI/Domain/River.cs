using NetTopologySuite.Geometries;

namespace RiverAPI.Domain
{
    public class River
    {
        public string Id { get; set; }
        public string? Flow_Direction { get; set; }
        public double Length { get; set; }
        public bool Fictitious { get; set; }
        public string? Form { get; set; }
        public string? Watercourse_Name { get; set; }
        public string? Watercourse_Name_Alternative { get; set; }
        public string? Start_Node { get; set; }
        public string? End_Node { get; set; }
        public Geometry Geometry { get; set; }
    }
}