using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mimsv2.Models
{
    public class FormModel
    {

        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Hospitals { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> IncidentCategories { get; set; } = new();
        public List<SelectListItem> IncidentSubCat1 { get; set; } = new();
        public List<SelectListItem> IncidentSubCat2 { get; set; } = new();
        public List<SelectListItem> IncidentSubCat3 { get; set; } = new();

        public List<string> UploadedFiles { get; set; } = new List<string>();



        public int id { get; set; }
        public string affectedward { get; set; }
        public string incidentarea { get; set; }
        public string incidentcriteria { get; set; }
        public string incidentcriteriasub { get; set; }
        public string requester { get; set; }
        public string requesteremail { get; set; }
        public string priority { get; set; }
        public string titles { get; set; }
        public string reportedby { get; set; }
        public string invesitgatedby { get; set; }
        public string assignedcat { get; set; }
        public string assignedstaff { get; set; }

        [DataType(DataType.Date)]
        public DateTime? incidentdate { get; set; } //datefield
        public string incidenttime { get; set; }

        [DataType(DataType.Date)]
        public DateTime? datereported { get; set; }


        [DataType(DataType.Date)]
        public DateTime? datecaptured { get; set; }

        public string summary { get; set; }
        public string description { get; set; }
        public string timecaptured { get; set; }
        public string active { get; set; }
        public string status { get; set; }
        public string hospitalid { get; set; }
        public string qarid { get; set; }
        public string username { get; set; }
        public string surname { get; set; }

        [DataType(DataType.Date)]
        public DateTime? onholddescdate { get; set; }


        [DataType(DataType.Date)]
        public DateTime? closeddescdate { get; set; }

        public string onholddesctime { get; set; }
        public string closeddesctime { get; set; }
        public string onholddesc { get; set; }
        public string closeddesc { get; set; }
        public string pte { get; set; }
        public string ptenumber { get; set; }
        public string ptename { get; set; }
        public string ptesurname { get; set; }
        public string ptetitle { get; set; }
        public string reportedbyemail { get; set; }
        public string correctaction { get; set; }
        public string correctactiontime { get; set; }
        public string preventaction { get; set; }
        public string preventactiontime { get; set; }

        [DataType(DataType.Date)]
        public DateTime? preventactiondate { get; set; }


        [DataType(DataType.Date)]
        public DateTime? correctactiondate { get; set; }

        public string investigation { get; set; }
        public string summary2 { get; set; }
        public string medrelatedtotal { get; set; }
        public string reportedbydepartment { get; set; }

        [DataType(DataType.Date)]
        public DateTime? incidentexpires { get; set; } //this must be 7 or 10 days
        public string incidentareanight { get; set; }
        public string acquired { get; set; }
        public string incidenttype { get; set; }
        public string inctypescat1 { get; set; }
        public string inctypescat2 { get; set; }
        public string inctypescat3 { get; set; }  
        public string inctypescat4 { get; set; }  
        public string CapturedByLoginName { get; set; }
        public string CapturedByName { get; set; }
        public string CapturedBySurname { get; set; }
        public string CapturedByTitle { get; set; }
        public string CapturedByEmail { get; set; }
        public string CapturedbyDpt { get; set; }
        public string HospitalId { get; set; }





    }
}
