using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication4.Services;

namespace WebApplication4.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly WebSocketMessageStore _wsStore;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            WebSocketMessageStore wsStore)
        {
            _logger = logger;
            _wsStore = wsStore;
        }

        // �������������һ�� WebSocket ��Ϣ
        [HttpGet("lastws")]
        public IActionResult GetLastWebSocketMessage()
        {
            var (message, ts) = _wsStore.Get();
            if (message == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "��δ�յ��κ� WebSocket ��Ϣ��"
                });
            }

            return Ok(message);
        }
    }
}
