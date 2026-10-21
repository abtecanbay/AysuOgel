using System.Security.Cryptography;
using System.Text;

namespace AysuOgel.Web.Helpers;

/// <summary>Üye (seans sorgulama) oturumu sabitleri.</summary>
public static class MemberAuth
{
    public const string Scheme = "Member";

    /// <summary>Şifre değişince eski oturumların geçersiz olması için çerezde tutulan parmak izi.</summary>
    public const string PasswordStampClaim = "pwd_stamp";

    public static string PasswordStamp(string passwordHash) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash)))[..16];
}
