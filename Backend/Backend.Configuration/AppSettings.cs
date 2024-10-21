using Backend.Security;

namespace Backend.Configuration;

public class AppSettings
{
    public JwtSettings JwtSettings { get; set; }
    public AESSettings AESSettings { get; set; }
}
