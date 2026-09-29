using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace AppAjuntament.Models.Base.Geo
{
    [Table("dades_geografiques")]
    public class DadesGeografiques
    {
        public int Id { get; set; }
        public int? NombreHabitants { get; set; }
        public decimal? Extensio { get; set; }
        public int? Altitud { get; set; }
        public double LocalitzacioLatitud { get; set; }
        public double LocalitzacioLongitud { get; set; }
        public double CentreMunicipalLatitud { get; set; }
        public double CentreMunicipalLongitud { get; set; }

        [NotMapped]
        public Point Localitzacio
        {
            get => new Point(LocalitzacioLatitud, LocalitzacioLongitud);
            set
            {
                LocalitzacioLatitud = value.X;
                LocalitzacioLongitud = value.Y;
            }
        }

        [NotMapped]
        public Point CentreMunicipal
        {
            get => new Point(CentreMunicipalLatitud, CentreMunicipalLongitud);
            set
            {
                CentreMunicipalLatitud = value.X;
                CentreMunicipalLongitud = value.Y;
            }
        }
    }
}
