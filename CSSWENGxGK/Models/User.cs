namespace CSSWENGxGK.Models;
using Microsoft.AspNetCore.Identity;

public class User : IdentityUser<int>
{
    public int user_id { get; set; }
}
