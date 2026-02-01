using Mde.Project.Mobile.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Domain.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly string _photoDirectory;
        public PhotoService()
        {
            _photoDirectory = Path.Combine(FileSystem.AppDataDirectory, "Photos");

            if (!Directory.Exists(_photoDirectory))
            {
                Directory.CreateDirectory(_photoDirectory);
            }
        }
        public async Task<string?> TakePhotoAsync()
        {
         
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    throw new NotSupportedException("Camera is not available on this device");
                }
                var photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo == null)
                    return null;

                return await SavePhotoAsync(photo);
        }

        public async Task<string?> PickPhotoAsync()
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo == null)
                return null;

            return await SavePhotoAsync(photo);
        }

        public Task<bool> DeletePhotoAsync(string photoPath)
        {
            if (File.Exists(photoPath))
            {
                File.Delete(photoPath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        private async Task<string> SavePhotoAsync(FileResult photo)
        {
            
            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(_photoDirectory, fileName);

            
            using var sourceStream = await photo.OpenReadAsync();
            using var fileStream = File.Create(filePath);
            await sourceStream.CopyToAsync(fileStream);

            return filePath;
        }


    }
}