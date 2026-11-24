using System.ComponentModel.DataAnnotations;

namespace RestFulWebService.Models;

public class WeatherForecastRequest
{
    [Required]
    public DateOnly Date { get; set; }

    [Range(-80, 60, ErrorMessage = "La temperatura debe estar entre -80 y 60 °C.")]
    public int TemperatureC { get; set; }

    [Required(ErrorMessage = "Debes seleccionar un estado del clima.")]
    [StringLength(60, MinimumLength = 2)]
    public string Summary { get; set; } = string.Empty;
}
