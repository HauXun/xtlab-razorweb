using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;

namespace App.Pages.Blog
{
  public class EditModel : PageModel
  {
    private readonly App.Models.AppDbContext _context;
    private readonly IAuthorizationService _authorization;

    public EditModel(App.Models.AppDbContext context, IAuthorizationService authorization)
    {
      _context = context;
      _authorization = authorization;
    }

    [BindProperty]
    public Article Article { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null || _context.articles == null)
      {
        return Content("Không thấy bài viết");
      }

      var article = await _context.articles.FirstOrDefaultAsync(m => m.Id == id);
      if (article == null)
      {
        return Content("Không thấy bài viết");
      }
      Article = article;
      return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid)
      {
        return Page();
      }

      _context.Attach(Article).State = EntityState.Modified;

      try
      {
        var canupdate = await _authorization.AuthorizeAsync(this.User, Article, "CanUpdateArticle");
        if (canupdate.Succeeded)
        {
          await _context.SaveChangesAsync();
        }
        else
        {
          return Content("Không được quyền truy cập");
        }
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!ArticleExists(Article.Id))
        {
          return Content("Không thấy bài viết");
        }
        else
        {
          throw;
        }
      }

      return RedirectToPage("./Index");
    }

    private bool ArticleExists(int id)
    {
      return (_context.articles?.Any(e => e.Id == id)).GetValueOrDefault();
    }
  }
}
