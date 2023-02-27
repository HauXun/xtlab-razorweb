using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorWeb.Models;

namespace App.Admin.Role
{
  [Authorize(Roles = "Administrator")]
  public class IndexModel : RolePageModel
  {
    public List<RoleModel> roles { get; set; }

    public class RoleModel : IdentityRole
    {
      public string[] Claims { get; init; }
    }

    public IndexModel(RoleManager<IdentityRole> roleManager, MyBlogContext context) : base(roleManager, context)
    {
    }

    public async Task OnGetAsync()
    {
      var r = await _roleManager.Roles.OrderByDescending(r => r.Name).ToListAsync();
      roles = new List<RoleModel>();
      foreach (var role in r)
      {
        var claims = await _roleManager.GetClaimsAsync(role);
        var claimsString = claims.Select(c => $"{c.Type}={c.Value}");
        var roleModel = new RoleModel
        {
          Id = role.Id,
          Name = role.Name,
          Claims = claimsString.ToArray()
        };
        roles.Add(roleModel);
      }
    }

    public void OnPost() => RedirectToPage();
  }
}
