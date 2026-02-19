using System;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace rest_client
{
    class Program
    {
        private static string AiSvcEndpoint;
        private static string AiSvCKey;
        static async Task Main(string[] args)
        {
            try
            {
                // Obtener configuración desde AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                AiSvcEndpoint = configuration["AIServicesEndpoint"];
                AiSvCKey = configuration["AIServicesKey"];

                // Obtener entrada del usuario (hasta que escriba "quit")
                string userText = "";
                while (!userText.Equals("quit", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("Enter some text ('quit' to stop)");
                    userText = Console.ReadLine();
                    if (!userText.Equals("quit", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Llamar a la función para detectar el idioma
                        await GetLanguage(userText);
                        // Llamar a la función para detectar el sentimiento
                        await GetSentiment(userText);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task GetLanguage(string text)
        {
            try
            {
                // Construir el cuerpo de la solicitud JSON
                JObject jsonBody = new JObject(
                    // Crear una colección de documentos (usaremos uno, pero podrían ser más)
                    new JProperty("documents",
                    new JArray(
                        new JObject(
                            // Cada documento necesita un ID único y texto
                            new JProperty("id", 1),
                            new JProperty("text", text)
                    ))));

                // Codificar como UTF-8
                UTF8Encoding utf8 = new UTF8Encoding(true, true);
                byte[] encodedBytes = utf8.GetBytes(jsonBody.ToString());

                // Mostrar el JSON que se enviará al servicio
                Console.WriteLine(utf8.GetString(encodedBytes, 0, encodedBytes.Length));

                // Realizar una solicitud HTTP a la interfaz REST
                var client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Agregar la clave de autenticación al encabezado
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AiSvCKey);

                // Usar el endpoint para acceder a la API de idiomas de Text Analytics
                var uri = AiSvcEndpoint + "text/analytics/v3.1/languages?" + queryString;

                // Enviar la solicitud y obtener la respuesta
                HttpResponseMessage response;
                using (var content = new ByteArrayContent(encodedBytes))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(uri, content);
                }

                // Si la llamada fue exitosa, obtener la respuesta
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Mostrar la respuesta JSON completa (para visualizarla)
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JObject results = JObject.Parse(responseContent);
                    Console.WriteLine(results.ToString());

                    // Extraer el nombre del idioma detectado para cada documento
                    foreach (JObject document in results["documents"])
                    {
                        Console.WriteLine("\nLanguage: " + (string)document["detectedLanguage"]["name"]);
                    }
                }
                else
                {
                    // Algo salió mal, mostrar la respuesta completa
                    Console.WriteLine(response.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Método que detecte polaridad de un texto usando Azure Text Analytics
        static async Task GetSentiment(string text)
        {
            try
            {
                // Construir el cuerpo de la solicitud JSON
                JObject jsonBody = new JObject(
                    // Crear una colección de documentos (usaremos uno, pero podrían ser más)
                    new JProperty("documents",
                    new JArray(
                        new JObject(
                            // Cada documento necesita un ID único y texto
                            new JProperty("id", 1),
                            new JProperty("text", text)
                    ))));

                // Codificar como UTF-8
                UTF8Encoding utf8 = new UTF8Encoding(true, true);
                byte[] encodedBytes = utf8.GetBytes(jsonBody.ToString());

                // Mostrar el JSON que se enviará al servicio
                Console.WriteLine(utf8.GetString(encodedBytes, 0, encodedBytes.Length));

                // Realizar una solicitud HTTP a la interfaz REST
                var client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Agregar la clave de autenticación al encabezado
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AiSvCKey);

                // Usar el endpoint para acceder a la API de análisis de sentimientos de Text Analytics
                var uri = AiSvcEndpoint + "text/analytics/v3.1/sentiment?" + queryString;

                // Enviar la solicitud y obtener la respuesta
                HttpResponseMessage response;
                using (var content = new ByteArrayContent(encodedBytes))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(uri, content);
                }

                // Si la llamada fue exitosa, obtener la respuesta
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Mostrar la respuesta JSON completa (para visualizarla)
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JObject results = JObject.Parse(responseContent);
                    Console.WriteLine(results.ToString());

                    // Extraer el sentimiento detectado para cada documento
                    foreach (JObject document in results["documents"])
                    {
                        Console.WriteLine("\nSentiment: " + (string)document["sentiment"]);
                    }
                }
                else
                {
                    // Algo salió mal, mostrar la respuesta completa
                    Console.WriteLine(response.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
