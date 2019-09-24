using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Balance.Controllers
{
    public class Inputs
    {
        public int in1 { get; set; }
        public int in2 { get; set; }
        public int in3 { get; set; }
        public int in4 { get; set; }
        public int in5 { get; set; }
    }

    public enum EventType
    {
        In = 1,
        Out = 2
    }
    public class Event
    {
        public int id { get; set; }

        public DateTime? dataHoraEvento { get; set; }

        public EventType tipoEvento { get; set; }

        [Required]
        public string nome { get; set; }

        [Required]
        public string msg { get; set; }

        public string cor { get; set; }
    }

    public class Outputs
    {
        public int out1 { get; set; }
        public int out2 { get; set; }
        public int out3 { get; set; }
        public int out4 { get; set; }
        public int out5 { get; set; }

    }

    public class PWM
    {
        public int canal { get; set; }
        public int potencia { get; set; }
    }


    [Route("api/command")]
    public class CommandController : Controller
    {
        readonly bool isMock;

        readonly PocContext context;

        static readonly HttpClient client = new HttpClient();
        private readonly string nodeReadApi = "http://localhost:1880";

        private readonly string InputsGetApi = "/inputs";
        private readonly string OutputsPostApi = "/outputs";
        private readonly string PanelPostApi = "/painel";
        private readonly string PWMPostApi = "/PWM";


        public CommandController(PocContext context, IConfiguration configuration)
        {
            this.context = context;
            isMock = configuration.GetValue<bool>("IsMock");
        }

        private string BuildApiUrl(string methodApi)
        {
            return $"{nodeReadApi}{methodApi}";
        }

        [HttpGet("inputs")]
        public async Task<Inputs> GetInputsAsync()
        {

            Inputs inputs = null;


            if (isMock)
            {
                inputs = new Inputs
                {
                    in1 = GetRandomNumber(),
                    in2 = GetRandomNumber(),
                    in3 = GetRandomNumber(),
                    in4 = GetRandomNumber(),
                    in5 = GetRandomNumber(),
                };
            }
            else
            {
                HttpResponseMessage response = await client.GetAsync(BuildApiUrl(InputsGetApi));
                if (response.IsSuccessStatusCode)
                {
                    var dataAsString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Inputs>(dataAsString);
                }
            }

            return inputs;
        }

        [HttpGet("event/in")]
        public async Task<List<Event>> ListEvents()
        {
            return await context.Events.Where(item => item.tipoEvento == EventType.In)
                                       .OrderByDescending(item => item.dataHoraEvento)
                                       .Take(5)
                                       .ToListAsync();
        }

        [HttpGet("event/in/last")]
        public async Task<Event> GetLastEventIn()
        {
            var test = await context.Events.Where(item => item.tipoEvento == EventType.In)
                                       .OrderBy(item => item.dataHoraEvento)
                                       .LastOrDefaultAsync();
            return test;
        }

        [HttpGet("event/out/last")]
        public async Task<Event> GetLastEventOut()
        {
            return await context.Events.Where(item => item.tipoEvento == EventType.Out)
                                       .OrderBy(item => item.dataHoraEvento)
                                       .LastOrDefaultAsync();
        }


        [HttpPost("event")]
        public async Task ProcessEvent(Event payload)
        {
            payload.dataHoraEvento = DateTime.Now;
            payload.tipoEvento = EventType.In;

            await context.Events.AddAsync(payload);
            await context.SaveChangesAsync();

            var panelEvent = new Event
            {
                dataHoraEvento = DateTime.Now,
                nome = "Display",
                msg = payload.msg == "ola 123" ? "autorizado" : "negado",
                cor = payload.msg == "ola 123" ? "verde" : "vermelho",
                tipoEvento = EventType.Out
            };

            await context.Events.AddAsync(panelEvent);
            await context.SaveChangesAsync();

            if (!isMock)
            {
                var dataAsString = JsonConvert.SerializeObject(panelEvent);
                var content = new StringContent(dataAsString);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync(BuildApiUrl(PanelPostApi), content);
                response.EnsureSuccessStatusCode();
            }
        }

        [HttpPost("outputs")]
        public async Task SendOutputsAsync(Outputs outputs)
        {
            if (!isMock)
            {
                var dataAsString = JsonConvert.SerializeObject(outputs);
                var content = new StringContent(dataAsString);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync(BuildApiUrl(OutputsPostApi), content);
                response.EnsureSuccessStatusCode();

            }
        }

        [HttpPost("pwm")]
        public async Task SendPWMAsync(PWM pwm)
        {
            if (!isMock)
            {

                var dataAsString = JsonConvert.SerializeObject(pwm);
                var content = new StringContent(dataAsString);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync(BuildApiUrl(PWMPostApi), content);
                response.EnsureSuccessStatusCode();
            }
        }


        private int GetRandomNumber()
        {
            Random random = new Random();
            return random.Next(0, 2);
        }
    }
}
