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
    public List<IdentityRole> roles { get; set; }

    public IndexModel(RoleManager<IdentityRole> roleManager, MyBlogContext context) : base(roleManager, context)
    {
    }

    public async Task OnGetAsync()
    {
      roles = await _roleManager.Roles.OrderByDescending(r => r.Name).ToListAsync();
    }

    public void OnPost() => RedirectToPage();
  }
}
