namespace DriveShare.API.Models.DTOs
{
    public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    
    public string SecurityAnswer1 { get; set; }
    public string SecurityAnswer2 { get; set; }
    public string SecurityAnswer3 { get; set; }
}

}
