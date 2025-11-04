using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    public enum TaskStatus { Todo, InProgress, Blocked, Done }
    public enum TaskPriority { Low, Medium, High, Urgent }

    public class TaskItem
    {
        public int Id { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Todo;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        // simple text assignee for now; later you can FK to AppUser
        [MaxLength(100)]
        public string? Assignee { get; set; }

        // Project relation
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(200)]
        public string? TagsCsv { get; set; } // comma separated, optional
    }
}
