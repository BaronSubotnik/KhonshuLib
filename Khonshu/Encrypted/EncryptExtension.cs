namespace Khonshu.Encrypted;

public static class EncryptExtension
{
    extension(String text)
    {
        public async Task<String> EncryptAsync(String yourCustomKey, String yourCustomSalt)
        {
            HelpKhonshu helperKeysAndIv = new(yourCustomKey, yourCustomSalt);
            Shifrovalshik shifrovalshik = new(helperKeysAndIv);
            return await  shifrovalshik.EncryptAsync(text);
        }

        public async Task<String> EncryptAsync(HelpKhonshu keyAndIv)
        {
            Shifrovalshik shifrovalshik = new(keyAndIv);
            return await shifrovalshik.EncryptAsync(text);
        }

        public async Task<String> DecryptAsync(String yourCustomKey, String yourCustomSalt)
        {
            HelpKhonshu helperKeyAndIv = new(yourCustomKey, yourCustomSalt);
            Shifrovalshik shifr = new(helperKeyAndIv);
            return await  shifr.DecryptAsync(yourCustomKey);
        }

        public async Task<String> DecryptAsync(HelpKhonshu keyAndIv)
        {
            Shifrovalshik shifr  = new(keyAndIv);
            return await shifr.DecryptAsync(text);
        }
    }
}