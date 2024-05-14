using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using UPB.PatientEntities.Managers.Exceptions;
using UPB.PatientEntities.Models;

namespace UPB.PatientEntities.Managers
{
    public class PatientManager
    {
        private readonly string _filePath;
        private readonly string _fileLog;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<int, Patient> _patients = new Dictionary<int, Patient>();

        public PatientManager(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;

            try
            {
                _filePath = _configuration.GetSection("Paths").GetSection("txt").Value;
            }
            catch (Exception ex)
            {
                PatientExceptions bsEx = new PatientExceptions(ex.Message);
                Log.Error(bsEx.GetMensajeforLogs("Configuring file Path"));
                throw bsEx;
            }

            LoadPatientsFromFile();
            _httpClient = httpClientFactory.CreateClient("Practice3Client");
        }

        public async Task<Patient> CreatePatientAsync(Patient patientToCreate)
        {
            // Crear la solicitud para Practice3
            var response = await _httpClient.PostAsJsonAsync("api/PatientController1/GeneratePatientCode", new
            {
                CI = patientToCreate.CI,
                Name = patientToCreate.Name,
                LastName = patientToCreate.LastName,
                BloodGroup = patientToCreate.GetRandomBloodGroup(),
            }) ;
            response.EnsureSuccessStatusCode();

            // Obtener el código de paciente de la respuesta
            var result = await response.Content.ReadFromJsonAsync<PatientCodeResponse>();
            patientToCreate.Code = result.PatientCode;

            _patients.Add(patientToCreate.CI, patientToCreate);
            EscribirPacientesEnArchivo();
            return patientToCreate;
        }

        public string UpdatePatient(int ci, string name, string lastName)
        {
            try
            {
                _patients[ci].Name = name;
                _patients[ci].LastName = lastName;
                EscribirPacientesEnArchivo();
                return "Datos actualizados";
            }
            catch (Exception ex)
            {
                PatientExceptions bsEx = new PatientExceptions(ex.Message);
                Log.Error(bsEx.GetMensajeforLogs("updatebyCiApellido"));
                throw bsEx;
            }
        }

        public void DeletePatient(int ci)
        {
            if (_patients.TryGetValue(ci, out Patient patient))
            {
                _patients.Remove(ci);
                EscribirPacientesEnArchivo();
            }
            else
            {
                PatientExceptions bsEx = new PatientExceptions();
                Log.Error(bsEx.GetMensajeforLogs("Remove by Ci"));
                throw new Exception("Error while removing CI");
            }
        }

        public Patient GetPatientByCI(int ci)
        {
            try
            {
                return _patients[ci];
            }
            catch (Exception ex)
            {
                PatientExceptions bsEx = new PatientExceptions(ex.Message);
                Log.Error(bsEx.GetMensajeforLogs("obtenerPacienteCI"));
                throw bsEx;
            }
        }

        public Dictionary<int, Patient> GetAll()
        {
            return _patients;
        }

        public void LoadPatientsFromFile()
        {
            try
            {
                var lines = File.ReadAllLines(_filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    var patient = new Patient()
                    {
                        Name = parts[0],
                        LastName = parts[1],
                        CI = int.Parse(parts[2]),
                        BloodGroup = parts[3],
                        Code = parts.Length > 4 ? parts[4] : null // Verifica si el código de paciente está presente
                    };
                    _patients.Add(int.Parse(parts[2]), patient);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void EscribirPacientesEnArchivo()
        {
            using (StreamWriter writer = new StreamWriter(_filePath, false)) // El segundo parámetro 'false' significa que sobreescribirá el archivo si existe
            {
                foreach (Patient paciente in _patients.Values)
                {
                    string linea = $"{paciente.Name},{paciente.LastName},{paciente.CI},{paciente.BloodGroup},{paciente.Code}";
                    writer.WriteLine(linea);
                }
            }
        }

        public class PatientCodeResponse
        {
            public string PatientCode { get; set; }
        }
    }
}
