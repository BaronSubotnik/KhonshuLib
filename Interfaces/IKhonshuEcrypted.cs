namespace Khonshu.Interfaces;

public interface IKhonshuEcrypted
{
    public Task<String> ToEncryptedStringAsync();
    
    public Task<String> ToDecryptedStringAsync();
    
    public String ToEncrypeted();

    public String ToDecrypted();

}
    