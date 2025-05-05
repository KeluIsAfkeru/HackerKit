using System;

namespace HackerKit.Models
{
	public enum ToastType
	{
		Info,
		Success,
		Warning,
		Error
	}

	public class ToastModel
	{
		public string Message { get; set; }
		public ToastType Type { get; set; }
		public int Duration { get; set; } = 3000;
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public Guid Id { get; set; } = Guid.NewGuid();
	}
}