using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorWeb.Models;

namespace App.Admin.Role;

public class RolePageModel : PageModel
{
  protected readonly RoleManager<IdentityRole> _roleManager;
  protected readonly MyBlogContext _context;

  [TempData]
  public string StatusMessage { get; set; }

  public RolePageModel(RoleManager<IdentityRole> roleManager, MyBlogContext context)
  {
    _roleManager = roleManager;
    _context = context;
  }
}
