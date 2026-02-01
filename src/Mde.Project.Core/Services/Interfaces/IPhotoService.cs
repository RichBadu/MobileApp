using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Services.Interfaces
{
    public interface IPhotoService
    {
        Task<string?> TakePhotoAsync();
        Task<string?> PickPhotoAsync();
        Task<bool> DeletePhotoAsync(string photoPath);

    }
}
