namespace ServiceDeskDESIMVC.Models
{
    public class HorarioViewModel
    {
        public int DiaSemana { get; set; }
        public bool EsLaboral { get; set; }
        public string HoraInicio { get; set; } // "HH:mm"
        public string HoraFin { get; set; }    // "HH:mm"
    }
}
