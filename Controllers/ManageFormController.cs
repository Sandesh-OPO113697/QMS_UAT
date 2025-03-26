using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Asn1.Ocsp;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace QMS.Controllers
{
    public class ManageFormController : Controller
    {
        private readonly Dl_Admin _admin;
        private readonly Dl_formBuilder dl_FormBuilder;
        public ManageFormController(Dl_formBuilder adl, Dl_Admin adam)
        {

            _admin = adam;
            dl_FormBuilder = adl;
        }
        [HttpPost("SaveDispositions")]
        public async Task< IActionResult> SaveDispositions([FromBody] EditDispoRequestModel dispositions)
        {
            await dl_FormBuilder.UpdateDispositionfeilds(dispositions.dispositions);
            return Ok();
        }
        [HttpPost("SaveSubDispositions")]
        public async Task<IActionResult> SaveSubDispositions([FromBody] EditSUBDispoRequestModel subDispositions)
        {
            await dl_FormBuilder.UpdateSubDispositionfeilds(subDispositions.subDispositions);
            return Ok();
        }
            [HttpPost]
        public async Task<IActionResult> BulkInsertDispostitionList(IFormFile file, string processID, string SubProcesID)
        {
            await dl_FormBuilder.BuilkDispoUpload(file , processID , SubProcesID);
            return Ok();
        }
            [HttpPost]
        public async Task< JsonResult> SaveAgents([FromBody] AgentRequestModel agents)
        {
            await dl_FormBuilder.UpdateAgentfeilds(agents.Agents);
            return Json(new { success = true, message = "Agents saved successfully!" });
        }
            [HttpPost("ManageForm/GetProcessListAsync")]
        public async Task< JsonResult> GetProcessListAsync([FromBody] DropDawon model)
        {

            var locations = await _admin.GetLocationAsync();

            var location = locations.FirstOrDefault(l => l.Value == model.Id.ToString())?.Text;

            List<SelectListItem> processList = new List<SelectListItem>();

            

            try
            {
                if (UserInfo.UserType == "Admin")
                {
                    DataTable dt = await dl_FormBuilder.GetProcessListAsync();
                    processList = dt.AsEnumerable()
                        .Where(row => row["LocationName"].ToString() == location)
                        .Select(row => new SelectListItem
                        {
                            Value = row["ID"].ToString(),
                            Text = row["ProcessName"].ToString(),
                        }).ToList();
                }
                else
                {
                    var data = await _admin.GetProcessListByLocation(UserInfo.LocationID);

                    processList = data.AsEnumerable()
                        .Where(row => row["LocationName"].ToString() == location) 
                        .Select(row => new SelectListItem
                        {
                            Value = row["ID"].ToString(),
                            Text = row["ProcessName"].ToString(),
                        }).ToList();
                }

            }
            catch(Exception ex)
            {

            }
          

            return Json(new { success = true, processList });
        }

        [HttpPost]
        public async Task<JsonResult> UpdatesecionGried([FromBody] SectionFormDataUpdateModel request)
        {
            if (request == null || request.sections == null || request.sections.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }

            int result = await dl_FormBuilder.UpdateSectionfeilds(request.sections);

            return result > 0
                ? Json(new { success = true, message = "Form Created  successfully!" })
                : Json(new { success = false, message = "Failed to insert data." });
        }

        [HttpPost]
        public async Task<JsonResult> UpdatesecionGriedReplicatedForm([FromBody] SectionFormDataUpdateModel request)
        {
            if (request == null || request.sections == null || request.sections.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }

            int result = await dl_FormBuilder.UpdateSectionfeildsReplicatedForm(request.sections);

            return result > 0
                ? Json(new { success = true, message = "Form Created  successfully!" })
                : Json(new { success = false, message = "Failed to insert data." });
        }

        [HttpPost]
        public async Task<IActionResult> ActivateReplicatedFormByID([FromBody] Process_SUbProcess id)
        {

            bool success = await dl_FormBuilder.ActivateFormByID(id.ProcessID, id.SUBProcessID);

            return Json(new { success = true, message = "Form Is Activated Successfully...!" });
        }

        [HttpPost]
        public async Task<IActionResult> ActivateForm(int processId, int subProcessId)
        {
           
            bool success = await dl_FormBuilder.ActivateFormByID(processId, subProcessId);

            return RedirectToAction("ActiveReplicatedForm");
        }
        public async Task<IActionResult> EditReplicatedForm(int processId, int subProcessId)
        {

            var Location = await _admin.GetLocationAsync();
            ViewBag.Locations = Location;
            DataSet dt = await dl_FormBuilder.GetSectionFeildAsync();
            var data1 = dt.Tables[0];

            var Section_Category = data1.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["id"].ToString(),
                Text = $"{row["SectionName"]}",
            }).ToList();
            ViewBag.Section_Category = Section_Category;

            ViewBag.ProgramId = processId;
            ViewBag.SubProgramId = subProcessId;
            string locationid = UserInfo.LocationID;
            var data = await _admin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
            var Routw_cause = await dl_FormBuilder.GetRoot_Cause_AnalysisAsync();
            var Routw_causeList = Routw_cause.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["Metric_RCA"]}",
            }).ToList();
            ViewBag.Routw_causeList = Routw_causeList;
            var getPredictive = await dl_FormBuilder.GetgetPredictive_Analysis();
            var getPredictiveList = getPredictive.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["Predictive_CSAT"]}",
            }).ToList();
            ViewBag.getPredictiveList = getPredictiveList;


            var getZT_Classification = await dl_FormBuilder.getZT_Classification();
            var getZT_ClassificationList = getZT_Classification.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ZT_Classification"]}",
            }).ToList();
            ViewBag.getZT_ClassificationList = getZT_ClassificationList;


            return View();
        }

        public async Task<IActionResult> ActiveReplicatedForm()
        {
            DataTable dt = await dl_FormBuilder.GetGriedOfReplicatedForm();


            return View(dt);
        }
        public async Task<IActionResult> FormReplication([FromBody] FormReplicationModel id)
        {
            int Result = await dl_FormBuilder.FormReplicationfromOld(id);

            if (Result == 1)
            {
                return Json(new { success = true, message = "Form Is Replicated Successfully...!", data = Result });
            }
            else
            {
                return Json(new { success = false, message = " Faileld to FormReplicated ...!", data = Result });
            }
          
        }
        public async Task<IActionResult> CheckIsFormvaialableOrNot([FromBody] Process_SUbProcess id)
        {
            int Result = await dl_FormBuilder.CheckIsFormCreatedInData(id);
            return Json(new { success = false, message = "Failed to Insert the field.", data = Result });
        }
        public async Task<IActionResult> Formdisable([FromBody] Process_SUbProcess id)
        {
            int Result = await dl_FormBuilder.DisableFormTable(id);
            return Json(new { success = true, message = "Form disabled successfully", data = Result });
        }
        public async Task<IActionResult> CheckIsFormReplicated([FromBody] Process_SUbProcess id)
        {
            int Result = await dl_FormBuilder.CheckIsFormreplicatedInData(id);
            return Json(new { success = false, message = "Failed to Insert the field."  , data= Result });
        }
        public async Task<IActionResult> CheckIsFormCreated([FromBody] Process_SUbProcess id)
        {
            int Result = await dl_FormBuilder.CheckIsFormCreatedInData(id);
            return Json(new { success = false, message = "Failed to Insert the field.", data = Result });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDynamicFields([FromBody] DynamicModelNew model)
        {
            int Result = await dl_FormBuilder.UpdateValueInDynamicmaster(model);
            if (Result == 1)
            {
                return Json(new { success = true, message = "Form inserted successfully\"" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to Insert the field." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateDynamicFieldsReplicatedForm([FromBody] DynamicModelNew model)
        {
            int Result = await dl_FormBuilder.UpdateValueInDynamicmasterReplicatedForm(model);
            if (Result == 1)
            {
                return Json(new { success = true, message = "Form inserted successfully\"" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to Insert the field." });
            }
        }
        public async Task<IActionResult> GetDynamicDropDawon(int id, string type)
        {
            object result;

            switch (type.ToLower())
            {
                case "rootcause":
                    var rootCauseData = await dl_FormBuilder.GetRoot_Cause_AnalysisAsync();
                    var rootCauseList = rootCauseData.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["ID"].ToString(),
                        Text = row["Metric_RCA"].ToString()
                    }).ToList();

                    result = new { success = true, data = rootCauseList, message = "Categories fetched successfully!" };
                    break;
                case "predictive":
                    var getPredictive = await dl_FormBuilder.GetgetPredictive_Analysis();
                    var getPredictiveList = getPredictive.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["ID"].ToString(),
                        Text = $"{row["Predictive_CSAT"]}"
                    }).ToList();

                    result = new { success = true, data = getPredictiveList, message = "Categories fetched successfully!" };
                    break;

                case "rating":
                    DataSet ds = await dl_FormBuilder.GetSectionDropdownDataAsync();
                    DataTable ratingTableData = ds.Tables[1];
                    var ratingTable = ratingTableData.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["Id"].ToString(),
                        Text = $"{row["RatingName"]}"
                    }).ToList();


                    result = new { success = true, data = ratingTable, message = "Categories fetched successfully!" };
                    break;
                case "ztclassification":
                    var getZT_Classification = await dl_FormBuilder.getZT_Classification();
                    var getZT_ClassificationList = getZT_Classification.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["ID"].ToString(),
                        Text = $"{row["ZT_Classification"]}"
                    }).ToList();

                    result = new { success = true, data = getZT_ClassificationList, message = "Categories fetched successfully!" };
                    break;


                default:
                    return Json(new { success = false, message = "Invalid type provided!" });
            }

            return Json(result);
        }




        [HttpPost]
        public async Task<IActionResult> InsertDynamicFields([FromBody] DynamicModelNew model)
         {
            int Result = await dl_FormBuilder.InsertValueInDynamicmaster(model);
          
            if (Result == 1)
            {
                return Json(new { success = true, message = "Form inserted successfully\"" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to Insert the field." });
            }
        }
            [HttpPost]
        public async Task<JsonResult> UpdateDynamicFieldsValue([FromBody] DynamicFieldUpdateRequest request)
        {
            try
            {
                int result = await dl_FormBuilder.UpdatedynamicFeilds(request);
                if (result == 1)
                {
                    return Json(new { success = true, message = "Field updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update the field." });
                }
            }
            catch (Exception ex)
            {
    
                Console.Error.WriteLine($"Error updating dynamic fields: {ex.Message}");

                return Json(new { success = false, message = "An error occurred while updating the field." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSection([FromBody] SectionGridModel model)
        {
            try
            {
                bool isDeleted = await dl_FormBuilder.DeleteSectionAsync(model.Id);
                if (isDeleted)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete section." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSection([FromBody] SectionGridModel section)
        {
             await dl_FormBuilder.EditSectionRow(section);
            return Ok();
        }

            [HttpGet]
        public async Task<IActionResult> GetDropdownData()
        {
            DataSet ds = await dl_FormBuilder.GetSectionDropdownDataAsync();

            if (ds.Tables.Count < 2)
                return BadRequest("Failed to retrieve dropdown data.");

            DataTable sectionTable = ds.Tables[0];
            DataTable ratingTable = ds.Tables[1];

            var sectionList = sectionTable.AsEnumerable().Select(row => new
            {
                Id = row.Field<int>("id"),
                Name = row.Field<string>("SectionName")
            }).ToList();

            var ratingList = ratingTable.AsEnumerable().Select(row => new
            {
                Id = row.Field<int>("Id"),
                Name = row.Field<string>("RatingName")
            }).ToList();

            return Json(new { Sections = sectionList, Ratings = ratingList });
        }

        public async Task<IActionResult> GetdynamicfeildsGried([FromBody] Process_SUbProcess id)
        {
            var dataTable = await dl_FormBuilder.GetdynamicFeildsGriedAsync(id.ProcessID, id.SUBProcessID);
            try
            {
                var sectionList = dataTable.AsEnumerable().Select(row => new dynamicgriedModel
                {
                    id = row.Field<int>("id"),
                    Dynamic_field_ID = row.Field<int>("Dynamic_field_ID"),
                    fields_Value = row.Field<string>("fields_Value"),
                    ValueType = row.Field<string>("ValueType"),
                    value = row.Field<string>("value")
                }).ToList();
                return Json(sectionList);
            }
            catch (Exception ex)
            {
                return Json(null);
            }

        }


        public async Task<IActionResult> GetSectionGried([FromBody] Process_SUbProcess id)
        {
            try
            {
                var dataTable = await dl_FormBuilder.GetSectionGriedAsync(id.ProcessID, id.SUBProcessID);
                var dataTable2 = await dl_FormBuilder.GetAgentGriedAsync(id.ProcessID, id.SUBProcessID);
                var dataTable3 = await dl_FormBuilder.GetDispostionGriedAsync(id.ProcessID, id.SUBProcessID);
                var dataTable4 = await dl_FormBuilder.GetSubDispositionriedAsync(id.ProcessID, id.SUBProcessID);
                DataTable DynamicFeild = await dl_FormBuilder.GetDynamicGriedAsync(id.ProcessID, id.SUBProcessID);


                var sectionList = dataTable.AsEnumerable().Select(row => new SectionGridModel
                {
                    Id = row.Field<int>("id"),
                    Category = row.Field<string>("Category"),
                    SectionName = row.Field<string>("SectionName"),
                    SectionId = row.Field<int>("SectionId"),

                    Scorable = row.Field<string>("Scorable"),
                    Score = row.Field<int>("Score"),
                    Level = row.Field<string>("Level"),

                    Active = row.Field<string>("Active")
                }).ToList();

                var agentgried = dataTable2.AsEnumerable().Select(row => new AgentListModel
                {
                    ID = row.Field<int>("ID"),
                    EmpName = row.Field<string>("EmpName"),
                    EmpCode = row.Field<string>("EmpCode"),
                    TL_Name = row.Field<string>("TL_Name"),
                    Batch_ID = row.Field<string>("Batch_ID"),
                    TL_Code = row.Field<string>("TL_Code"),
                    QA_Name = row.Field<string>("QA_Name")




                }).ToList();
                var dispositiongried = dataTable3.AsEnumerable().Select(row => new EditDispoModel
                {
                    
                    Dispostition_ID = row.Field<int>("Dispostition_ID"),
                    Disposition_name = row.Field<string>("Disposition_name")
                   

                }).ToList();

                var subdispositiongried = dataTable4.AsEnumerable().Select(row => new EditSubDispoModel
                {

                    Disposition = row.Field<string>("Disposition"),
                    SubDisposition = row.Field<string>("SubDisposition")


                }).ToList();



                List<SelectListItem> filteredRoutwCauseList = new List<SelectListItem>();
                List<SelectListItem> filteredPredictiveList = new List<SelectListItem>();
                List<SelectListItem> filteredZTClassificationList = new List<SelectListItem>();
                List<SelectListItem> filteredRatingList = new List<SelectListItem>();
                string Zero_Tolerance = "No";

                if (DynamicFeild.Rows.Count > 0)
                {
                    Zero_Tolerance = DynamicFeild.Rows[0]["Zero_Tolerance"].ToString();

                    var Root_Cause_Analysis = DynamicFeild.Rows[0]["Root_Cause_Analysis"].ToString().Split(',').Select(s => s.Trim()).ToList();
                    var Predictive_Analysis = DynamicFeild.Rows[0]["Predictive_Analysis"].ToString().Split(',').Select(s => s.Trim()).ToList();
                    var ZT_Classification = DynamicFeild.Rows[0]["ZT_Classification"].ToString().Split(',').Select(s => s.Trim()).ToList();

                    var Rating = DynamicFeild.Rows[0]["Rating"].ToString().Split(',').Select(s => s.Trim()).ToList();

                    var Routw_cause = await dl_FormBuilder.GetRoot_Cause_AnalysisAsync();
                    var Routw_causeList = Routw_cause.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["ID"].ToString(),
                        Text = $"{row["Metric_RCA"]}"
                    }).ToList();
                    filteredRoutwCauseList = Routw_causeList.Where(item => Root_Cause_Analysis.Contains(item.Value)).ToList();

                    var getPredictive = await dl_FormBuilder.GetgetPredictive_Analysis();
                    var getPredictiveList = getPredictive.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["ID"].ToString(),
                        Text = $"{row["Predictive_CSAT"]}"
                    }).ToList();
                    filteredPredictiveList = getPredictiveList.Where(item => Predictive_Analysis.Contains(item.Value)).ToList();
                    DataSet ds = await dl_FormBuilder.GetSectionDropdownDataAsync();
                    DataTable ratingTableData = ds.Tables[1];
                    var ratingTable = ratingTableData.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["Id"].ToString(),
                        Text = $"{row["RatingName"]}"
                    }).ToList();
                    filteredRatingList = ratingTable.Where(item => Rating.Contains(item.Value)).ToList();
                    var getZT_Classification = await dl_FormBuilder.getZT_Classification();
                    var getZT_ClassificationList = getZT_Classification.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["ID"].ToString(),
                        Text = $"{row["ZT_Classification"]}"
                    }).ToList();
                    filteredZTClassificationList = getZT_ClassificationList.Where(item => ZT_Classification.Contains(item.Value)).ToList();
                }

                return Json(new
                {
                    SectionGrid = sectionList,
                    FilteredRoutwCauseList = filteredRoutwCauseList,
                    FilteredPredictiveList = filteredPredictiveList,
                    FilteredZTClassificationList = filteredZTClassificationList,
                    ZeroTolerance = Zero_Tolerance,
                    agentgried= agentgried,
                    dispositiongried= dispositiongried,
                    subdispositiongried=subdispositiongried,
                    filteredRatingList=filteredRatingList
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }



        public async Task<IActionResult> GetSectionGriedForReplicateForm([FromBody] Process_SUbProcess id)
        {
            try
            {
                var dataTable = await dl_FormBuilder.GetSectionGriedReplicatedAsync(id.ProcessID, id.SUBProcessID);
                DataTable DynamicFeild = await dl_FormBuilder.GetdynamicFeildsGriedAsync(id.ProcessID, id.SUBProcessID);

                var dataTable2 = await dl_FormBuilder.GetAgentGriedAsync(id.ProcessID, id.SUBProcessID);

                var dataTable3 = await dl_FormBuilder.GetDispostionGriedAsync(id.ProcessID, id.SUBProcessID);
                var dataTable4 = await dl_FormBuilder.GetSubDispositionriedAsync(id.ProcessID, id.SUBProcessID);

                var sectionList = dataTable.AsEnumerable().Select(row => new SectionGridModel
                {
                    Id = row.Field<int>("id"),
                    Category = row.Field<string>("Category"),
                    SectionName = row.Field<string>("SectionName"),
                    SectionId = row.Field<int>("SectionId"),

                    Scorable = row.Field<string>("Scorable"),
                    Score = row.Field<int>("Score"),
                    Level = row.Field<string>("Level"),

                    Active = row.Field<string>("Active")
                }).ToList();

                var agentgried = dataTable2.AsEnumerable().Select(row => new AgentListModel
                {
                    ID = row.Field<int>("ID"),
                    EmpName = row.Field<string>("EmpName"),
                    EmpCode = row.Field<string>("EmpCode"),
                    TL_Name = row.Field<string>("TL_Name"),
                    Batch_ID = row.Field<string>("Batch_ID"),
                    TL_Code = row.Field<string>("TL_Code"),
                    QA_Name = row.Field<string>("QA_Name")




                }).ToList();

                var dispositiongried = dataTable3.AsEnumerable().Select(row => new EditDispoModel
                {

                    Dispostition_ID = row.Field<int>("Dispostition_ID"),
                    Disposition_name = row.Field<string>("Disposition_name")


                }).ToList();

                var subdispositiongried = dataTable4.AsEnumerable().Select(row => new EditSubDispoModel
                {

                    Disposition = row.Field<string>("Disposition"),
                    SubDisposition = row.Field<string>("SubDisposition")


                }).ToList();


                List<SelectListItem> filteredRoutwCauseList = new List<SelectListItem>();
                List<SelectListItem> filteredPredictiveList = new List<SelectListItem>();
                List<SelectListItem> filteredZTClassificationList = new List<SelectListItem>();
                List<SelectListItem> filteredRatingList = new List<SelectListItem>();
                string Zero_Tolerance = "No";

                if (DynamicFeild.Rows.Count > 0)
                {
                    Zero_Tolerance = DynamicFeild.Rows[0]["Zero_Tolerance"].ToString();

                    var Root_Cause_Analysis = DynamicFeild.Rows[0]["Root_Cause_Analysis"].ToString().Split(',').Select(s => s.Trim()).ToList();
                    var Predictive_Analysis = DynamicFeild.Rows[0]["Predictive_Analysis"].ToString().Split(',').Select(s => s.Trim()).ToList();
                    var ZT_Classification = DynamicFeild.Rows[0]["ZT_Classification"].ToString().Split(',').Select(s => s.Trim()).ToList();
                    var Rating = DynamicFeild.Rows[0]["Rating"].ToString().Split(',').Select(s => s.Trim()).ToList();
                    var Routw_cause = await dl_FormBuilder.GetRoot_Cause_AnalysisAsync();
                    var Routw_causeList = Routw_cause.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["ID"].ToString(),
                        Text = $"{row["Metric_RCA"]}"
                    }).ToList();
                    filteredRoutwCauseList = Routw_causeList.Where(item => Root_Cause_Analysis.Contains(item.Value)).ToList();

                    var getPredictive = await dl_FormBuilder.GetgetPredictive_Analysis();
                    var getPredictiveList = getPredictive.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["ID"].ToString(),
                        Text = $"{row["Predictive_CSAT"]}"
                    }).ToList();
                    filteredPredictiveList = getPredictiveList.Where(item => Predictive_Analysis.Contains(item.Value)).ToList();
                    DataSet ds = await dl_FormBuilder.GetSectionDropdownDataAsync();
                    DataTable ratingTableData = ds.Tables[1];
                    var ratingTable = ratingTableData.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["Id"].ToString(),
                        Text = $"{row["RatingName"]}"
                    }).ToList();
                    filteredRatingList = ratingTable.Where(item => Rating.Contains(item.Value)).ToList();
                    var getZT_Classification = await dl_FormBuilder.getZT_Classification();
                    var getZT_ClassificationList = getZT_Classification.AsEnumerable().Select(row => new SelectListItem
                    {
                        Value = row["ID"].ToString(),
                        Text = $"{row["ZT_Classification"]}"
                    }).ToList();
                    filteredZTClassificationList = getZT_ClassificationList.Where(item => ZT_Classification.Contains(item.Value)).ToList();
                }

                return Json(new
                {
                    SectionGrid = sectionList,
                    FilteredRoutwCauseList = filteredRoutwCauseList,
                    FilteredPredictiveList = filteredPredictiveList,
                    FilteredZTClassificationList = filteredZTClassificationList,
                    ZeroTolerance = Zero_Tolerance,
                    agentgried = agentgried,
                    dispositiongried = dispositiongried,
                    subdispositiongried = subdispositiongried,
                    filteredRatingList=filteredRatingList
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }


        public async Task<IActionResult> EditForm(string programId, string subProgramId)
        {

            var Location = await _admin.GetLocationAsync();
            ViewBag.Locations = Location;
            DataSet dt = await dl_FormBuilder.GetSectionFeildAsync();
            var data1 = dt.Tables[0];
         
            var Section_Category = data1.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["id"].ToString(),
                Text = $"{row["SectionName"]}",
            }).ToList();
            ViewBag.Section_Category = Section_Category;

            ViewBag.ProgramId = programId;
            ViewBag.SubProgramId = subProgramId;
            string locationid = UserInfo.LocationID;
            var data = await _admin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
            var Routw_cause = await dl_FormBuilder.GetRoot_Cause_AnalysisAsync();
            var Routw_causeList = Routw_cause.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["Metric_RCA"]}",
            }).ToList();
            ViewBag.Routw_causeList = Routw_causeList;
            var getPredictive = await dl_FormBuilder.GetgetPredictive_Analysis();
            var getPredictiveList = getPredictive.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["Predictive_CSAT"]}",
            }).ToList();
            ViewBag.getPredictiveList = getPredictiveList;


            var getZT_Classification = await dl_FormBuilder.getZT_Classification();
            var getZT_ClassificationList = getZT_Classification.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ZT_Classification"]}",
            }).ToList();
            ViewBag.getZT_ClassificationList = getZT_ClassificationList;

            return View();
        }

        public async Task<IActionResult> ViewForm()
        {
            var fields = await dl_FormBuilder.GetStaticfiedls();
            ViewBag.Fields = fields;
            return View();
        }
       
        [HttpPost]
        public async Task<JsonResult> InsertSectionFeilds([FromBody] SectionFormData request)
        {
            if (request == null || request.sections == null || request.sections.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }

            int result = await dl_FormBuilder.AddSectionfeilds(request.sections);

            return result > 0
                ? Json(new { success = true, message = "Form Created  successfully!" })
                : Json(new { success = false, message = "Failed to insert data." });
        }

        public IActionResult FormBuilder()
        {
            return View();
        }

        public async Task<IActionResult> CreateForm()
        {
            string locationid = UserInfo.LocationID;
            var data = await _admin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
            DataSet dt = await dl_FormBuilder.GetSectionFeildAsync();
            var data1 = dt.Tables[0];
            var data2 = dt.Tables[1];
            var data3 = dt.Tables[2];
            var Section_Category = data1.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["id"].ToString(),
                Text = $"{row["SectionName"]}",
            }).ToList();
            ViewBag.Section_Category = Section_Category;

            var RatingMaste = data2.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["Id"].ToString(),
                Text = $"{row["RatingName"]}",
            }).ToList();
            ViewBag.RatingMaste = RatingMaste;

            //var DynamicFields = data3.AsEnumerable().Select(row => new SelectListItem
            //{
            //    Value = row["id"].ToString(),
            //    Text = $"{row["fields_Value"]}",
            //}).ToList();
            //ViewBag.DynamicFields = DynamicFields;
            var Routw_cause = await dl_FormBuilder.GetRoot_Cause_AnalysisAsync();
            var Routw_causeList = Routw_cause.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["Metric_RCA"]}",
            }).ToList();
            ViewBag.Routw_causeList = Routw_causeList;
            var getPredictive = await dl_FormBuilder.GetgetPredictive_Analysis();
            var getPredictiveList = getPredictive.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["Predictive_CSAT"]}",
            }).ToList();
            ViewBag.getPredictiveList = getPredictiveList;


            var getZT_Classification = await dl_FormBuilder.getZT_Classification();
            var getZT_ClassificationList = getZT_Classification.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ZT_Classification"]}",
            }).ToList();
            ViewBag.getZT_ClassificationList = getZT_ClassificationList;
            return View();
        }
    }
}
