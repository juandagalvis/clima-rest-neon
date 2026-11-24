using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestFulWebService.Data;
using RestFulWebService.Models;

namespace RestFulWebService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController(WeatherDbContext db) : ControllerBase
{
    // GET: /api/weatherforecast
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WeatherForecastResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WeatherForecastResponse>>> GetAll()
    {
        var forecasts = await db.Forecasts
            .AsNoTracking()
            .OrderBy(forecast => forecast.Date)
            .ToListAsync();

        return Ok(forecasts.Select(ToResponse));
    }

    // GET: /api/weatherforecast/1
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WeatherForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherForecastResponse>> GetById(int id)
    {
        var forecast = await db.Forecasts.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        return forecast is null ? NotFound() : Ok(ToResponse(forecast));
    }

    // POST: /api/weatherforecast
    [HttpPost]
    [ProducesResponseType(typeof(WeatherForecastResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WeatherForecastResponse>> Create(WeatherForecastRequest request)
    {
        var forecast = new WeatherForecast
        {
            Date = request.Date,
            TemperatureC = request.TemperatureC,
            Summary = request.Summary.Trim()
        };

        db.Forecasts.Add(forecast);
        await db.SaveChangesAsync();

        var response = ToResponse(forecast);
        return CreatedAtAction(nameof(GetById), new { id = forecast.Id }, response);
    }

    // PUT: /api/weatherforecast/1
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, WeatherForecastRequest request)
    {
        var forecast = await db.Forecasts.FindAsync(id);
        if (forecast is null)
        {
            return NotFound();
        }

        forecast.Date = request.Date;
        forecast.TemperatureC = request.TemperatureC;
        forecast.Summary = request.Summary.Trim();

        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /api/weatherforecast/1
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var forecast = await db.Forecasts.FindAsync(id);
        if (forecast is null)
        {
            return NotFound();
        }

        db.Forecasts.Remove(forecast);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static WeatherForecastResponse ToResponse(WeatherForecast forecast)
    {
        var temperatureF = (int)Math.Round((forecast.TemperatureC * 9.0 / 5.0) + 32);
        return new WeatherForecastResponse(
            forecast.Id,
            forecast.Date,
            forecast.TemperatureC,
            temperatureF,
            forecast.Summary);
    }
}
