using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Infrastructure.Identity.Models
{
    public class AuthenticationResult
    {
        public bool Succeeded { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public UserDto? User { get; set; }
        public string? Error { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public static AuthenticationResult Success(
            string accessToken,
            string refreshToken,
            DateTimeOffset expiresAt,
            UserDto user)
        {
            return new AuthenticationResult
            {
                Succeeded = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = user
            };
        }

        public static AuthenticationResult Failure(string error)
        {
            return new AuthenticationResult
            {
                Succeeded = false,
                Error = error
            };
        }

        public static AuthenticationResult Failure(IEnumerable<string> errors)
        {
            return new AuthenticationResult
            {
                Succeeded = false,
                Errors = errors
            };
        }
    }

}
