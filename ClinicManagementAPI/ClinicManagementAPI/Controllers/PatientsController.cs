using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UPB.PatientEntities.Models;
using UPB.PatientEntities.Managers;

namespace UPB.ClinicManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly PatientManager _patientManager;

        public PatientsController(PatientManager patientManager)
        {
            _patientManager = patientManager;
        }

        // POST: api/Patient
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Patient patient)
        {
            var createdPatient = await _patientManager.CreatePatientAsync(patient);
            return CreatedAtAction(nameof(GetByCI), new { ci = createdPatient.CI }, createdPatient);
        }

        // PUT: api/Patient/5
        [HttpPut("{ci}")]
        public void Put(int ci, [FromBody] Patient patient)
        {
            _patientManager.UpdatePatient(ci, patient.Name, patient.LastName);
        }

        // DELETE: api/Patient/5
        [HttpDelete("{ci}")]
        public void Delete(int ci)
        {
            _patientManager.DeletePatient(ci);
        }

        // GET: api/Patient
        [HttpGet]
        public Dictionary<int, Patient> Get()
        {
            return _patientManager.GetAll();
        }

        // GET: api/Patient/5
        [HttpGet("{ci}")]
        public Patient GetByCI(int ci)
        {
            return _patientManager.GetPatientByCI(ci);
        }
    }
}
