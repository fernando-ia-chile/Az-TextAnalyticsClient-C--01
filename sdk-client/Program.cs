using System;
using Azure;
using Microsoft.Extensions.Configuration;
// Agregar la referencia al paquete Azure.AI.TextAnalytics para usar el cliente de Text Analytics
using Azure.AI.TextAnalytics;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace sdk_client
{
    class Program
    {
        // Variables estáticas para almacenar el endpoint y la clave del servicio de IA de Azure
        private static string AISvcEndpoint;
        private static string AISvcKey;

        static async Task Main(string[] args)
        {
            try
            {
                // Obtener la configuración desde el archivo appsettings.json
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                AISvcEndpoint = configuration["AIServicesEndpoint"];
                AISvcKey = configuration["AIServicesKey"];

                // Solicitar texto al usuario en un bucle hasta que escriba "quit"
                string userText = "";
                while (!userText.Equals("quit", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("\nEnter some text ('quit' to stop)");
                    userText = Console.ReadLine();
                    if (!userText.Equals("quit", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Llamar a la función para detectar el idioma del texto ingresado
                        string language = GetLanguage(userText);
                        Console.WriteLine("Language: " + language);

                        // Llamar a la función para detectar la polaridad del texto ingresado
                        string sentiment =  GetSentiment(userText);
                        Console.WriteLine("Sentiment: " + sentiment);

                        // Llamar a la función para detectar las frases clave del texto ingresado
                        string keyPhrases = await GetKeyPhrases(userText);
                        Console.WriteLine("Key Phrases: " + keyPhrases);

                        // // Llamar a la función para detectar las entidades nombradas del texto ingresado
                        string namedEntities = await GetNamedEntities(userText);
                        Console.WriteLine("Named Entities: " + namedEntities);

                        // Llamar a la función para detectar las entidades de información personal (PII) del texto ingresado
                        string piiEntities = await GetPIIEntities(userText);
                        Console.WriteLine("PII Entities: " + piiEntities);

                        // Llamar a la función para obtener el resumen extractivo del texto ingresado
                        string extractedSummary = await GetExtractedSummary(userText);
                        Console.WriteLine("Extracted Summary: " + extractedSummary);

                        
                    }
                }
            }
            catch (Exception ex)
            {
                // Mostrar cualquier error que ocurra durante la ejecución
                Console.WriteLine(ex.Message);
            }
        }

        // Método DetectLanguage que detecta el idioma de un texto usando Azure Text Analytics
        static string GetLanguage(string text)
        {
            // Crear el cliente de Text Analytics usando el endpoint y la clave de autenticación
            AzureKeyCredential credentials = new AzureKeyCredential(AISvcKey);
            Uri endpoint = new Uri(AISvcEndpoint);
            var client = new TextAnalyticsClient(endpoint, credentials);

            // Llamar al servicio para detectar el idioma y retornar su nombre
            //DetectedLanguage detectedLanguage = client.DetectLanguage(text); // sincrono
            DetectedLanguage detectedLanguage = client.DetectLanguageAsync(text).Result; // asincrono
            return detectedLanguage.Name;
        }


        // Método AnalyzeSentiment que detecte polaridad de un texto usando Azure Text Analytics
         static string GetSentiment(string text)
        {
            // Crear el cliente de Text Analytics usando el endpoint y la clave de autenticación
            AzureKeyCredential credentials = new AzureKeyCredential(AISvcKey);
            Uri endpoint = new Uri(AISvcEndpoint);
            var client = new TextAnalyticsClient(endpoint, credentials);

            // Llamar al servicio para detectar la polaridad y retornar su nombre
            //DocumentSentiment documentSentiment = client.AnalyzeSentiment(text); sincrono
            DocumentSentiment documentSentiment = client.AnalyzeSentimentAsync(text).Result; // asincrono
            return documentSentiment.Sentiment.ToString();
        }

        // Metodo ExtractKeyPhrases (Extracción de frases clave) de un texto usando Azure Text Analytics
        static async Task<string> GetKeyPhrases(string text)
        {
            // Crear el cliente de Text Analytics usando el endpoint y la clave de autenticación
            AzureKeyCredential credentials = new AzureKeyCredential(AISvcKey);
            Uri endpoint = new Uri(AISvcEndpoint);
            var client = new TextAnalyticsClient(endpoint, credentials);

            // Llamar al servicio para detectar las frases clave y retornar una cadena con las frases separadas por comas
            //KeyPhraseCollection keyPhrases = client.ExtractKeyPhrases(text); // sincrono
            KeyPhraseCollection keyPhrases = await client.ExtractKeyPhrasesAsync(text);  // asincrono
            return string.Join(", ", keyPhrases);
        }

        // Metodo RecognizeEntities (Reconocimiento de entidades) nombradas de un texto usando Azure Text Analytics
        static async Task<string> GetNamedEntities(string text)
        {
            // Crear el cliente de Text Analytics usando el endpoint y la clave de autenticación
            AzureKeyCredential credentials = new AzureKeyCredential(AISvcKey);
            Uri endpoint = new Uri(AISvcEndpoint);
            var client = new TextAnalyticsClient(endpoint, credentials);

            // Llamar al servicio para detectar las entidades nombradas y retornar una cadena con las entidades separadas por comas
            var response = await client.RecognizeEntitiesAsync(text);
            var entities = response.Value;
            return string.Join(", ", entities.Select(e => e.Text));
        }

        // Metodo RecognizePiiEntities (Detección de información personal PII) de un texto usando Azure Text Analytics
        static async Task<string> GetPIIEntities(string text) 
        {
            // Crear el cliente de Text Analytics usando el endpoint y la clave de autenticación
            AzureKeyCredential credentials = new AzureKeyCredential(AISvcKey);
            Uri endpoint = new Uri(AISvcEndpoint);
            var client = new TextAnalyticsClient(endpoint, credentials);

            // Llamar al servicio para detectar las entidades de información personal (PII) y retornar una cadena con las entidades separadas por comas
            var response = await client.RecognizePiiEntitiesAsync(text);
            var entities = response.Value;
            return string.Join(", ", entities.Select(e => e.Text));
        }


       
        // Método ExtractiveSummarizeAsync de resumen extractivo
        static async Task<string> GetExtractedSummary(string text)
        {
            // detectar idioma para configurar el documento de entrada
            string language = GetLanguageISO(text);
            Console.WriteLine("Detected language: " + language);

            // Crear cliente
            AzureKeyCredential credentials = new AzureKeyCredential(AISvcKey);
            Uri endpoint = new Uri(AISvcEndpoint);
            var client = new TextAnalyticsClient(endpoint, credentials);

            // Opciones de resumen
            var options = new ExtractiveSummarizeOptions()
            {
                MaxSentenceCount = 10 // limitar a 10 frases
            };

            // Documento con ID y lenguaje explícito
            var documents = new List<TextDocumentInput>
            {
                new TextDocumentInput("1", text) { Language = language }
            };

            // Ejecutar operación de resumen
            var operation = await client.ExtractiveSummarizeAsync(
                WaitUntil.Completed,
                documents,
                options
            );

            // Recorrer resultados
            var summaryParts = new List<string>();
            await foreach (var documentsInPage in operation.Value)
            {
                foreach (var documentResult in documentsInPage)
                {
                    foreach (var sentence in documentResult.Sentences)
                    {
                        summaryParts.Add(sentence.Text);
                    }
                }
            }

            return string.Join(" ", summaryParts);
        }

        // Método DetectLanguage - devuelve código ISO (ej: "es", "en", "fr", "de", etc.)
        static string GetLanguageISO(string text)
        {
            AzureKeyCredential credentials = new AzureKeyCredential(AISvcKey);
            Uri endpoint = new Uri(AISvcEndpoint);
            var client = new TextAnalyticsClient(endpoint, credentials);

            // Detectar idioma (asincrónico con .Result para simplificar)
            DetectedLanguage detectedLanguage = client.DetectLanguageAsync(text).Result;
            return detectedLanguage.Iso6391Name; 
        }
    }
}
