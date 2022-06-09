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

        public List<LoginClaims> LoginClaims { set; get; } = null!;

        public string ReturnUrl { set; get; } = "";
    }
}
