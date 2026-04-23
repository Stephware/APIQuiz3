using APIQuiz3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace APIQuiz3.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("TaskPolicy")]
    public class TasksController : ControllerBase
    {
        private static List<TaskItem> tasks = new List<TaskItem>
        {
                new TaskItem { Id = 1, Title = "Finish the Vet Clinic System", IsCompleted = false },
                new TaskItem { Id = 2, Title = "Make Parents Proud", IsCompleted = true },
                new TaskItem { Id = 3, Title = "Review for ELECT2", IsCompleted = false }
        };

        private static int counter = 1;

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult GetAll() => Ok(tasks);

        [HttpGet("{id}")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Get(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            return task == null ? NotFound() : Ok(task);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Create(TaskItem item)
        {
            item.Id = counter++;
            tasks.Add(item);
            return Ok(item);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, TaskItem updated)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) return NotFound();

            task.Title = updated.Title;
            task.IsCompleted = updated.IsCompleted;

            return Ok(task);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) return NotFound();

            tasks.Remove(task);
            return Ok("Deleted");
        }
    }
}