namespace ASP_P22.Services.Kdf
{
    // key derivation function by rfc2898
    public interface IKdfService
    {
        string Dk(string password, string salt, uint iterationCount, uint dkLength);
    }
}
