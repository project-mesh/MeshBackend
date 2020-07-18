namespace MeshBackend.Models
{
    public class TaskTag
    {
        public int TaskId { get; set; }
        
        public string tag { get; set; }
        
        public Task Task { get; set; }
    }
}