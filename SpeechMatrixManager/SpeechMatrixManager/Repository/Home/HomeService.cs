using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechMatrixManager.Repository.Home
{
    public class HomeService : IHomeService
    {
        public string GetTranscription()
        {

            try
            {

            }
            catch (Exception ex)
            {

            }
            return string.Empty;

        }

        public byte[] ReadAudio(string FilePath)
        {
            byte[] audio = null;
            try
            {
                audio = File.ReadAllBytes(FilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while reading Audio strem!");
            }
            return audio;
        }
    }
}
