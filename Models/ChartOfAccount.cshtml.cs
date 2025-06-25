using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace MiniAccountsSystem.Models
{
    public class ChartOfAccountsModel : PageModel
    {
        public List<ChartOfAccount> Accounts { get; set; }
        [BindProperty]
        public ChartOfAccount Account { get; set; }
        public SelectList ParentAccountList { get; set; }

        private readonly IConfiguration _config;
        public ChartOfAccountsModel(IConfiguration config)
        {
            _config = config;
        }

        public void OnGet()
        {
            LoadAccounts();
        }

        public void LoadAccounts()
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "Select");

            con.Open();
            using var reader = cmd.ExecuteReader();
            var list = new List<ChartOfAccount>();
            while (reader.Read())
            {
                list.Add(new ChartOfAccount
                {
                    AccountId = (int)reader["AccountId"],
                    AccountName = reader["AccountName"].ToString(),
                    ParentAccountId = reader["ParentAccountId"] as int?,
                    AccountType = reader["AccountType"].ToString()
                });
            }
            Accounts = list;
            ParentAccountList = new SelectList(list, "AccountId", "AccountName");
        }

        public IActionResult OnPost()
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", Account.AccountId > 0 ? "Update" : "Create");
            cmd.Parameters.AddWithValue("@AccountId", Account.AccountId);
            cmd.Parameters.AddWithValue("@AccountName", Account.AccountName);
            cmd.Parameters.AddWithValue("@ParentAccountId", (object)Account.ParentAccountId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountType", Account.AccountType);

            con.Open();
            cmd.ExecuteNonQuery();

            return RedirectToPage();
        }
    }

    public class ChartOfAccount
    {
        [Key]
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public int? ParentAccountId { get; set; }
    }
}
