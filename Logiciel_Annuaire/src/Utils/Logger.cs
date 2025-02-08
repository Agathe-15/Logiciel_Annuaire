using System;
using System.IO;


namespace Logiciel_Annuaire.src.Utils
{
    public static class Logger
    {
        private static readonly string LogDirectory = "Logs"; // 📂 Dossier des logs
        private static readonly string LogFilePath; // 📌 Chemin du fichier log

        static Logger()
        {
            // 📌 Générer un nom de fichier unique basé sur la date et l'heure
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            LogFilePath = Path.Combine(LogDirectory, $"log_{timestamp}.txt");

            // 📌 Créer le dossier des logs s'il n'existe pas
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            // 📌 Écrire une ligne d'ouverture pour identifier la session
            File.AppendAllText(LogFilePath, $"==== DÉBUT DE SESSION {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===={Environment.NewLine}");
        }

        public static void Log(string message)
        {
            try
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                Console.WriteLine(logMessage); // ✅ Affiche aussi dans la console
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur de log : {ex.Message}");
            }
        }

        public static void LogError(Exception ex, string context = "")
        {
            try
            {
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERREUR {context}: {ex.Message}\n{ex.StackTrace}";
                Console.WriteLine(errorMessage);
                File.AppendAllText(LogFilePath, errorMessage + Environment.NewLine);
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"❌ Impossible d'écrire dans le fichier log : {logEx.Message}");
            }
        }
    }
}
