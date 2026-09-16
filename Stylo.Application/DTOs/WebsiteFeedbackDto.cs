namespace Stylo.Backend.Stylo.Application.DTOs
{
    public class CreateWebsiteFeedbackDto
    {
        public string Message { get; set; } = string.Empty;
    }
    public class WebsiteFeedbackDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
    public class WebsiteFeedbackAdminDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
