using BalancePoc.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BalancePoc.Controllers
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
            isMock = false;
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
                    in3 = 1,
                    in4 = GetRandomNumber(),
                    in5 = GetRandomNumber(),
                };
            }
            else
            {
                HttpResponseMessage response = await client.GetAsync(BuildApiUrl(InputsGetApi));
                if (response.IsSuccessStatusCode)
                {
                    inputs = await response.Content.ReadAsAsync<Inputs>();
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
            return await context.Events.Where(item => item.tipoEvento == EventType.In)
                                       .OrderBy(item => item.dataHoraEvento)
                                       .LastOrDefaultAsync();
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
                HttpResponseMessage response = await client.PostAsJsonAsync(BuildApiUrl(PanelPostApi), panelEvent);
                response.EnsureSuccessStatusCode();
            }
        }

        [HttpPost("outputs")]
        public async Task SendOutputsAsync(Outputs outputs)
        {
            if (!isMock)
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(BuildApiUrl(OutputsPostApi), outputs);
                response.EnsureSuccessStatusCode();
            }
        }

        [HttpPost("pwm")]
        public async Task SendPWMAsync(PWM pwm)
        {
            if (!isMock)
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(BuildApiUrl(PWMPostApi), pwm);
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
