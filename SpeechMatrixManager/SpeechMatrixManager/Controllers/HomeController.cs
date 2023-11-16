using Microsoft.AspNetCore.Mvc;
using SpeechMatrixManager.Repository.Home;
using SpeechMatrixManager.Repository.WebSocketManager;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpeechMatrixManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {

        private readonly IHomeService _IHomeService;
        private readonly SocketManager Manager;
      

        public HomeController(IHomeService HomeService, SocketManager _manager)
        {
            _IHomeService = HomeService;
            Manager = _manager;
        }
        //// GET: api/<HomeController>
        [HttpGet("StartRecognition")]
        public async Task<IActionResult> StartWebSocket(string FilePath)
        {

            try
            {
                // Read audio file as byte array
                FilePath = "D://SpeechMatrix//AudioFiles//example.wav";
                var audioData = _IHomeService.ReadAudio(FilePath);

                var response = await Manager.StartWebSocketAsync(audioData);
                return Ok(response);
                //return Ok("WebSocket communication started.");
            }
            catch (Exception e)
            {
                return BadRequest("WebSocket communication failed.");
            }

        }
        // POST api/<HomeController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<HomeController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<HomeController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
