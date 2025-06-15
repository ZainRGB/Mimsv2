using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Mimsv2.Models;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mimsv2.Controllers
{
    public class FormController : Controller
    {
        private readonly IConfiguration _configuration;

        public FormController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> IncidentForm()
        {
            var model = new FormModel();

            // Populate session values
            model.CapturedByLoginName = HttpContext.Session.GetString("loginname") ?? "Unknown";
            model.CapturedByName = HttpContext.Session.GetString("username") ?? "Unknown";
            model.CapturedBySurname = HttpContext.Session.GetString("surname") ?? "Unknown";
            model.CapturedByTitle = HttpContext.Session.GetString("titles") ?? "Unknown";
            model.CapturedByEmail = HttpContext.Session.GetString("email") ?? "Unknown";
            model.HospitalId = HttpContext.Session.GetString("hospitalid") ?? "0";

            await PopulateDropdowns(model);
            return View(model);
        }

        private async Task PopulateDropdowns(FormModel model)
        {
            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            // 1. Departments Dropdown
            string deptSql = "SELECT department FROM tbldepartments WHERE description = 'ward' ORDER BY department";
            using var deptCmd = new NpgsqlCommand(deptSql, conn);
            using var deptReader = await deptCmd.ExecuteReaderAsync();

            model.Departments = new List<SelectListItem>();
            while (await deptReader.ReadAsync())
            {
                model.Departments.Add(new SelectListItem
                {
                    Value = deptReader["department"].ToString(),
                    Text = deptReader["department"].ToString()
                });
            }
            await deptReader.CloseAsync();

            model.IncidentCategories = await GetDistinctCategories(); // cat

            if (!string.IsNullOrEmpty(model.inctypescat1))
            {
                model.IncidentSubCat1 = await GetSubCategories(1, model.inctypescat1);
            }

            if (!string.IsNullOrEmpty(model.inctypescat2))
            {
                model.IncidentSubCat2 = await GetSubCategories(2, model.inctypescat2);
            }

            if (!string.IsNullOrEmpty(model.inctypescat3))
            {
                model.IncidentSubCat3 = await GetSubCategories(3, model.inctypescat3);
            }


            // 2. Add another dropdown list here (Example: Locations)

            string locationSql = "SELECT hospitalid, hospital FROM tblhospitals WHERE active = 'Y' ORDER BY hospitalid";
            using var locationCmd = new NpgsqlCommand(locationSql, conn);
            using var locationReader = await locationCmd.ExecuteReaderAsync();
            
            model.Hospitals = new List<SelectListItem>();
            while (await locationReader.ReadAsync())
            {
                model.Hospitals.Add(new SelectListItem
                {
                    Value = locationReader["hospitalid"].ToString(),
                    Text = locationReader["hospital"].ToString()
                });
            }
            await locationReader.CloseAsync();
            

            // 3. Add another dropdown list here (Example: Incident Types)
            /*
            string incidentTypeSql = "SELECT code, description FROM tblIncidentTypes ORDER BY description";
            using var incidentTypeCmd = new NpgsqlCommand(incidentTypeSql, conn);
            using var incidentTypeReader = await incidentTypeCmd.ExecuteReaderAsync();
            
            model.IncidentTypes = new List<SelectListItem>();
            while (await incidentTypeReader.ReadAsync())
            {
                model.IncidentTypes.Add(new SelectListItem
                {
                    Value = incidentTypeReader["code"].ToString(),
                    Text = incidentTypeReader["description"].ToString()
                });
            }
            await incidentTypeReader.CloseAsync();
            */

            // 4. Add more dropdown lists as needed following the same pattern
        }


        //SECTION A
        [HttpGet]
        public async Task<IActionResult> GetDepartmentsByHospital(int hospitalId)
        {
            try
            {
                var departments = new List<SelectListItem>();

                using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();

                string sql = "SELECT department FROM tbldepartments WHERE description = 'ward' AND hospitalid = @hid ORDER BY department";
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@hid", hospitalId.ToString()); // FIXED HERE

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    departments.Add(new SelectListItem
                    {
                        Value = reader["department"].ToString(),
                        Text = reader["department"].ToString()
                    });
                }

                return Json(departments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to load departments", details = ex.Message });
            }
        }



        //SECTION B
        [HttpGet]
        public async Task<IActionResult> GetIncidentCategories()
        {
            var list = new List<SelectListItem>();

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = "SELECT DISTINCT cat FROM tblincidenttype WHERE active = 'Y' ORDER BY cat";
            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var cat = reader["cat"].ToString();
                if (!string.IsNullOrWhiteSpace(cat))
                {
                    list.Add(new SelectListItem { Value = cat, Text = cat });
                }
            }

            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategory1(string cat)
        {
            var list = new List<SelectListItem>();

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = "SELECT DISTINCT subcat1 FROM tblincidenttype WHERE active = 'Y' AND cat = @cat ORDER BY subcat1";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("cat", cat ?? "");

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var val = reader["subcat1"].ToString();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    list.Add(new SelectListItem { Value = val, Text = val });
                }
            }

            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategory2(string sub1)
        {
            var list = new List<SelectListItem>();

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = "SELECT DISTINCT subcat2 FROM tblincidenttype WHERE active = 'Y' AND subcat1 = @sub1 ORDER BY subcat2";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("sub1", sub1 ?? "");

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var val = reader["subcat2"].ToString();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    list.Add(new SelectListItem { Value = val, Text = val });
                }
            }

            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategory3(string sub2)
        {
            var list = new List<SelectListItem>();

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = "SELECT DISTINCT subcat3 FROM tblincidenttype WHERE active = 'Y' AND subcat2 = @sub2 ORDER BY subcat3";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("sub2", sub2 ?? "");

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var val = reader["subcat3"].ToString();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    list.Add(new SelectListItem { Value = val, Text = val });
                }
            }

            return Json(list);
        }
        // Load all categories



private async Task<List<SelectListItem>> GetDistinctCategories()
{
    var result = new List<SelectListItem>();
    using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    await conn.OpenAsync();

    var cmd = new NpgsqlCommand("SELECT DISTINCT cat FROM tblincidenttype WHERE active = 'Y' ORDER BY cat", conn);
    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        result.Add(new SelectListItem
        {
            Value = reader["cat"].ToString(),
            Text = reader["cat"].ToString()
        });
    }
    return result;
}

private async Task<List<SelectListItem>> GetSubCategories(int level, string parentValue)
{
    string field = level switch
    {
        1 => "subcat1",
        2 => "subcat2",
        3 => "subcat3",
        _ => throw new ArgumentException("Invalid level")
    };

    string parentField = level switch
    {
        1 => "cat",
        2 => "subcat1",
        3 => "subcat2",
        _ => throw new ArgumentException("Invalid level")
    };

    var result = new List<SelectListItem>();
    using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    await conn.OpenAsync();

    var sql = $"SELECT DISTINCT {field} FROM tblincidenttype WHERE {parentField} = @parent AND active = 'Y' ORDER BY {field}";
    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("parent", parentValue);

    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        result.Add(new SelectListItem
        {
            Value = reader[field].ToString(),
            Text = reader[field].ToString()
        });
    }

    return result;
}


        //SECTION B END



        //SECTION C
        //[HttpGet]
        //public async Task<IActionResult> GetUsers(int hospitalId)
        //{
        //    try
        //    {
        //        var loginname = new List<SelectListItem>();

        //        using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        //        await conn.OpenAsync();

        //        string sql = "SELECT * FROM tblusers WHERE active = 'Y' AND hospitalid = @hid ORDER BY username";
        //        using var cmd = new NpgsqlCommand(sql, conn);
        //        cmd.Parameters.AddWithValue("@hid", hospitalId.ToString()); // FIXED HERE

        //        using var reader = await cmd.ExecuteReaderAsync();
        //        while (await reader.ReadAsync())
        //        {
        //            loginname.Add(new SelectListItem
        //            {
        //                Value = reader["loginname"].ToString(),
        //                Text = reader["username"].ToString()
        //            });
        //        }

        //        return Json(loginname);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { error = "Failed to load user", details = ex.Message });
        //    }
        //}



        [HttpGet]
        public async Task<IActionResult> GetInvestigators(int hospitalId)
        {
            var list = new List<SelectListItem>();

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = @"SELECT * FROM tblusers
                   WHERE hospitalid = @hospitalId AND active = 'Y'
                   ORDER BY username, surname";

            using var cmd = new NpgsqlCommand(sql, conn);
            //cmd.Parameters.AddWithValue("@hospitalId", hospitalId);
            cmd.Parameters.AddWithValue("@hospitalId", hospitalId.ToString());

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string loginname = reader["loginname"].ToString();
                string fullName = $"{reader["username"]} {reader["surname"]}";
                list.Add(new SelectListItem { Value = loginname, Text = fullName });
            }

            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetInvestigatorDetails(string loginname)
        {
            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = @"SELECT * FROM tblusers
                   WHERE loginname = @loginname AND active = 'Y'";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@loginname", loginname);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Json(new
                {
                    username = reader["username"].ToString(),
                    surname = reader["surname"].ToString(),
                    email = reader["email"].ToString()
                });
            }

            return Json(null);
        }



        //SECTION C END






        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForm(FormModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View("IncidentForm", model);
            }

            // Form submission logic goes here

            return RedirectToAction("Success");
        }
    }
}