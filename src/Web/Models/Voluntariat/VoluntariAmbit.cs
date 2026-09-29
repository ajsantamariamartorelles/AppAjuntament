namespace AppAjuntament.Models.Voluntariat
{
    public class VoluntariAmbit
    {
        public int Id { get; set; }
        public int VoluntariId { get; set; }
        public int AmbitVoluntariId { get; set; }
        public int? SubambitVoluntariId { get; set; }
        public DateTime DataAssignacio { get; set; } = DateTime.Now;
        public virtual Voluntari? Voluntari { get; set; }
        public virtual AmbitVoluntari? AmbitVoluntari { get; set; }
        public virtual SubambitVoluntari? SubambitVoluntari { get; set; }
    }
}
