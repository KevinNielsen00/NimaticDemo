namespace Backend.DTOs.Measurements
{
    public class MeasurementDto
    {
        public int Depth { get; set; }
        public int Restarts { get; set; }
        public decimal Battery { get; set; }
        public decimal Temperature { get; set; }
        public DateTime MeasuredAt { get; set; }
    }
}
