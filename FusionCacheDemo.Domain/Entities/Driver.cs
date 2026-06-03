namespace FusionCacheDemo.Domain.Entities
{
    public class Driver
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int TruckId { get; set; }
        public bool IsActive { get; set; }
    }
}
