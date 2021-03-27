namespace EventSourcingTaskApp.Controllers
{
    using EventSourcingTaskApp.Core;
    using EventSourcingTaskApp.Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;
    using MassTransit;
    using Microsoft.AspNetCore.Mvc;
    using Faker;
    using Abstractions;

    [Route("api/tasks/{id}")]
    [ApiController]
    [Consumes("application/x-www-form-urlencoded")]
    public class TasksController : ControllerBase
    {
        private readonly AggregateRepository _aggregateRepository;
        private readonly IPublishEndpoint _bus;

        public TasksController(AggregateRepository aggregateRepository, IPublishEndpoint bus)
        {
            _aggregateRepository = aggregateRepository;
            _bus = bus;
        }

        [HttpPost, Route("create")]
        public async Task<IActionResult> Create(Guid id, [FromForm] string title)
        {
            var aggregate = await _aggregateRepository.LoadAsync<Core.Task>(id);
            aggregate.Create(id, title, "Grant Test");

            await _aggregateRepository.SaveAsync(aggregate);

            return Ok();
        }

        [HttpPatch, Route("assign")]
        public async Task<IActionResult> Assign(Guid id, [FromForm] string assignedTo)
        {
            var aggregate = await _aggregateRepository.LoadAsync<Core.Task>(id);
            aggregate.Assign(assignedTo, "Grant Test");

            await _aggregateRepository.SaveAsync(aggregate);

            return Ok();
        }

        [HttpPatch, Route("move")]
        public async Task<IActionResult> Move(Guid id, [FromForm] BoardSections section)
        {
            var aggregate = await _aggregateRepository.LoadAsync<Core.Task>(id);
            aggregate.Move(section, "Grant Test");

            await _aggregateRepository.SaveAsync(aggregate);

            return Ok();
        }

        [HttpPatch, Route("complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            var aggregate = await _aggregateRepository.LoadAsync<Core.Task>(id);
            aggregate.Complete("Grant Test");

            await _aggregateRepository.SaveAsync(aggregate);

            return Ok();
        }

        [HttpGet, Route("add-event")]
        public async Task<IActionResult> AddEvent()
        {
            var evt = new CreateEmail
            {
                Title = $"{Faker.Name.First()} - sending emails",
                Message = Faker.Lorem.Paragraph(5),
                Recipient = Faker.Name.FullName(NameFormats.StandardWithMiddle)
            };
            await _bus.Publish<CreateEmail>(evt);

            return Ok("Event added");
        }
    }
}