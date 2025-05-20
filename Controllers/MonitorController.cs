using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Asn1.X509;
using QMS.DataBaseService;
using QMS.Models;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;

namespace QMS.Controllers
{
    public class MonitorController : Controller
    {
        private readonly DlSampling dlSampling;
        private readonly Dl_Admin _admin;
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly dl_Monitoring dl_monitor;
        private readonly DL_Agent dlagent;
        public MonitorController(Dl_formBuilder adl, DlSampling dl, Dl_Admin adam, dl_Monitoring dmmonoi, DL_Agent dlagent)
        {
            dlSampling = dl;
            _admin = adam;
            dl_FormBuilder = adl;
            dl_monitor = dmmonoi;
            this.dlagent = dlagent;
        }

        public async Task<IActionResult> Testdetails(int TestID)
        {

            DataTable assment = await dl_monitor.TestviewDetails(TestID);

            DataTable Pening = await dl_monitor.PendingTestviewDetails(TestID);
            var list = assment.AsEnumerable().Select(row => new TestResultViewModel22
            {
                TestName = row.Field<string>("TestName"),
                UserName = row.Field<string>("UserName"),
                ScorePercentage = row.Field<string>("ScorePercentage"),
                CreatedDate = row.Field<DateTime>("CreatedDate"),
                ExpiryDate = row.Field<DateTime?>("ExpiryDate"),
                AttemptDate = row.Field<DateTime>("AttemptDate")
            }).ToList();


            var Peninglist = Pening.AsEnumerable().Select(row => new PendingAssesment
            {
                
                UserName = row.Field<string>("UserName"),
                Role = row.Field<string>("Role_Name")

            }).ToList();
            var model = new AssessmentPageViewModel
            {
                CompletedAssessments = list,
                PendingAssessments = Peninglist
            };

            return View(model);
        }


        public async Task<IActionResult> Assesmanetdashboard()
        {
            try
            {
                DataTable assment = await dl_monitor.GetAssesment();

                List<Assesmentmonitor> assmentonl = assment.AsEnumerable().Select(row => new Assesmentmonitor
                {
                    TestID = row.Field<int>("TestID"),
                    TestName = row.Field<string>("TestName"),
                    Process = row.Field<string>("Process"),
                    SubProcessName = row.Field<string>("SubProcessName"),
                    TestCategory = row.Field<string>("TestCategory"),
                    CreatedDate = row.Field<DateTime>("CreatedDate"),

                  
                    expiryType = row["expiryType"] != DBNull.Value ? row.Field<string>("expiryType") : string.Empty,

                    // Handle nullable DateTime
                    expiryDate = row["expiryDate"] != DBNull.Value ? row.Field<DateTime>("expiryDate") : DateTime.MinValue,

                    // Handle nullable int
                    expiryHours = row["expiryHours"] != DBNull.Value ? row.Field<int>("expiryHours") : 0

                }).ToList();

                return View(assmentonl);
            }
            catch (Exception ex)
            {
                // You can optionally log the exception here
                return View();
            }
        }

        [HttpPost]
        [Route("save-voice-message")]
        public async Task<IActionResult> SaveVoiceMessage([FromBody] VoiceMessageModel model)
        {
            string pauselimit = await dl_monitor.SaveVoiceMessage(model);
            return Ok(new { message = "Voice message saved successfully!" });
        }
        public async Task<JsonResult> GetPauseLimit([FromBody] Process_SUbProcess id)
        {
            string pauselimit = await dl_monitor.GetPauseLimitByProgram(id.ProcessID.ToString(), id.SUBProcessID.ToString());
           
            return Json(new { success = true, pauselimit = pauselimit });

        }
        [HttpPost]
        [Route("save-audit")]
        public async Task< IActionResult > SaveAudit([FromBody] AuditPauseLog audit)
        {
            if (audit == null)
                return BadRequest("Invalid audit data");
            var data = await dl_monitor.InsertAuditPauseLog(audit);

            return Ok(new { message = "Audit saved successfully!" });
        }


        public async Task<IActionResult> CheckTheAuditIsDone([FromBody] DropDawnString id)
        {
            if (id == null || string.IsNullOrEmpty(id.ID))
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            try
            {
                string TransactionID = await dl_monitor.CheckAuditByTransactionDone(id.ID.ToString());


                return Json(new { success = true, connid = TransactionID });
            }
            catch (FileNotFoundException)
            {
                return Json(new { success = false, message = "File not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> InsertPridictiveCoauseAudit([FromBody] List<PredictiveEvaluationModel> sectionData)
        {


            if (sectionData == null || sectionData.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }
            var result = await dl_monitor.SubmitePridictiveEvaluation(sectionData);
            if (result == 1)
            {
                return Json(new { success = true, message = "Ok" });
            }
            else
            {
                return Json(new { success = false, message = "No data Insert" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> InsertRouutCoauseAudit([FromBody] List<RootCauseAnalysisModel> sectionData)
        {


            if (sectionData == null || sectionData.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }
            var result = await dl_monitor.SubmiteRouteCauseEvaluation(sectionData);
            if (result == 1)
            {
                return Json(new { success = true, message = "Ok" });
            }
            else
            {
                return Json(new { success = false, message = "No data Insert" });
            }
        }
        [HttpPost]
        public  async Task< JsonResult> InsertSectionAudit([FromBody]  List<SectionAuditModel> sectionData)
        {
            
          
            if (sectionData == null || sectionData.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }
            var result = await dl_monitor.SubmiteSectionEvaluation(sectionData);
            if (result == 1)
            {
                return Json(new { success = true, message = "Ok" });
            }
            else
            {
                return Json(new { success = false, message = "No data Insert" });
            }
        }


        [HttpPost]
        public  async Task  < IActionResult> FormMinitoringAuditr([FromBody] MonitorFormModel model)
        {
            var result= await dl_monitor.SubmiteFormEvaluation(model);
            if(result==1)
            {
                return Ok(new { sucesss =true, message = "Data received successfully" });
            }
            else
            {
                return Ok(new { sucesss = false,  message = "Data received successfully" });
            }
                
        }

            [HttpGet]
        public async Task<JsonResult> GetRCAValues()
        {
            DataSet ds = await dl_monitor.GetRCAVAluesDroppdawn();
            if (ds.Tables.Count == 3) 
            {
                DataTable rca1 = ds.Tables[0]; 
                DataTable rca2 = ds.Tables[1]; 
                DataTable rca3 = ds.Tables[2];
                var rca1List = rca1.AsEnumerable().Select(row => new { ID = row["ID"], RCA_Value = row["RCA_Value"] }).ToList();
                var rca2List = rca2.AsEnumerable().Select(row => new { ID = row["ID"], RCA_Value = row["RCA_Value"] }).ToList();
                var rca3List = rca3.AsEnumerable().Select(row => new { ID = row["ID"], RCA_Value = row["RCA_Value"] }).ToList();

                return Json(new { success = true, rca1 = rca1List, rca2 = rca2List, rca3 = rca3List });
            }
            else
                return Json(new { success = false, message = "fails." });

        }
               
        public async Task<IActionResult> GetRecording([FromBody] DropDawnString id)
        {
            if (id == null || string.IsNullOrEmpty(id.ID))
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            try
            {
                string base64Audio = await dl_monitor.GetRecordingByConnID(id.ID.ToString());
                

                return Json(new { success = true, audioData = base64Audio });
            }
            catch (FileNotFoundException)
            {
                return Json(new { success = false, message = "File not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        public async Task<JsonResult> GetTLName([FromBody] DropDawnString id)
        {
            string tl_name = string.Empty;
            try
            {
               
                tl_name = await dl_monitor.GetTeamLeaderName(id.ID.ToString());
            }
            catch(Exception ex )
            {

            }

       

            return Json(new { success = true, tl_name });
        }
        [HttpPost]
        public async Task<JsonResult> GetRecListBydate([FromBody] RecApiModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.AgentId) || string.IsNullOrEmpty(request.FromDate) || string.IsNullOrEmpty(request.ToDate))
            {
                return Json(new { success = false, message = "Invalid input data." });
            }
            else
            {
                string reclist = await dl_monitor.GetRecListByAPi(request.FromDate, request.ToDate, request.AgentId);


                return Json(new { success = true, message = "sucesss.", reclist = reclist });
            }

        }

        public async Task<JsonResult> GetDispositoin([FromBody] Process_SUbProcess id)
        {
            List<SelectListItem> sampleSize = await dl_monitor.GetDisposition(id.ProcessID.ToString(), id.SUBProcessID.ToString());

           string typeofprocess = await dl_monitor.GetProvcessType(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            List<SelectListItem> agentlist = await dl_monitor.GetAgentName(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            string typeOfDisposition = await dl_monitor.GetcategoryorDispo(id.ProcessID.ToString(), id.SUBProcessID.ToString());

            List<SelectListItem> cat1 = await dl_monitor.GetCat1(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            List<SelectListItem> cat2= await dl_monitor.GetCat2(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            List<SelectListItem> cat3 = await dl_monitor.GetCat3(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            List<SelectListItem> cat4 = await dl_monitor.GetCat4(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            List<SelectListItem> cat5 = await dl_monitor.GetCat5(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            return Json(new { success = true, samplesize = sampleSize, agentlist = agentlist  , typeofprocess =typeofprocess , typeOfDisposition= typeOfDisposition , cat1= cat1 , cat2= cat2 , cat3= cat3 , cat4= cat4 , cat5= cat5 });

        }

        public async Task<JsonResult> GetSubDispositoin([FromBody] DispositionModel id)
        {
            List<SelectListItem> sampleSize = await dl_monitor.GetSubDisposition(id.ProcessID.ToString(), id.SUBProcessID.ToString(), id.Disposition.ToString());

            return Json(new { success = true, samplesize = sampleSize });

        }

        public async Task<IActionResult> CallMonitor(int SubFeatureid, string RoleName, int Featureid)
        {
            var AuditType = await dlSampling.GetAuditType();
            ViewBag.AuditTypeList = AuditType;

            DataTable dt = await _admin.GetProcessListAsync();
            DataTable LastTransaction = await _admin.GetLastTransactionListAsync();
            ViewBag.LastTransaction = LastTransaction;

            var processList = dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.Process = processList;
            return View();
        }
    }
}
