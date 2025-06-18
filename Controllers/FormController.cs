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

            model.AccessLevel = HttpContext.Session.GetString("accessLevel") ?? "User";

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
        RETURNING id;";

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

            // Build QARID
            string abbr = "";
            string abbrSql = "SELECT abbr FROM tblhospitals WHERE hospitalid = @hospitalid";
            using (var abbrCmd = new NpgsqlCommand(abbrSql, conn))
            {
                abbrCmd.Parameters.AddWithValue("@hospitalid", model.hospitalid);
                abbr = (await abbrCmd.ExecuteScalarAsync())?.ToString() ?? "XX";
            }

            var now = DateTime.Now;
            //string qarid = $"{abbr}/{now.Day}/{now.Month}/{insertedId}";
            string qarid = $"{abbr}-{now.Day}-{now.Month}-{insertedId}";

            string updateSql = "UPDATE tblincident SET qarid = @qarid WHERE id = @id";
            using (var updateCmd = new NpgsqlCommand(updateSql, conn))
            {
                updateCmd.Parameters.AddWithValue("@qarid", qarid);
                updateCmd.Parameters.AddWithValue("@id", insertedId);
                await updateCmd.ExecuteNonQueryAsync();
            }

            if (model.UploadedFiles != null && model.UploadedFiles.Any())
            {
                foreach (var filename in model.UploadedFiles)
                {
                    string fileUpdateSql = @"
                UPDATE incidentattachments
                SET qarid = @qarid
                WHERE attachment = @filename AND (qarid IS NULL OR qarid = '' OR qarid = '0')";
                    using var fileCmd = new NpgsqlCommand(fileUpdateSql, conn);
                    fileCmd.Parameters.AddWithValue("@qarid", qarid);
                    fileCmd.Parameters.AddWithValue("@filename", filename);
                    await fileCmd.ExecuteNonQueryAsync();
                }
            }

            //  Final redirect to edit mode using qarid
            //return RedirectToAction("EditIncident", new { id = qarid });
            return RedirectToAction("EditIncident", "Form", new { id = qarid });
        }

        //INPUT END

        //EDIT INCIDENT
        //[HttpGet]
        //public async Task<IActionResult> EditIncident(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        return NotFound();
        //    }

        //    var model = new FormModel();

        //    using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        //    await conn.OpenAsync();

        //    string sql = @"SELECT * FROM tblincident WHERE qarid = @qarid LIMIT 1";
        //    using var cmd = new NpgsqlCommand(sql, conn);
        //    cmd.Parameters.AddWithValue("@qarid", id);

        //    using var reader = await cmd.ExecuteReaderAsync();
        //    if (await reader.ReadAsync())
        //    {
        //        model.qarid = reader["qarid"]?.ToString();
        //        model.affectedward = reader["affectedward"]?.ToString();
        //        cmd.Parameters.AddWithValue("@incidentarea", (object)model.incidentcriteria ?? DBNull.Value);
        //        model.incidentcriteria = reader["incidentcriteria"]?.ToString();
        //        model.incidentcriteriasub = reader["incidentcriteriasub"]?.ToString();
        //        model.CapturedByLoginName = reader["requester"]?.ToString();
        //        model.CapturedByEmail = reader["requesteremail"]?.ToString();
        //        model.priority = reader["priority"]?.ToString();
        //        model.CapturedByTitle = reader["titles"]?.ToString();
        //        model.invesitgatedby = reader["invesitgatedby"]?.ToString();
        //        model.assignedcat = reader["assignedcat"]?.ToString();
        //        model.assignedstaff = reader["assignedstaff"]?.ToString();
        //        model.incidentdate = reader["incidentdate"] as DateTime?;
        //        model.incidenttime = reader["incidenttime"]?.ToString();
        //        model.datereported = reader["datereported"] as DateTime?;
        //        model.datecaptured = reader["datecaptured"] as DateTime?;
        //        model.summary = reader["summary"]?.ToString();
        //        model.description = reader["description"]?.ToString();
        //        model.timecaptured = reader["timecaptured"]?.ToString();
        //        model.active = reader["active"]?.ToString();
        //        model.status = reader["status"]?.ToString();
        //        model.hospitalid = reader["hospitalid"]?.ToString();
        //        model.CapturedByName = reader["username"]?.ToString();
        //        model.CapturedBySurname = reader["surname"]?.ToString();
        //        model.onholddescdate = reader["onholddescdate"] as DateTime?;
        //        model.closeddescdate = reader["closeddescdate"] as DateTime?;
        //        model.onholddesctime = reader["onholddesctime"]?.ToString();
        //        model.closeddesctime = reader["closeddesctime"]?.ToString();
        //        model.onholddesc = reader["onholddesc"]?.ToString();
        //        model.closeddesc = reader["closeddesc"]?.ToString();
        //        model.pte = reader["pte"]?.ToString();
        //        model.ptenumber = reader["ptenumber"]?.ToString();
        //        model.ptename = reader["ptename"]?.ToString();
        //        model.ptesurname = reader["ptesurname"]?.ToString();
        //        model.ptetitle = reader["ptetitle"]?.ToString();
        //        model.correctaction = reader["correctaction"]?.ToString();
        //        model.correctactiontime = reader["correctactiontime"]?.ToString();
        //        model.preventaction = reader["preventaction"]?.ToString();
        //        model.preventactiontime = reader["preventactiontime"]?.ToString();
        //        model.preventactiondate = reader["preventactiondate"] as DateTime?;
        //        model.correctactiondate = reader["correctactiondate"] as DateTime?;
        //        model.investigation = reader["investigation"]?.ToString();
        //        model.summary2 = reader["summary2"]?.ToString();
        //        model.medrelatedtotal = reader["medrelatedtotal"]?.ToString();
        //        model.CapturedbyDpt = reader["reportedbydepartment"]?.ToString();
        //        cmd.Parameters.AddWithValue("@incidentareanight", (object)model.incidentareanight ?? DBNull.Value);
        //        model.acquired = reader["acquired"]?.ToString();
        //        model.incidenttype = reader["incidenttype"]?.ToString();
        //        model.inctypescat1 = reader["inctypescat1"]?.ToString();
        //        model.inctypescat2 = reader["inctypescat2"]?.ToString();
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }

        //    /*return View("IncidentFormInput", model);*/ // or a separate edit view if you have one
           
        //    await PopulateDropdowns(model);
        //    return View("IncidentForm", model);
        //}

        //EDIT INCIDENT END














        //ADDENDUMS
        //[HttpPost]
        //public async Task<IActionResult> UploadAddendum(int qarid, string addendumType, string? otherAddendum, IFormFile addendumFile, string inctypescat2)
        //{
        //    if (addendumFile == null || addendumFile.Length == 0)
        //        return BadRequest("No file uploaded.");

        //    var addendumName = addendumType == "Other" ? otherAddendum : addendumType;

        //    using var ms = new MemoryStream();
        //    await addendumFile.CopyToAsync(ms);
        //    var fileBytes = ms.ToArray();

        //    using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        //    await conn.OpenAsync();

        //    var cmd = new NpgsqlCommand(@"
        //INSERT INTO incidentattachments 
        //(attchatime, attachment, active, qarid, attachdate, addendums, criteria) 
        //VALUES (CURRENT_TIME, @attachment, 'Y', @qarid, CURRENT_DATE, @addendums, @criteria)", conn);

        //    cmd.Parameters.AddWithValue("attachment", fileBytes);
        //    cmd.Parameters.AddWithValue("qarid", qarid);
        //    cmd.Parameters.AddWithValue("addendums", addendumName ?? "");
        //    cmd.Parameters.AddWithValue("criteria", inctypescat2 ?? "");

        //    await cmd.ExecuteNonQueryAsync();

        //    TempData["UploadSuccess"] = "Addendum uploaded successfully.";
        //    return RedirectToAction("IncidentForm"); // adjust if needed
        //}




        [HttpPost]
        public async Task<IActionResult> UploadAddendum(IFormFile addendumFile, string addendumType, string criteria)
        {
            try
            {
                if (addendumFile == null || addendumFile.Length == 0 || string.IsNullOrEmpty(addendumType) || string.IsNullOrEmpty(criteria))
                {
                    return BadRequest("Missing file or required metadata.");
                }

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(addendumFile.FileName)}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await addendumFile.CopyToAsync(stream);
                }

                using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();

                string insertSql = @"
            INSERT INTO incidentattachments 
            (attachtime, attachment, active, qarid, attachdate,foldername, addendums, criteria)
            VALUES (@attchatime, @attachment, 'Y', @qarid, @attachdate,@foldername, @addendums, @criteria)";

                using var cmd = new NpgsqlCommand(insertSql, conn);
                cmd.Parameters.AddWithValue("attchatime", DateTime.Now);
                cmd.Parameters.AddWithValue("attachment", uniqueFileName);
                cmd.Parameters.AddWithValue("qarid", 0); // temp ID
                cmd.Parameters.AddWithValue("attachdate", DateTime.Today);
                cmd.Parameters.AddWithValue("addendums", addendumType);
                cmd.Parameters.AddWithValue("criteria", criteria);
                cmd.Parameters.AddWithValue("foldername", Path.GetFileName(addendumFile.FileName));

                await cmd.ExecuteNonQueryAsync();

                return Ok(new
                {
                    message = "File uploaded successfully.",
                    attachment = uniqueFileName,
                   
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //[HttpDelete]
        //public async Task<IActionResult> DeleteAddendum(string fileName)
        //{
        //    if (string.IsNullOrWhiteSpace(fileName))
        //        return BadRequest("Invalid filename.");

        //    using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        //    await conn.OpenAsync();

        //    // First, find the record
        //    string selectSql = "SELECT id, attachment FROM incidentattachments WHERE foldername = @foldername AND active = 'Y' LIMIT 1";
        //    int id = 0;
        //    string savedFileName = "";

        //    using (var cmd = new NpgsqlCommand(selectSql, conn))
        //    {
        //        cmd.Parameters.AddWithValue("foldername", fileName);
        //        using var reader = await cmd.ExecuteReaderAsync();
        //        if (await reader.ReadAsync())
        //        {
        //            id = reader.GetInt32(0);
        //            savedFileName = reader.GetString(1);
        //        }
        //        else
        //        {
        //            return NotFound("File not found in database.");
        //        }
        //    }

        //    // Soft delete from DB
        //    string updateSql = "UPDATE incidentattachments SET active = 'N' WHERE id = @id";
        //    using (var cmd = new NpgsqlCommand(updateSql, conn))
        //    {
        //        cmd.Parameters.AddWithValue("id", id);
        //        await cmd.ExecuteNonQueryAsync();
        //    }

        //    // Delete from disk
        //    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", savedFileName);
        //    if (System.IO.File.Exists(filePath))
        //    {
        //        System.IO.File.Delete(filePath);
        //    }

        //    return Ok("Addendum deleted successfully.");
        //}

        [HttpPost]
        public async Task<IActionResult> DeleteAddendum([FromForm] string attachment)
        {
            if (string.IsNullOrWhiteSpace(attachment))
                return BadRequest("Missing attachment filename.");

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = @"UPDATE incidentattachments 
                   SET active = 'N' 
                   WHERE attachment = @attachment";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("attachment", attachment);
            var rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return NotFound("No matching record found.");

            return Ok("File deleted.");
        }
        //ADDENDUMS END





        //NEW INCIDENT EDIT
        [HttpGet]
        public async Task<IActionResult> EditIncident(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var model = new FormModel();

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = @"SELECT * FROM tblincident WHERE qarid = @qarid LIMIT 1";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@qarid", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                //model.qarid = reader["qarid"]?.ToString();
                //model.affectedward = reader["affectedward"]?.ToString();
                //model.incidentcriteria = reader["incidentcriteria"]?.ToString();
                //model.incidentcriteriasub = reader["incidentcriteriasub"]?.ToString();
                //model.CapturedByLoginName = reader["requester"]?.ToString();
                //model.CapturedByEmail = reader["requesteremail"]?.ToString();
                //model.priority = reader["priority"]?.ToString();
                //model.CapturedByTitle = reader["titles"]?.ToString();
                //model.invesitgatedby = reader["invesitgatedby"]?.ToString();
                //model.assignedcat = reader["assignedcat"]?.ToString();
                //model.assignedstaff = reader["assignedstaff"]?.ToString();
                //model.incidentdate = reader["incidentdate"] as DateTime?;
                //model.incidenttime = reader["incidenttime"]?.ToString();
                //model.datereported = reader["datereported"] as DateTime?;
                //model.datecaptured = reader["datecaptured"] as DateTime?;
                //model.summary = reader["summary"]?.ToString();
                //model.description = reader["description"]?.ToString();
                //model.timecaptured = reader["timecaptured"]?.ToString();
                //model.active = reader["active"]?.ToString();
                //model.status = reader["status"]?.ToString();
                //model.hospitalid = reader["hospitalid"]?.ToString();
                //model.CapturedByName = reader["username"]?.ToString();
                //model.CapturedBySurname = reader["surname"]?.ToString();
                //model.onholddescdate = reader["onholddescdate"] as DateTime?;
                //model.closeddescdate = reader["closeddescdate"] as DateTime?;
                //model.onholddesctime = reader["onholddesctime"]?.ToString();
                //model.closeddesctime = reader["closeddesctime"]?.ToString();
                //model.onholddesc = reader["onholddesc"]?.ToString();
                //model.closeddesc = reader["closeddesc"]?.ToString();
                //model.pte = reader["pte"]?.ToString();
                //model.ptenumber = reader["ptenumber"]?.ToString();
                //model.ptename = reader["ptename"]?.ToString();
                //model.ptesurname = reader["ptesurname"]?.ToString();
                //model.ptetitle = reader["ptetitle"]?.ToString();
                //model.correctaction = reader["correctaction"]?.ToString();
                //model.correctactiontime = reader["correctactiontime"]?.ToString();
                //model.preventaction = reader["preventaction"]?.ToString();
                //model.preventactiontime = reader["preventactiontime"]?.ToString();
                //model.preventactiondate = reader["preventactiondate"] as DateTime?;
                //model.correctactiondate = reader["correctactiondate"] as DateTime?;
                //model.investigation = reader["investigation"]?.ToString();
                //model.summary2 = reader["summary2"]?.ToString();
                //model.medrelatedtotal = reader["medrelatedtotal"]?.ToString();
                //model.CapturedbyDpt = reader["reportedbydepartment"]?.ToString();
                //model.acquired = reader["acquired"]?.ToString();
                //model.incidenttype = reader["incidenttype"]?.ToString();
                //model.inctypescat1 = reader["inctypescat1"]?.ToString();
                //model.inctypescat2 = reader["inctypescat2"]?.ToString();
                //model.inctypescat3 = reader["inctypescat3"]?.ToString();
                //model.IsDayShift = reader["dayshift"]?.ToString() == "Y";
                //model.IsNightShift = reader["nightshift"]?.ToString() == "Y";


                model.qarid = reader["qarid"]?.ToString();
                model.affectedward = reader["affectedward"]?.ToString();
                cmd.Parameters.AddWithValue("@incidentarea", (object)model.incidentcriteria ?? DBNull.Value);
                model.incidentcriteria = reader["incidentcriteria"]?.ToString();
                model.incidentcriteriasub = reader["incidentcriteriasub"]?.ToString();
                model.CapturedByLoginName = reader["requester"]?.ToString();
                model.CapturedByEmail = reader["requesteremail"]?.ToString();
                model.priority = reader["priority"]?.ToString();
                model.CapturedByTitle = reader["titles"]?.ToString();
                model.invesitgatedby = reader["invesitgatedby"]?.ToString();
                model.assignedcat = reader["assignedcat"]?.ToString();
                model.assignedstaff = reader["assignedstaff"]?.ToString();
                model.incidentdate = reader["incidentdate"] as DateTime?;
                model.incidenttime = reader["incidenttime"]?.ToString();
                model.datereported = reader["datereported"] as DateTime?;
                model.datecaptured = reader["datecaptured"] as DateTime?;
                model.summary = reader["summary"]?.ToString();
                model.description = reader["description"]?.ToString();
                model.timecaptured = reader["timecaptured"]?.ToString();
                model.active = reader["active"]?.ToString();
                model.status = reader["status"]?.ToString();
                model.hospitalid = reader["hospitalid"]?.ToString();
                model.CapturedByName = reader["username"]?.ToString();
                model.CapturedBySurname = reader["surname"]?.ToString();
                model.onholddescdate = reader["onholddescdate"] as DateTime?;
                model.closeddescdate = reader["closeddescdate"] as DateTime?;
                model.onholddesctime = reader["onholddesctime"]?.ToString();
                model.closeddesctime = reader["closeddesctime"]?.ToString();
                model.onholddesc = reader["onholddesc"]?.ToString();
                model.closeddesc = reader["closeddesc"]?.ToString();
                model.pte = reader["pte"]?.ToString();
                model.ptenumber = reader["ptenumber"]?.ToString();
                model.ptename = reader["ptename"]?.ToString();
                model.ptesurname = reader["ptesurname"]?.ToString();
                model.ptetitle = reader["ptetitle"]?.ToString();
                model.correctaction = reader["correctaction"]?.ToString();
                model.correctactiontime = reader["correctactiontime"]?.ToString();
                model.preventaction = reader["preventaction"]?.ToString();
                model.preventactiontime = reader["preventactiontime"]?.ToString();
                model.preventactiondate = reader["preventactiondate"] as DateTime?;
                model.correctactiondate = reader["correctactiondate"] as DateTime?;
                model.investigation = reader["investigation"]?.ToString();
                model.summary2 = reader["summary2"]?.ToString();
                model.medrelatedtotal = reader["medrelatedtotal"]?.ToString();
                model.CapturedbyDpt = reader["reportedbydepartment"]?.ToString();
                cmd.Parameters.AddWithValue("@incidentareanight", (object)model.incidentareanight ?? DBNull.Value);
                model.acquired = reader["acquired"]?.ToString();
                model.incidenttype = reader["incidenttype"]?.ToString();
                model.inctypescat1 = reader["inctypescat1"]?.ToString();
                model.inctypescat2 = reader["inctypescat2"]?.ToString();

            }

            await PopulateDropdowns(model); // your existing dropdown helper

            return View("IncidentEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditIncident(FormModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View("IncidentEdit", model);
            }

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = @"UPDATE tblincident SET
                    affectedward = @affectedward,
                    incidentcriteria = @incidentcriteria,
                    incidentcriteriasub = @incidentcriteriasub,
                    assignedcat = @assignedcat,
                    assignedstaff = @assignedstaff,
                    summary = @summary,
                    description = @description,
                    dayshift = @dayshift,
                    nightshift = @nightshift,
                    status = @status
                WHERE qarid = @qarid";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@qarid", model.qarid ?? "");
            cmd.Parameters.AddWithValue("@affectedward", model.affectedward ?? "");
            cmd.Parameters.AddWithValue("@incidentcriteria", model.incidentcriteria ?? "");
            cmd.Parameters.AddWithValue("@incidentcriteriasub", model.incidentcriteriasub ?? "");
            cmd.Parameters.AddWithValue("@assignedcat", model.assignedcat ?? "");
            cmd.Parameters.AddWithValue("@assignedstaff", model.assignedstaff ?? "");
            cmd.Parameters.AddWithValue("@summary", model.summary ?? "");
            cmd.Parameters.AddWithValue("@description", model.description ?? "");
            cmd.Parameters.AddWithValue("@dayshift", model.IsDayShift ? "Y" : "N");
            cmd.Parameters.AddWithValue("@nightshift", model.IsNightShift ? "Y" : "N");
            cmd.Parameters.AddWithValue("@status", model.status ?? "");

            await cmd.ExecuteNonQueryAsync();
            await PopulateDropdowns(model);
            return View("IncidentEdit", model); // Or wherever you want to go after update
        }

        //NEW INCIDENT EDIT END


    }
}