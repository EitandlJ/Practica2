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
using Newtonsoft.Json;

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

       public async Task<Patient> CreatePatient(Patient patient)
        {
            // Generamos el código de paciente llamando al servicio del Practice 3
            string patientCode = await GeneratePatientCodeAsync(patient.Name, patient.LastName, patient.CI);
            patient.BloodGroup = patient.GetRandomBloodGroup();
            // Asignamos el código de paciente generado
            patient.Code = patientCode;

            // Agregamos el paciente a la lista
            _patients.Add(patient.CI, patient);
            EscribirPacientesEnArchivo();
            // Retornamos el paciente creado
            return patient;
        }

        public async Task<string> GeneratePatientCodeAsync(string name, string lastName, int ci)
        {
            // Construimos la URL del servicio en Practice 3
            string practice3BaseUrl = _configuration.GetSection("Practice3BaseUrl").Value;
            string generateCodeEndpoint = "api/PatientController1";

            // Creamos el objeto que contiene la información del paciente
            var patientInfo = new { Name = name, LastName = lastName, CI = ci };

            // Serializamos el objeto a JSON
            var jsonPatientInfo = JsonConvert.SerializeObject(patientInfo);

            // Creamos el contenido de la solicitud HTTP
            var content = new StringContent(jsonPatientInfo, System.Text.Encoding.UTF8, "application/json");

            // Realizamos la solicitud HTTP POST al servicio de Practice 3
            var response = await _httpClient.PostAsync($"{practice3BaseUrl}/{generateCodeEndpoint}", content);

            // Verificamos si la solicitud fue exitosa
            if (response.IsSuccessStatusCode)
            {
                // Leemos la respuesta como texto
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserializamos la respuesta para obtener el código de paciente
                var responseObject = JsonConvert.DeserializeAnonymousType(responseContent, new { PatientCode = "" });


                // Retornamos el código de paciente generado
                return responseObject.PatientCode;
            }
            else
            {
                // Si la solicitud no fue exitosa, lanzamos una excepción o manejamos el error según sea necesario
                throw new HttpRequestException($"Error al generar el código de paciente: {response.StatusCode}");
            }
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
