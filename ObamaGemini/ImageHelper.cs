using System;
using System.Collections.Generic;
using System.Text;


namespace Khonshu
{
    public static class ImageHelper
    {
        public static string GetMimeType(Byte[] data)
        {
            if (data == null || data.Length < 4) return "application/octet-stream";
            
            if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            {
                return "image/jpeg";
            }

            if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            {
                return "image/png";
            }

            if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
            {
                return "image/gif";
            }

            if (data[0] == 0x42 && data[1] == 0x4D)
            {
                return "image/bmp";
            }
            return "application/octet-stream";
        }
        extension(Byte[] imageBytes)
        {
            public String GetImageMimeType()
            {
                return GetMimeType(imageBytes);
            }
        }
       
        
    }
}
