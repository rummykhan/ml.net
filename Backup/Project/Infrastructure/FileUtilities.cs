using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Project.Infrastructure
{
    public class FileUtilities
    {
        public static string ToImageSrcString(string path)
        {
            byte[] bytes = null;
            
            if (!File.Exists(path))
            {
                return String.Empty;
            }
            else
            {
                bytes = File.ReadAllBytes(path);
            }

            if (bytes == null)
            {
                return string.Empty;
            }

            var base64 = Convert.ToBase64String(bytes);
            return String.Format("data:image/gif;base64,{0}", base64);
        }

        public static string ToAudioSrcString(string path)
        {
            byte[] bytes = null;

            if(!File.Exists(path))
            {
                return String.Empty;
            }
            else
            {
                bytes = File.ReadAllBytes(path);
            }

            if (bytes == null)
            {
                return string.Empty;
            }

            var base64 = Convert.ToBase64String(bytes);
            return String.Format("data:audio/mp3;base64,{0}", base64);
        }
    }
}