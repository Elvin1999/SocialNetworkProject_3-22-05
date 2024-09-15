
namespace SocialNetworkProject_3_22_05.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHost;

        public ImageService(IWebHostEnvironment webHost)
        {
            _webHost = webHost;
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            var saveImg = Path.Combine(_webHost.WebRootPath, "images", file.FileName);
            using (var img=new FileStream(saveImg,FileMode.OpenOrCreate))
            {
                await file.CopyToAsync(img);
            }
            return file.FileName.ToString();
        }
    }
}
