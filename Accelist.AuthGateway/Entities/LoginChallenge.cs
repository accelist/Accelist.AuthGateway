using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accelist.AuthGateway.Entities
{
    public class LoginChallenge
    {
        [Key]
        public string LoginChallengeID { set; get; } = "";

        public bool IsValid { set; get; }

        public DateTime ValidUntil { set; get; }

        [ForeignKey(nameof(LoginClaims))]
        public string? LoginClaimsID { set; get; }

        public LoginClaims? LoginClaims { set; get; }

        public string ReturnUrl { set; get; } = "";
    }
}
