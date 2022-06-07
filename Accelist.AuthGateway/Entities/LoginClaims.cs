using System.ComponentModel.DataAnnotations;

namespace Accelist.AuthGateway.Entities
{
    public class LoginClaims
    {
        [Key]
        public string LoginClaimsID { set; get; } = "";

        public bool IsValid { set; get; }

        public DateTimeOffset ValidUntil { set; get; }

        public string? Address { set; get; }

        public DateOnly? Birthdate { set; get; }

        public string? Email { set; get; }

        public bool? EmailVerified { set; get; }

        public string? FamilyName { set; get; }

        public string? Gender { set; get; }

        public string? GivenName { set; get; }

        public string? Locale { set; get; }

        public string? MiddleName { set; get; }

        public string? Name { set; get; }

        public string? Nickname { set; get; }

        public string? PhoneNumber { set; get; }

        public bool? PhoneNumberVerified { set; get; }

        public string? Picture { set; get; }

        public string? PreferredUsername { set; get; }

        public string? Profile { set; get; }

        public string Subject { set; get; } = "";

        public DateTimeOffset? UpdatedAt { set; get; }

        public string? Website { set; get; }

        public string? ZoneInfo { set; get; }

        public bool RememberMe { set; get; }

        public List<LoginChallenge> LoginChallenges { set; get; } = null!;
    }
}
