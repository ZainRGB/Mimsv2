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
            model.CapturedbyDpt = HttpContext.Session.GetString("department") ?? "Unknown";
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



        //SECTION D
        [HttpGet]
        public async Task<IActionResult> GetEmp()
        {
            var list = new List<SelectListItem>();

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = @"SELECT Distinct dpt FROM tblempcat
                   WHERE active = 'Y'
                   ORDER BY dpt";

            using var cmd = new NpgsqlCommand(sql, conn);


            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string dpt = reader["dpt"].ToString();
                string fullName = $"{reader["dpt"]}";
                list.Add(new SelectListItem { Value = dpt, Text = fullName });
            }

            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmpDetails(string dpt)
        {
            var list = new List<SelectListItem>();

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = @"SELECT dptcat FROM tblempcat
                   WHERE dpt = @dpt AND active = 'Y'
                   ORDER BY dptcat";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dpt", dpt);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string dptcat = reader["dptcat"].ToString();
                list.Add(new SelectListItem { Value = dptcat, Text = dptcat });
            }

            return Json(list);
        }


        //SECTION D END






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


        //INPUT FORM
        [HttpPost]
        public async Task<IActionResult> IncidentFormInput(FormModel model)
        {
          //  if (!ModelState.IsValid)
          //  {
                // You can reload dropdowns here if needed
          //      return View(model);
          //  }

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = @"INSERT INTO tblincident (
                    affectedward, 
                    incidentarea, 
                    incidentcriteria, 
                    incidentcriteriasub, 
                    requester,
                    requesteremail, 
                    priority, 
                    titles, 
                    reportedby, 
                    invesitgatedby, 
                    assignedcat, 
                    assignedstaff, 
                    incidentdate, 
                    incidenttime, 
                    datereported, 
                    datecaptured, 
                    summary, 
                    description, 
                    timecaptured, 
                    active, 
                    status, 
                    hospitalid, 
                    qarid, 
                    username, 
                    surname, 
                    onholddescdate, 
                    onholddesctime, 
                    closeddesctime, 
                    onholddesc, 
                    closeddesc, 
                    pte, 
                    ptenumber, 
                    ptename, 
                    ptesurname, 
                    ptetitle, 
                    reportedbyemail, 
                    correctaction, 
                    correctactiontime, 
                    preventaction, 
                    preventactiontime, 
                    preventactiondate, 
                    correctactiondate, 
                    investigation, 
                    summary2, 
                    medrelatedtotal, 
                    reportedbydepartment, 
                    incidentexpires, 
                    incidentareanight, 
                    acquired, 
                    incidenttype, 
                    inctypescat1, 
                    inctypescat2
                   
                ) VALUES (
                     @affectedward, 
                    @incidentarea, 
                    @incidentcriteria, 
                    @incidentcriteriasub, 
                    @requester,  
                    @requesteremail, 
                    @priority, 
                    @titles, 
                    @reportedby, 
                    @invesitgatedby, 
                    @assignedcat, 
                    @assignedstaff, 
                    @incidentdate, 
                    @incidenttime, 
                    @datereported, 
                    @datecaptured, 
                    @summary, 
                    @description, 
                    @timecaptured, 
                    @active, 
                    @status, 
                    @hospitalid, 
                    @qarid, 
                    @username, 
                    @surname, 
                    @onholddescdate, 
                    @onholddesctime, 
                    @closeddesctime, 
                    @onholddesc, 
                    @closeddesc, 
                    @pte, 
                    @ptenumber, 
                    @ptename, 
                    @ptesurname, 
                    @ptetitle, 
                    @reportedbyemail, 
                    @correctaction, 
                    @correctactiontime, 
                    @preventaction, 
                    @preventactiontime, 
                    @preventactiondate, 
                    @correctactiondate, 
                    @investigation, 
                    @summary2, 
                    @medrelatedtotal, 
                    @reportedbydepartment, 
                    @incidentexpires, 
                    @incidentareanight, 
                    @acquired, 
                    @incidenttype, 
                    @inctypescat1, 
                    @inctypescat2
            )
            RETURNING id;
               ";

            int insertedId;
            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@affectedward", model.affectedward ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@incidentarea", model.incidentarea ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@incidentcriteria", model.incidentcriteria ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@incidentcriteriasub", model.incidentcriteriasub ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@requester", model.CapturedByLoginName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@requesteremail", model.CapturedByEmail ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@priority", model.priority ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@titles", model.CapturedByTitle ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@reportedby", model.CapturedByLoginName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@invesitgatedby", model.invesitgatedby ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@assignedcat", model.assignedcat ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@assignedstaff", model.assignedstaff ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@incidentdate", model.incidentdate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@incidenttime", model.incidenttime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@datereported", model.datereported ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@datecaptured", model.datecaptured?.Date ?? DateTime.Today);

            cmd.Parameters.AddWithValue("@summary", model.summary ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@description", model.description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@timecaptured", string.IsNullOrWhiteSpace(model.timecaptured) ? DateTime.Now.ToString("HH:mm") : model.timecaptured);

            cmd.Parameters.AddWithValue("@active", model.active ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@status", model.status ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@hospitalid", model.hospitalid ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@qarid", model.qarid ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@username", model.CapturedByName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@surname", model.CapturedBySurname ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@onholddescdate", model.onholddescdate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@closeddescdate", model.closeddescdate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@onholddesctime", model.onholddesctime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@closeddesctime", model.closeddesctime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@onholddesc", model.onholddesc ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@closeddesc", model.closeddesc ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@pte", model.pte ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ptenumber", model.ptenumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ptename", model.ptename ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ptesurname", model.ptesurname ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ptetitle", model.ptetitle ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@reportedbyemail", model.CapturedByEmail ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@correctaction", model.correctaction ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@correctactiontime", model.correctactiontime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@preventaction", model.preventaction ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@preventactiontime", model.preventactiontime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@preventactiondate", model.preventactiondate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@correctactiondate", model.correctactiondate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@investigation", model.investigation ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@summary2", model.summary2 ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@medrelatedtotal", model.medrelatedtotal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@reportedbydepartment", model.CapturedbyDpt ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@incidentexpires", DateTime.Now.Date.AddDays(5));
            cmd.Parameters.AddWithValue("@incidentareanight", model.incidentareanight ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@acquired", model.acquired ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@incidenttype", model.incidenttype ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@inctypescat1", model.inctypescat1 ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@inctypescat2", model.inctypescat2 ?? (object)DBNull.Value);
            insertedId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            //cmd.Parameters.AddWithValue("@incidentarea", model.incidentarea);
            //cmd.Parameters.AddWithValue("@incidentareanight", model.incidentareanight);

            // Add remaining parameters here...

            //await cmd.ExecuteNonQueryAsync();

            string abbr = "";
            string abbrSql = "SELECT abbr FROM tblhospitals WHERE hospitalid = @hospitalid";
            using (var abbrCmd = new NpgsqlCommand(abbrSql, conn))
            {
                abbrCmd.Parameters.AddWithValue("@hospitalid", model.hospitalid); // still string
                abbr = (await abbrCmd.ExecuteScalarAsync())?.ToString() ?? "XX"; // fallback if not found
            }

            var now = DateTime.Now;
            string qarid = $"{abbr}/{now.Day}/{now.Month}/{insertedId}";

            string updateSql = "UPDATE tblincident SET qarid = @qarid WHERE id = @id";
            using (var updateCmd = new NpgsqlCommand(updateSql, conn))
            {
                updateCmd.Parameters.AddWithValue("@qarid", qarid);
                updateCmd.Parameters.AddWithValue("@id", insertedId);
                await updateCmd.ExecuteNonQueryAsync();
            }

            // Optional: Redirect or show a success message
            return RedirectToAction("IncidentFormConfirmation");
        }

        public IActionResult IncidentFormConfirmation()
        {
            return View(); // Create a simple confirmation view if you want
        }


    }
}