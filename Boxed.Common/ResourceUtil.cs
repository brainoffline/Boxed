
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Newtonsoft.Json;

namespace Boxed.Common
{
    public class ResourceUtil
    {
        public static async Task<string> Read(string folderName, string resourceName)
        {
            var filename = Path.Combine(Package.Current.InstalledLocation.Path, folderName, resourceName);

            var file = await StorageFile.GetFileFromPathAsync(filename);

            return await FileIO.ReadTextAsync(file);
        }

        public static async Task<T> Read<T>(string foldername, string resourceName)
        {
            var json = await Read(foldername, resourceName);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
