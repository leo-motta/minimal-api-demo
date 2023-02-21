namespace MinimalApi.Models
{
    public class Provider
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Document { get; set; }
        public bool isActive { get; set; }
    }
}