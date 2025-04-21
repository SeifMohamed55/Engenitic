
using GraduationProject.Application.Services.Interfaces;
using System.IO.Hashing;
namespace GraduationProject.Application.Services
{
    public class FileHashingService : IHashingService
    {

        public async Task<ulong> HashWithxxHash(Stream file)
        {
            var xxHash = new XxHash64(); // Create a new instance of XxHash64
            byte[] buffer = new byte[8192]; // Allocate an 8KB buffer
            int bytesRead; // Track the number of bytes read

            // Read from stream in chunks
            while ((bytesRead = await file.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                xxHash.Append(buffer.AsSpan(0, bytesRead)); // Use Append instead of Update
            }

            return xxHash.GetCurrentHashAsUInt64();
        }

    }
}
