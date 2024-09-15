namespace SocialNetworkProject_3_22_05.Services
{
    public interface IImageService
    {
        Task<string> SaveFileAsync(IFormFile file);
    }
}
