using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Brain.Storage;
using Newtonsoft.Json;

namespace Boxed.Common
{
    public class FileUtil
    {
        public static async Task<string> Read(string folderName, string filename)
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName,
                    CreationCollisionOption.OpenIfExists);

                var file = await folder.GetFileAsync(filename);

                return await FileIO.ReadTextAsync(file);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public static async Task<T> Read<T>(string foldername, string filename)
        {
            var json = await Read(foldername, filename);
            if (string.IsNullOrWhiteSpace(json))
                return default(T);

            return JsonConvert.DeserializeObject<T>(json);
        }


        public static async Task<bool> Write<T>(string folderName, string filename, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);

                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName,
                    CreationCollisionOption.OpenIfExists);

                var file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteTextAsync(file, json);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

    }
}
