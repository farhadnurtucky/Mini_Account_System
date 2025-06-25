using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MiniAccountsSystem.Data;
using MiniAccountsSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniAccountsSystem.Areas.Identity.Pages.Admin.chartOfAccount
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<MiniAccountsSystem.Models.ChartOfAccount> ChartOfAccount { get; set; } // This will hold the list of accounts

        public async Task OnGetAsync()
        {
            // Fetch all ChartOfAccounts from the database
            ChartOfAccount = await _context.ChartOfAccounts.ToListAsync();
        }
    }
}
