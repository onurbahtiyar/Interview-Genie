using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Domain.Entities;

[Table("users")]
public class User
{
    [Column("id")]
    public int Id { get; set; }
    [Column("username")]
    public string Username { get; set; }
    [Column("passwordhash")]
    public string PasswordHash { get; set; }
    [Column("email")]
    public string Email { get; set; }
}
