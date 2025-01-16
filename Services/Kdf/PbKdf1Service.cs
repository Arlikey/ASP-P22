using ASP_P22.Services.Hash;

namespace ASP_P22.Services.Kdf
{
    public class PbKdf1Service(IHashService hashService) : IKdfService
    {
        private readonly IHashService _hashService = hashService;

        public string Dk(string password, string salt, uint iterationCount, uint dkLength)
        {
            if (iterationCount == 0) 
            {
                throw new ArgumentException("Iteration count must be more than 0.");
            }
            string t = _hashService.Digest(password + salt);
            for (int i = 0; i < iterationCount-1; i++)
            {
                t = _hashService.Digest(t);
            }
            if (dkLength > t.Length) {
                throw new ArgumentException($"dkLength {dkLength} must be less than hash length {t.Length}.");
            }
            return t[..(int)dkLength];
        }
    }
}
