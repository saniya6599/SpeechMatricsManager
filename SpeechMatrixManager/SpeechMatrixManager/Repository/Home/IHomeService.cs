using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechMatrixManager.Repository.Home
{
   public interface IHomeService
    {
        string GetTranscription();
        byte[] ReadAudio(string FilePath);
    }
}
