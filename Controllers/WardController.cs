using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Mimsv2.Models;
using Npgsql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;
using System.Text;

namespace Mimsv2.Controllers
{
    public class WardController : Controller
    {
        private readonly IConfiguration _configuration;

        public WardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var users = new List<WardModel>();

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = @"SELECT *, h.hospital AS hospitalname 
                        FROM tbldepartments d 
                        INNER JOIN tblhospitals h ON d.hospitalid = h.hospitalid
                        WHERE d.active = 'Y' AND description = 'ward' order by h.hospitalid ";
            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new WardModel
                {
                    id = Convert.ToInt32(reader["id"]),
                    department = reader["department"].ToString(),
                    active = reader["active"].ToString(),
                    description = reader["description"].ToString(),
                    hospitalid = reader["hospitalid"].ToString(),
                    hospitalname = reader["hospitalname"].ToString()
                });
            }

            return View(users);
        }


        //ADD WARD
        [HttpGet]
        public async Task<IActionResult> AddWard(int id) // id = hospitalid
        {
            var model = new WardModel
            {
                hospitalid = id.ToString(),
                description = "ward",
                Hospitals = new List<SelectListItem>()
            };

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = "SELECT hospitalid, hospital FROM tblhospitals WHERE active = 'Y'";
            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var hospId = reader["hospitalid"].ToString();
                model.Hospitals.Add(new SelectListItem
                {
                    Value = hospId,
                    Text = reader["hospital"].ToString(),
                    Selected = hospId == model.hospitalid
                });
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AddWard(WardModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = @"INSERT INTO tbldepartments (department, description, hospitalid, active)
                   VALUES (@department, @description, @hospitalid, 'Y')";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("department", model.department);
            cmd.Parameters.AddWithValue("description", model.description ?? "");
            cmd.Parameters.AddWithValue("hospitalid", Convert.ToInt32(model.hospitalid));

            await cmd.ExecuteNonQueryAsync();

            return RedirectToAction("Index");
        }


        //ADD WARD END


        //EDIT WARD HERE
        [HttpGet]
        public async Task<IActionResult> EditWard(int id)
        {
            var model = new WardModel();
            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            // Load ward details
            string sqlWard = "SELECT * FROM tbldepartments WHERE id = @id";
            using (var cmd = new NpgsqlCommand(sqlWard, conn))
            {
                cmd.Parameters.AddWithValue("id", id);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    model.id = id;
                    model.department = reader["department"].ToString();
                    model.description = reader["description"].ToString();
                    model.hospitalid = reader["hospitalid"].ToString();
                    model.active = reader["active"].ToString();
                }
            }

            // Load hospital list
            model.Hospitals = new List<SelectListItem>();
            string sqlHosp = "SELECT hospitalid, hospital FROM tblhospitals WHERE active = 'Y'";
            using (var cmd = new NpgsqlCommand(sqlHosp, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var hospId = reader["hospitalid"].ToString();
                    model.Hospitals.Add(new SelectListItem
                    {
                        Value = hospId,
                        Text = reader["hospital"].ToString(),
                        Selected = hospId == model.hospitalid
                    });
                }
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditWard(WardModel model)
        {
           // if (!ModelState.IsValid)
           // {
           //     return View(model);
          //  }

            using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            string sql = @"UPDATE tbldepartments 
                   SET department = @department, 
                       description = @description, 
                       hospitalid = @hospitalid, 
                       active = @active 
                   WHERE id = @id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("department", model.department);
            cmd.Parameters.AddWithValue("description", model.description ?? "ward");
            cmd.Parameters.AddWithValue("hospitalid", Convert.ToInt32(model.hospitalid));
            cmd.Parameters.AddWithValue("active", model.active ?? "Y");
            cmd.Parameters.AddWithValue("id", model.id);

            await cmd.ExecuteNonQueryAsync();

            return RedirectToAction("Index");
        }


        //EDIT WARD HERE


    }

}
